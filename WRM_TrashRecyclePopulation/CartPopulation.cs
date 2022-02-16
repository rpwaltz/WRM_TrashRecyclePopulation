using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using OfficeOpenXml;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;
using System.Globalization;

namespace WRM_TrashRecyclePopulation
    {

    class CartPopulation
        {
        private FileInfo xlsxSNMasterlistFileInfo;

        public Dictionary<string, SNSpreadsheetRow> snMasterDictionary = new Dictionary<string, SNSpreadsheetRow>();

        public List<string> ignoreCartDuringPopulation = new List<string>();

        public static string xlsxSNMasterlistPath = @"C:\Users\rwaltz\Documents\SolidWaste\SN_MASTERLIST_Current.xlsx";

        private ExcelWorksheet worksheet;
        private int totalRowsWorksheet;


        public CartPopulation()
            {

            xlsxSNMasterlistFileInfo = new FileInfo(xlsxSNMasterlistPath);
            if (!xlsxSNMasterlistFileInfo.Exists)
                {
                throw new Exception(xlsxSNMasterlistPath + "does not exist");
                }

            ExcelPackage package = new ExcelPackage(xlsxSNMasterlistFileInfo);
            worksheet = package.Workbook.Worksheets[0];
            totalRowsWorksheet = worksheet.Dimension.End.Row;
            }

        public void populateCarts()
            {
            try
                {
                DateTime begin = DateTime.Now;
                DateTime beforeNow = DateTime.Now;
                DateTime justNow = DateTime.Now;
                TimeSpan timeDiff = justNow - beforeNow;
                double loopMillisecondsPast = 0;
                String logLine;
                int maxToProcess = 0;

                // populate all the addresses that are new
                WRMLogger.Logger.logMessageAndDeltaTime("populateSNMasterDictionary ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                populateSNMasterDictionary();
                WRMLogger.Logger.logMessageAndDeltaTime("addAddressesFromSNMasterList ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                addAddressesFromSNMasterList();
                WRMLogger.Logger.logMessageAndDeltaTime("addFirstTrashCarts ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                addFirstTrashCarts();
                WRMLogger.LogBuilder.AppendLine("addSecondTrashCarts");
                addSecondTrashCarts();
                WRMLogger.LogBuilder.AppendLine("addThirdTrashCarts");
                addThirdTrashCarts();
                WRMLogger.LogBuilder.AppendLine("addFirstRecyclingCarts");


                addFirstRecyclingCarts();
                WRMLogger.LogBuilder.AppendLine("addSecondRecyclingCarts");
                addSecondRecyclingCarts();

                WRMLogger.LogBuilder.AppendLine("addCurrentTrashCarts");
                addCurrentTrashCarts();
                WRMLogger.LogBuilder.AppendLine("addFirstRecyclingCarts");
                addCurrentRecyclingCarts();

                justNow = DateTime.Now;
                timeDiff = justNow - beforeNow;
                WRMLogger.LogBuilder.AppendLine("End Cart Population" + justNow.ToString("o", new CultureInfo("en-us")) + "Total MilliSeconds passed : " + timeDiff.TotalMilliseconds.ToString());
                //                WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref beforeNow, ref justNow, ref loopMillisecondsPast);
                WRMLogger.Logger.log();
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw ex;
                }
            }


        public void addCurrentTrashCarts()
            {
            try
                {
                WRMLogger.Logger.logMessageAndDeltaTime("addCurrentTrashCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                int cartCount = 0;
                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    if ((cartCount) > 0 && (cartCount % 1000 == 0))
                        {
                        WRMLogger.Logger.logMessageAndDeltaTime("addCurrentTrashCart Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        ++cartCount;

                        }
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;
                    try
                        {
                        if ((!String.IsNullOrEmpty(snSpreadsheetRow.CurrentTrashCartSN)) 
                            && (!snSpreadsheetRow.CurrentTrashCartSN.ToUpper().Equals("NO TRASH")))
                            {
                            Cart cart = new Cart();
  
                            cart.CartSerialNumber = snSpreadsheetRow.CurrentTrashCartSN;
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.CurrentTrashCartDeliveryDate))
                                {
                                if (validateDateString(snSpreadsheetRow.CurrentTrashCartDeliveryDate))
                                    {
                                    cart.SerialNumberReceivedDate = DateTime.Parse(snSpreadsheetRow.CurrentTrashCartDeliveryDate);
                                    }
                                else
                                    {
                                    WRMLogger.LogBuilder.AppendLine("Current Cart Date recieved is invalid " + snSpreadsheetRow.FirstTrashCartDeliveryDate);
                                    }
                                }
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SmallTrashCart) && snSpreadsheetRow.SmallTrashCart.Equals("1"))
                                {
                                cart.CartSize = "65 GALLON";
                                }
                            else
                                {
                                cart.CartSize = "90 GALLON";
                                }
                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.IsRecyclingCart = false;
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("Current Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
                            cart.CreateDate = DateTime.Now;
                            cart.CreateUser = "TrashRecyclePopulation";
                            cart.UpdateDate = DateTime.Now;
                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Current Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    }

                WRMLogger.Logger.logMessageAndDeltaTime("addCurrentTrashCart End Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw ex;
                }
            }

        public void addFirstTrashCarts()
            {
            try
                {
                WRMLogger.Logger.logMessageAndDeltaTime("addFirstTrashCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                int cartCount = 0;
                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    if ((cartCount) > 0 && (cartCount % 1000 == 0))
                        {
                        WRMLogger.Logger.logMessageAndDeltaTime("addFirstTrashCart Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        ++cartCount;

                        }
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;
                    try
                        {
                        if (!String.IsNullOrEmpty(snSpreadsheetRow.FirstTrashCartSN))
                            {
                            Cart cart = new Cart();
                            cart.CartSerialNumber = snSpreadsheetRow.FirstTrashCartSN;
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.FirstTrashCartDeliveryDate))
                                {
                                if (validateDateString(snSpreadsheetRow.FirstTrashCartDeliveryDate))
                                    {
                                    cart.SerialNumberReceivedDate = DateTime.Parse(snSpreadsheetRow.FirstTrashCartDeliveryDate);
                                    }
                                else
                                    {
                                    WRMLogger.LogBuilder.AppendLine("First Trash Cart Date recieved is invalid " + snSpreadsheetRow.FirstTrashCartDeliveryDate);
                                    }
                                }
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SmallTrashCart) && snSpreadsheetRow.SmallTrashCart.Equals("1"))
                                {
                                cart.CartSize = "65 GALLON";
                                }
                            else
                                {
                                cart.CartSize = "90 GALLON";
                                }
                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.IsRecyclingCart = false;
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("First Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
                            cart.CreateDate = DateTime.Now;
                            cart.CreateUser = "TrashRecyclePopulation";
                            cart.UpdateDate = DateTime.Now;
                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: First Trash Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    }

                WRMLogger.Logger.logMessageAndDeltaTime("addFirstTrashCart End Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                deleteAllCartsFromContext();
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw ex;
                }
            }


        public void addSecondTrashCarts()
            {
            try
                {
                WRMLogger.Logger.logMessageAndDeltaTime("secondTrashCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                int cartCount = 0;
                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    if ((cartCount) > 0 && (cartCount % 1000 == 0))
                        {
                        WRMLogger.Logger.logMessageAndDeltaTime("secondTrashCart Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        ++cartCount;

                        }
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;
                    try
                        {
                        if (!String.IsNullOrEmpty(snSpreadsheetRow.SecondTrashCartSN))
                            {
                            Cart cart = new Cart();
                            cart.CartSerialNumber = snSpreadsheetRow.SecondTrashCartSN;
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SecondTrashCartDeliveryDate))
                                {
                                if (validateDateString(snSpreadsheetRow.SecondTrashCartDeliveryDate))
                                    {
                                    cart.SerialNumberReceivedDate = DateTime.Parse(snSpreadsheetRow.SecondTrashCartDeliveryDate);
                                    }
                                else
                                    {
                                    WRMLogger.LogBuilder.AppendLine("Second Trash Cart Date recieved is invalid " + snSpreadsheetRow.SecondTrashCartDeliveryDate);
                                    }
                                }
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SmallTrashCart) && snSpreadsheetRow.SmallTrashCart.Equals("1"))
                                {
                                cart.CartSize = "65 GALLON";
                                }
                            else
                                {
                                cart.CartSize = "90 GALLON";
                                }
                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.IsRecyclingCart = false;
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("Second Trash Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
                            cart.CreateDate = DateTime.Now;
                            cart.CreateUser = "TrashRecyclePopulation";
                            cart.UpdateDate = DateTime.Now;
                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Second Trash Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    }

                WRMLogger.Logger.logMessageAndDeltaTime("secondFirstTrashCart End Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                deleteAllCartsFromContext();
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw ex;
                }
            }

        public void addThirdTrashCarts()
            {
            try
                {
                WRMLogger.Logger.logMessageAndDeltaTime("thirdTrashCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                int cartCount = 0;
                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    if ((cartCount) > 0 && (cartCount % 1000 == 0))
                        {
                        WRMLogger.Logger.logMessageAndDeltaTime("thirdTrashCart Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        ++cartCount;

                        }
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;
                    try
                        {
                        if (!String.IsNullOrEmpty(snSpreadsheetRow.ThirdTrashCartSN))
                            {
                            Cart cart = new Cart();
                            cart.CartSerialNumber = snSpreadsheetRow.ThirdTrashCartSN;
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.ThirdTrashCartDeliveryDate))
                                {
                                if (validateDateString(snSpreadsheetRow.ThirdTrashCartDeliveryDate))
                                    {
                                    cart.SerialNumberReceivedDate = DateTime.Parse(snSpreadsheetRow.ThirdTrashCartDeliveryDate);
                                    }
                                else
                                    {
                                    WRMLogger.LogBuilder.AppendLine("Third Trash Cart Date recieved is invalid " + snSpreadsheetRow.ThirdTrashCartDeliveryDate);
                                    }
                                }
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SmallTrashCart) && snSpreadsheetRow.SmallTrashCart.Equals("1"))
                                {
                                cart.CartSize = "65 GALLON";
                                }
                            else
                                {
                                cart.CartSize = "90 GALLON";
                                }
                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.IsRecyclingCart = false;
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("Third Trash Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
                            cart.CreateDate = DateTime.Now;
                            cart.CreateUser = "TrashRecyclePopulation";
                            cart.UpdateDate = DateTime.Now;
                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Third Trash Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    }

                WRMLogger.Logger.logMessageAndDeltaTime("thirdTrashCart End Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                deleteAllCartsFromContext();
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw ex;
                }
            }


        public void addFirstRecyclingCarts()
            {
            try
                {
                WRMLogger.Logger.logMessageAndDeltaTime("firstRecyclingCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                int cartCount = 0;
                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    if ((cartCount) > 0 && (cartCount % 1000 == 0))
                        {
                        WRMLogger.Logger.logMessageAndDeltaTime("firstRecyclingCart Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        ++cartCount;

                        }
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;
                    try
                        {
                        if (!String.IsNullOrEmpty(snSpreadsheetRow.FirstRecycleCartSN))
                            {
                            Cart cart = new Cart();
                            cart.CartSerialNumber = snSpreadsheetRow.FirstRecycleCartSN;
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.FirstRecycleCartDeliveryDate))
                                {
                                if (validateDateString(snSpreadsheetRow.FirstRecycleCartDeliveryDate))
                                    {
                                    cart.SerialNumberReceivedDate = DateTime.Parse(snSpreadsheetRow.FirstRecycleCartDeliveryDate);
                                    }
                                else
                                    {
                                    WRMLogger.LogBuilder.AppendLine("First Recycling Cart Date recieved is invalid " + snSpreadsheetRow.FirstRecycleCartDeliveryDate);
                                    }
                                }

                            cart.CartSize = "65 GALLON";

                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.IsRecyclingCart = true;
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("First Recycling Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
                            cart.CreateDate = DateTime.Now;
                            cart.CreateUser = "TrashRecyclePopulation";
                            cart.UpdateDate = DateTime.Now;
                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: First Recycling Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    }

                WRMLogger.Logger.logMessageAndDeltaTime("firstRecycleCart End Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                deleteAllCartsFromContext();
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw ex;
                }
            }

        public void addSecondRecyclingCarts()
            {
            try
                {
                WRMLogger.Logger.logMessageAndDeltaTime("secondRecyclingCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                int cartCount = 0;
                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    if ((cartCount) > 0 && (cartCount % 1000 == 0))
                        {
                        WRMLogger.Logger.logMessageAndDeltaTime("secondRecyclingCart Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        ++cartCount;

                        }
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;
                    try
                        {
                        if (!String.IsNullOrEmpty(snSpreadsheetRow.SecondRecycleCartSN))
                            {
                            Cart cart = new Cart();
                            cart.CartSerialNumber = snSpreadsheetRow.SecondRecycleCartSN;
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SecondRecycleCartDeliveryDate))
                                {
                                if (validateDateString(snSpreadsheetRow.SecondRecycleCartDeliveryDate))
                                    {
                                    cart.SerialNumberReceivedDate = DateTime.Parse(snSpreadsheetRow.SecondRecycleCartDeliveryDate);
                                    }
                                else
                                    {
                                    WRMLogger.LogBuilder.AppendLine("Second Recycling Cart Date recieved is invalid " + snSpreadsheetRow.SecondRecycleCartDeliveryDate);
                                    }
                                }

                            cart.CartSize = "65 GALLON";

                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.IsRecyclingCart = true;
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("Second Recycling Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
                            cart.CreateDate = DateTime.Now;
                            cart.CreateUser = "TrashRecyclePopulation";
                            cart.UpdateDate = DateTime.Now;
                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Second Recycling Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    }

                WRMLogger.Logger.logMessageAndDeltaTime("secondRecycleCart End Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                deleteAllCartsFromContext();
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw ex;
                }
            }

        public void addCurrentRecyclingCarts()
            {
            try
                {
                WRMLogger.Logger.logMessageAndDeltaTime("currentRecyclingCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                int cartCount = 0;
                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    if ((cartCount) > 0 && (cartCount % 1000 == 0))
                        {
                        WRMLogger.Logger.logMessageAndDeltaTime("currentRecyclingCart Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        ++cartCount;

                        }
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;
                    try
                        {
                        if (!String.IsNullOrEmpty(snSpreadsheetRow.CurrentRecycleCartSN))
                            {
                            Cart cart = new Cart();
                            cart.CartSerialNumber = snSpreadsheetRow.CurrentRecycleCartSN;
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.CurrentRecycleCartDeliveryDate))
                                {
                                if (validateDateString(snSpreadsheetRow.CurrentRecycleCartDeliveryDate))
                                    {
                                    cart.SerialNumberReceivedDate = DateTime.Parse(snSpreadsheetRow.CurrentRecycleCartDeliveryDate);
                                    }
                                else
                                    {
                                    WRMLogger.LogBuilder.AppendLine("Current Recycling Cart Date recieved is invalid " + snSpreadsheetRow.CurrentRecycleCartDeliveryDate);
                                    }
                                }

                            cart.CartSize = "65 GALLON";

                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.IsRecyclingCart = true;
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("Current Recycling Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
                            cart.CreateDate = DateTime.Now;
                            cart.CreateUser = "TrashRecyclePopulation";
                            cart.UpdateDate = DateTime.Now;
                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Current Recycling Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    }

                WRMLogger.Logger.logMessageAndDeltaTime("currentRecycleCart End Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw ex;
                }
            }

        public void addAddressesFromSNMasterList()
            {
            try
                {
                DateTime begin = DateTime.Now;
                DateTime beforeNow = DateTime.Now;
                DateTime justNow = DateTime.Now;
                TimeSpan timeDiff = justNow - beforeNow;
                double loopMillisecondsPast = 0;
                String logLine;
                int maxToProcess = 0;

                // populate all the addresses that are new

                //update any addresses with new Address types, commercial and specialty
                logLine = "Start Cart Address Population";
                WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);

                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    String dictionaryKey = snSpreadSheetRowPair.Key;
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;
                    try
                        {
                        if ((maxToProcess > 0) && (maxToProcess % 1000 == 0))
                            {
                            logLine = "Cart addAddressesFromSNMasterList " + maxToProcess;
                            WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                            WRMLogger.Logger.log();
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                            maxToProcess++;
                            }

                        Address address = snSpreadsheetRow.Address;
                        Address foundAddress = new Address();
                        if (AddressPopulation.AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                            {
                            // overwrite address type if populated in spreadsheet
                            if ((!String.IsNullOrEmpty(snSpreadsheetRow.CurrentTrashCartSN))
                                    && (!snSpreadsheetRow.CurrentTrashCartSN.ToUpper().Equals("NO TRASH")))
                                {
                                AddressPopulation.AddressDictionary[dictionaryKey].TrashPickup = true;
                                }
                            else
                                {
                                AddressPopulation.AddressDictionary[dictionaryKey].TrashPickup = false;
                                }

                            if (!String.IsNullOrEmpty(snSpreadsheetRow.CurrentRecycleCartSN))
                                {
                                AddressPopulation.AddressDictionary[dictionaryKey].RecyclingPickup = true;
                                }
                            else
                                {
                                AddressPopulation.AddressDictionary[dictionaryKey].RecyclingPickup = false;
                                }

                            if ((!String.IsNullOrEmpty(address.AddressType)) && !address.AddressType.Equals(foundAddress.AddressType))
                                {
                                AddressPopulation.AddressDictionary[dictionaryKey].AddressType = address.AddressType;
                                }
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[dictionaryKey]);
                            }
                        else
                            {
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(address);
                            AddressPopulation.AddressDictionary.Add(dictionaryKey, address);
                            }
                        ++maxToProcess;
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    catch (WRMNotSupportedException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }

                    }
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                foreach (Address address in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Address.ToList())
                    {
                    string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                    AddressPopulation.AddressIdentiferDictionary[dictionaryKey] = address.AddressID;
                    AddressPopulation.ReverseAddressIdentiferDictionary[address.AddressID] = dictionaryKey;
                    }
                justNow = DateTime.Now;
                timeDiff = justNow - beforeNow;
                WRMLogger.LogBuilder.AppendLine("End Address Cart population " + maxToProcess + " " + justNow.ToString("o", new CultureInfo("en-us")) + "Total MilliSeconds passed : " + timeDiff.TotalMilliseconds.ToString());
                //                WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref beforeNow, ref justNow, ref loopMillisecondsPast);
                WRMLogger.Logger.log();
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw ex;
                }
            }

        public bool rejectAddress(SNSpreadsheetRow snSpreadsheetRow)
            {
            bool rejectAddress = false;

            if (!String.IsNullOrEmpty(snSpreadsheetRow.AddressPreviouslyDemolished))
                {
                if (snSpreadsheetRow.AddressPreviouslyDemolished.Equals("1"))
                    {
                    rejectAddress = true;
                    }
                }
            if (!String.IsNullOrEmpty(snSpreadsheetRow.Disapproved))
                {
                if (snSpreadsheetRow.Disapproved.Equals("1"))
                    {
                    rejectAddress = true;
                    }
                }

            if (!String.IsNullOrEmpty(snSpreadsheetRow.NoCartHere))
                {
                if (snSpreadsheetRow.NoCartHere.Equals("1"))
                    {
                    rejectAddress = true;
                    }
                }

            return rejectAddress;
            }

        public Cart saveCart(Cart cart)
            {

            string serialNumber = cart.CartSerialNumber;
            if (String.IsNullOrWhiteSpace(serialNumber))
                {
                throw new WRMNullValueException("Cart Serial Number is Null");
                }
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);

            /*
            if (serialNumber.Contains(","))
                {
                serialNumberList = new List<String>(serialNumber.Split(","));
                }

            if (serialNumberList == null)
                {
                wrmTrashRecycleContext.Add(cart);
                // logBuilder.AppendLine("Add " + request.StreetNumber + "  " + request.StreetName);
                wrmTrashRecycleContext.SaveChanges();
                wrmTrashRecycleContext.ChangeTracker.DetectChanges();
                }
            else
                {
                foreach (string serialNumberNew in serialNumberList)
                    {
                    Cart additionalCart = new Cart();
                    additionalCart.AddressID = cart.AddressID;
                    additionalCart.CartSerialNumber = serialNumberNew;
                    additionalCart.CartStatus = cart.CartStatus;
                    additionalCart.CreateDate = cart.CreateDate;
                    additionalCart.CreateUser = cart.CreateUser;
                    additionalCart.IsRecyclingCart = cart.IsRecyclingCart;
                    additionalCart.Note = cart.Note;
                    additionalCart.CartSize = cart.CartSize;
                    additionalCart.SerialNumberReceivedDate = cart.SerialNumberReceivedDate;

                    wrmTrashRecycleContext.Add(additionalCart);
                    // logBuilder.AppendLine("Add " + request.StreetNumber + "  " + request.StreetName);
                    wrmTrashRecycleContext.SaveChanges();
                    wrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    }

                }
            */
            return cart;
            }
        private void deleteAllCartsFromContext()
            {
            int maxBeforeCommit = 0;
            WRMLogger.Logger.logMessageAndDeltaTime("Start deleteAllCartsFromContext ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
            foreach (Cart cart in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Cart.ToList())
                {
                if (maxBeforeCommit % 1000 == 0)
                    {
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    }
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Remove(cart);
                maxBeforeCommit++;
                }
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
            WRMLogger.Logger.logMessageAndDeltaTime("End deleteAllCartsFromContext ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Cart.ToList().Clear();
            WRMLogger.Logger.logMessageAndDeltaTime("Clear deleteAllCartsFromContext ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
            }

        private string getAddressTypeFromWorksheet(ExcelWorksheet worksheet, int row)
            {

            ExcelRange commercialAccount = worksheet.Cells[row, 21];
            ExcelRange specialtyAccount = worksheet.Cells[row, 23];

            if (commercialAccount != null && commercialAccount.Value != null && !string.IsNullOrEmpty(commercialAccount.Value.ToString()))
                {
                return "COMMERCIAL";
                }

            if (specialtyAccount != null && specialtyAccount.Value != null && !string.IsNullOrEmpty(specialtyAccount.Value.ToString()))
                {
                return "SPECIALTY";
                }

            return null;
            }

        private Address buildAddressFromWorksheet(ExcelWorksheet worksheet, int row)
            {


            // First determine if address should be added or ignored. if ignored, then return null


            // only extract ;information from cell 3
            Address address = new Address();
            ExcelRange streetNumberCell = worksheet.Cells[row, 1];
            ExcelRange streetNameCell = worksheet.Cells[row, 2];
            ExcelRange streetNameNumberCell = worksheet.Cells[row, 3];

            if (!(streetNumberCell == null || streetNumberCell.Value == null || streetNameCell == null || streetNameCell.Value == null ||
                String.IsNullOrEmpty(streetNameCell.Value.ToString()) || String.IsNullOrEmpty(streetNumberCell.Value.ToString())))
                {
                string streetName = streetNameCell.Value.ToString();
                address.StreetName = IdentifierProvider.normalizeStreetName(streetName);

                string streetNumber = streetNumberCell.Value.ToString();
                int streetNumberInt32 = IdentifierProvider.normalizeStreetNumber(streetNumber);

                address.StreetNumber = streetNumberInt32;
                }
            else if (!(streetNameNumberCell == null && streetNameNumberCell.Value == null || String.IsNullOrEmpty(streetNameNumberCell.Value.ToString())))
                {

                if (streetNameNumberCell.Value != null)
                    {
                    string streetNameAndNumber = streetNameNumberCell.Value.ToString();
                    string[] streetNameAndNumberArray = streetNameAndNumber.Split(' ', 2);
                    string streetNumber = streetNameAndNumberArray[0];
                    int streetNumberInt32 = IdentifierProvider.normalizeStreetNumber(streetNumber);
                    address.StreetNumber = streetNumberInt32;

                    string streetName = streetNameAndNumberArray[1];
                    address.StreetName = IdentifierProvider.normalizeStreetName(streetName);
                    }
                }
            else
                {
                throw new WRMNullValueException("Street and Number Address values for cart at row" + row + " empty");
                }
            ExcelRange unitCell = worksheet.Cells[row, 4];
            if (unitCell != null && unitCell.Value != null && !string.IsNullOrEmpty(unitCell.Value.ToString()))
                {
                string unitNumber = unitCell.Value.ToString();
                address.UnitNumber = IdentifierProvider.normalizeUnitNumber(unitNumber);
                }
            ExcelRange zipCell = worksheet.Cells[row, 5];
            if (zipCell != null && zipCell.Value != null && !string.IsNullOrEmpty(zipCell.Value.ToString()))
                {

                String zipString = zipCell.Value.ToString();
                address.ZipCode = IdentifierProvider.normalizeZipCode(zipString);

                }
            else
                {
                throw new WRMNullValueException("Zip Code value for cart at row" + row + " empty");
                }
            AddressPopulation.populateAddressFromKGIS(ref address);


            string addressType = getAddressTypeFromWorksheet(worksheet, row);


            // overwrite address type if populated in spreadsheet
            if (!(String.IsNullOrEmpty(addressType)) && !addressType.Equals(address.AddressType))
                {
                address.AddressType = addressType;
                }

            else if (String.IsNullOrEmpty(address.AddressType) && String.IsNullOrEmpty(addressType))
                {
                throw new WRMNullValueException("Address Type is null");
                }

            return address;
            }
        static bool validateDateString(string dateValue)
            {

            string[] formats = { "MM/dd/yyyy", "M/d/yyyy", "M/d/yyyy h:m:s tt", "MM/dd/yyyy hh:mm:ss tt", "M/d/yyyy hh:mm:ss tt" };
            DateTime dt = new DateTime();
            if (DateTime.TryParseExact(dateValue, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out dt))
                {
                DateTime earliestDate = DateTime.Parse("01/01/2010");
                DateTime latestDate = DateTime.Parse("01/01/9999");
                if ((dt > earliestDate) && (dt < latestDate))
                    {
                    return true;
                    }
                }

            return false;
            }

        public void populateSNMasterDictionary()
            {
            try
                {

                for (int row = 2; row <= totalRowsWorksheet; row++)
                    {
                    try
                        {

                        SNSpreadsheetRow snSpreadsheetRow = new SNSpreadsheetRow();
                        snSpreadsheetRow.RowNumber = row;
                        // has the default manner to 
                        Address address = buildAddressFromWorksheet(worksheet, row);
                        if (address == null)
                            {
                            continue;
                            }
                        string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                        SNSpreadsheetRow foundSNSpreadSheetRow = new SNSpreadsheetRow();
                        if (snMasterDictionary.TryGetValue(dictionaryKey, out foundSNSpreadSheetRow))
                            {
                            snMasterDictionary[dictionaryKey].CurrentTrashCartSN = "UNKNOWN";
                            if (!String.IsNullOrEmpty(foundSNSpreadSheetRow.CurrentRecycleCartSN))
                                {
                                snMasterDictionary[dictionaryKey].CurrentRecycleCartSN = "UNKNONW";
                                }
                            WRMLogger.LogBuilder.AppendLine("Address already added " + dictionaryKey);
                            continue;
                            }

                        snSpreadsheetRow.Address = address;

                        // ADd Current Trash Serial Number

                        ExcelRange currentTrashCartSNDeliveryDateRange = worksheet.Cells[row, 6];
                        snSpreadsheetRow.CurrentTrashCartDeliveryDate = convertExcelRangeToString(currentTrashCartSNDeliveryDateRange);

                        ExcelRange currentTrashCartSNRange = worksheet.Cells[row, 7];
                        snSpreadsheetRow.CurrentTrashCartSN = convertExcelRangeToString(currentTrashCartSNRange);

                        ExcelRange firstTrashCartSNDeliveryDateRange = worksheet.Cells[row, 8];
                        snSpreadsheetRow.FirstTrashCartDeliveryDate = convertExcelRangeToString(firstTrashCartSNDeliveryDateRange);

                        ExcelRange firstTrashCartSNRange = worksheet.Cells[row, 9];
                        snSpreadsheetRow.FirstTrashCartSN = convertExcelRangeToString(firstTrashCartSNRange);

                        ExcelRange secondTrashCartSNDeliveryDateRange = worksheet.Cells[row, 10];
                        snSpreadsheetRow.SecondTrashCartDeliveryDate = convertExcelRangeToString(secondTrashCartSNDeliveryDateRange);

                        ExcelRange secondTrashCartSNRange = worksheet.Cells[row, 11];
                        snSpreadsheetRow.SecondTrashCartSN = convertExcelRangeToString(secondTrashCartSNRange);

                        ExcelRange thirdTrashCartSNDeliveryDateRange = worksheet.Cells[row, 12];
                        snSpreadsheetRow.ThirdTrashCartDeliveryDate = convertExcelRangeToString(thirdTrashCartSNDeliveryDateRange);

                        ExcelRange thirdTrashCartSNRange = worksheet.Cells[row, 13];
                        snSpreadsheetRow.ThirdTrashCartSN = convertExcelRangeToString(thirdTrashCartSNRange);

                        ExcelRange currentRecyclingCartSNDeliveryDateRange = worksheet.Cells[row, 14];
                        snSpreadsheetRow.CurrentRecycleCartDeliveryDate = convertExcelRangeToString(currentRecyclingCartSNDeliveryDateRange);

                        ExcelRange currentRecyclingCartSNRange = worksheet.Cells[row, 15];
                        snSpreadsheetRow.CurrentRecycleCartSN = convertExcelRangeToString(currentRecyclingCartSNRange);

                        ExcelRange firstRecyclingCartSNDeliveryDateRange = worksheet.Cells[row, 16];
                        snSpreadsheetRow.FirstRecycleCartDeliveryDate = convertExcelRangeToString(firstRecyclingCartSNDeliveryDateRange);

                        ExcelRange firstRecyclingCartSNRange = worksheet.Cells[row, 17];
                        snSpreadsheetRow.FirstRecycleCartSN = convertExcelRangeToString(firstRecyclingCartSNRange);

                        ExcelRange secondRecyclingCartSNDeliveryDateRange = worksheet.Cells[row, 18];
                        snSpreadsheetRow.SecondTrashCartDeliveryDate = convertExcelRangeToString(secondRecyclingCartSNDeliveryDateRange);

                        ExcelRange secondRecyclingCartSNRange = worksheet.Cells[row, 19];
                        snSpreadsheetRow.SecondRecycleCartSN = convertExcelRangeToString(secondRecyclingCartSNRange);

                        ExcelRange multifamilyUnitSNRange = worksheet.Cells[row, 20];
                        snSpreadsheetRow.MultiFamilyUnit = convertExcelRangeToString(multifamilyUnitSNRange);

                        ExcelRange commercialSNRange = worksheet.Cells[row, 21];
                        snSpreadsheetRow.CommercialAccount = convertExcelRangeToString(commercialSNRange);

                        ExcelRange smallCartSNRange = worksheet.Cells[row, 22];
                        snSpreadsheetRow.SmallTrashCart = convertExcelRangeToString(smallCartSNRange);

                        ExcelRange specialtySNRange = worksheet.Cells[row, 23];
                        snSpreadsheetRow.Specialty = convertExcelRangeToString(specialtySNRange);

                        ExcelRange demolishedSNRange = worksheet.Cells[row, 24];
                        snSpreadsheetRow.AddressPreviouslyDemolished = convertExcelRangeToString(demolishedSNRange);

                        ExcelRange disapprovedSNRange = worksheet.Cells[row, 25];
                        snSpreadsheetRow.Disapproved = convertExcelRangeToString(disapprovedSNRange);

                        ExcelRange noCartSNRange = worksheet.Cells[row, 28];
                        snSpreadsheetRow.NoCartHere = convertExcelRangeToString(noCartSNRange);

                        ExcelRange invalidKGISSNRange = worksheet.Cells[row, 27];
                        snSpreadsheetRow.InvalidKGIS = convertExcelRangeToString(invalidKGISSNRange);

                        ExcelRange notesSNRange = worksheet.Cells[row, 29];
                        snSpreadsheetRow.Notes = convertExcelRangeToString(notesSNRange);

                        if (!rejectAddress(snSpreadsheetRow))
                            {

                            snMasterDictionary.Add(dictionaryKey, snSpreadsheetRow);
                            }

                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Cart Population At row " + row + " " + ex.Message);

                        }
                    catch (WRMNotSupportedException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Cart Population At row " + row + " " + ex.Message);

                        }

                    }

                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw ex;
                }
            }
        private string convertExcelRangeToString(ExcelRange snSpreadSheetExcelRange)
            {
            string returnString = "";
            if (snSpreadSheetExcelRange != null && snSpreadSheetExcelRange.Value != null)
                {
                string cellValue = snSpreadSheetExcelRange.Value.ToString();
                if (!String.IsNullOrWhiteSpace(cellValue))
                    {
                    if (cellValue.Equals("NO TRASH"))
                        {
                        throw new WRMNullValueException("Serial Number is NO TRASH");
                        }
                    else
                        {
                        returnString = cellValue;
                        }
                    }
                }
            return returnString;
            }

        }

    }

