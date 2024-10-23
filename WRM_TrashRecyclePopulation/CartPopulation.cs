using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using OfficeOpenXml;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;
using System.Globalization;
using System.ComponentModel;
using System.Configuration;

namespace WRM_TrashRecyclePopulation
    {

    class CartPopulation
        {
        private FileInfo xlsxSNMasterlistFileInfo;

        public Dictionary<string, SNSpreadsheetRow> snMasterDictionary = new Dictionary<string, SNSpreadsheetRow>();

        public List<string> ignoreCartDuringPopulation = new List<string>();

        public static string xlsxSNMasterlistPath = ConfigurationManager.AppSettings["SN_Masterlist_Spreadsheet"].ToString();

        public static string xlsxSNMasterlistPathWithErrors = ConfigurationManager.AppSettings["SN_Masterlist_Spreadsheet_WithErrors"].ToString();
        private int totalRowsWorksheet;

        ExcelPackage package;

        public List<string> addedAddressesDuringPopulation = new List<string>();

        /* 
         * Pull all the records from the Serial Number Master Spreadsheet
         * and populate the cart & address tables in the database
         * 
         */
        public CartPopulation()
            {
            // open the spreadsheet
            xlsxSNMasterlistFileInfo = new FileInfo(xlsxSNMasterlistPath);
            if (!xlsxSNMasterlistFileInfo.Exists)
                {
                throw new Exception(xlsxSNMasterlistPath + "does not exist");
                }
            // read in the spreadsheet creating a few additional columns for error reporting
            package = new ExcelPackage(xlsxSNMasterlistFileInfo);
            package.Workbook.Worksheets[0].InsertColumn(35, 1);
            package.Workbook.Worksheets[0].Cells[1, 35].Value = "Found in KGIS";
            package.Workbook.Worksheets[0].InsertColumn(36, 1);
            package.Workbook.Worksheets[0].Cells[1, 36].Value = "Current Trash Cart Errors";
            package.Workbook.Worksheets[0].InsertColumn(37, 1);
            package.Workbook.Worksheets[0].Cells[1, 37].Value = "Current Recycling Cart Errors";
            package.Workbook.Worksheets[0].InsertColumn(38, 1);
            package.Workbook.Worksheets[0].Cells[1, 38].Value = "Address Population Cart Errors";
            totalRowsWorksheet = package.Workbook.Worksheets[0].Dimension.End.Row;
            }

        public void populateCarts()
            {
            try
                {
                DateTime begin = DateTime.Now;
                DateTime beforeNow = DateTime.Now;
                DateTime justNow = DateTime.Now;
                TimeSpan timeDiff = justNow - beforeNow;

                // populate all the addresses that are new
                WRMLogger.Logger.logMessageAndDeltaTime("populateSNMasterDictionary ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                populateSNMasterDictionary();
                WRMLogger.Logger.logMessageAndDeltaTime("addOrUpdateAddressesFromSNMasterList ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                addOrUpdateAddressesFromSNMasterList();

                // add the carts to the database in chronological order
                WRMLogger.Logger.logMessageAndDeltaTime("addFirstTrashCarts ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                addFirstTrashCarts();
                // carts that replace old carts are deleted and placed in the cart history table
                WRMLogger.LogBuilder.AppendLine("addSecondTrashCarts");
                addSecondTrashCarts();

                WRMLogger.LogBuilder.AppendLine("addThirdTrashCarts");
                addThirdTrashCarts();
                WRMLogger.LogBuilder.AppendLine("addFourthTrashCarts");
                addFourthTrashCarts();

                WRMLogger.LogBuilder.AppendLine("addFirstRecyclingCarts");
                addFirstRecyclingCarts();
                WRMLogger.LogBuilder.AppendLine("addSecondRecyclingCarts");
                addSecondRecyclingCarts();
                WRMLogger.LogBuilder.AppendLine("addThirdRecyclingCarts");
                addThirdRecyclingCarts();

                
                WRMLogger.LogBuilder.AppendLine("addCurrentTrashCarts");
                addCurrentTrashCarts();
                WRMLogger.LogBuilder.AppendLine("addFirstRecyclingCarts");
                addCurrentRecyclingCarts();
                // the entire spreadsheet has been parsed
                // print off the spreadsheet with the errors included for debugging
                package.SaveAs(xlsxSNMasterlistPathWithErrors);

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
                List<String> updateDatabaseAddressList = new List<String>();
                //WRMLogger.Logger.logMessageAndDeltaTime("addCurrentTrashCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
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
                           && (snSpreadsheetRow.CurrentTrashCartSN.ToUpper().Equals("INELIGIBLE")))
                            {
                            AddressPopulation.AddressDictionary[snSpreadSheetRowPair.Key].TrashStatus = "INELIGIBLE";
                            updateDatabaseAddressList.Add(snSpreadSheetRowPair.Key);
                            //package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 36].Value = "Trash Cart INELIGIBLE";
                            continue;
                            }
                        if ((!String.IsNullOrEmpty(snSpreadsheetRow.CurrentTrashCartSN))
                        && ((!snSpreadsheetRow.CurrentTrashCartSN.ToUpper().Equals("NO TRASH")) ||
                            (snSpreadsheetRow.CurrentRecycleCartSN.ToUpper().Equals("WITHDRAWN"))))
                        
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

                                    package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 36].Value = "Current Cart Date received is invalid " + snSpreadsheetRow.CurrentTrashCartDeliveryDate;
                                    WRMLogger.LogBuilder.AppendLine("Current Cart Date received is invalid " + snSpreadsheetRow.CurrentTrashCartDeliveryDate);
                                    }
                                }
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SmallTrashCart) && snSpreadsheetRow.SmallTrashCart.Equals("1"))
                                {
                                cart.CartSize = "64 GALLON";
                                }
                            else
                                {
                                cart.CartSize = "96 GALLON";
                                }
                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.CartStatus = "TRASH";
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;
                            if (!addressKey.StartsWith("NOADDRESS"))
                                {
                                if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                    {
                                    throw new WRMNullValueException("Current Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                    }
                                AddressPopulation.AddressDictionary[addressKey].TrashStatus = "ELIGIBLE";
                                updateDatabaseAddressList.Add(addressKey);
                                cart.AddressID = addressId;
                                }
                            else
                                {
                                WRMLogger.LogBuilder.AppendLine("Ignoring ? in line: " + snSpreadsheetRow.RowNumber);
                                }
                            cart.Note = snSpreadsheetRow.Notes;
 //                           cart.CreateDate = DateTime.Now;
 //                           cart.CreateUser = "TrashRecyclePopulation";
 //                           cart.UpdateDate = DateTime.Now;
 //                           cart.UpdateUser = "TrashRecyclePopulation";



                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (Exception ex) when (ex is WRMWithdrawnStatusException || ex is WRMNotSupportedException || ex is WRMNullValueException)
                        {
                        package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 36].Value = ex.Message;
                        WRMLogger.LogBuilder.AppendLine("ERROR C15: Current Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    }

               
                int maxBeforeCommit = 0;
                foreach (String addressDictionaryKey in updateDatabaseAddressList)
                    {
                    if (maxBeforeCommit % 2000 == 0)
                        {
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        }
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[addressDictionaryKey]);
                    maxBeforeCommit++;
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
                                    package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 36].Value = "Second Trash Cart Date received is invalid " + snSpreadsheetRow.FirstTrashCartDeliveryDate;
                                    WRMLogger.LogBuilder.AppendLine("First Trash Cart Date received is invalid " + snSpreadsheetRow.FirstTrashCartDeliveryDate);
                                    }
                                }
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SmallTrashCart) && snSpreadsheetRow.SmallTrashCart.Equals("1"))
                                {
                                cart.CartSize = "64 GALLON";
                                }
                            else
                                {
                                cart.CartSize = "96 GALLON";
                                }
                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record

                            cart.CartType = "TRASH";
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("First Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
                            //                            cart.CreateDate = DateTime.Now;
                            //                            cart.CreateUser = "TrashRecyclePopulation";
                            //                            cart.UpdateDate = DateTime.Now;
                            //                            cart.UpdateUser = "TrashRecyclePopulation";
                            //  foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(cart))
                            //      {
                            //      string name = descriptor.Name;
                            //      object value = descriptor.GetValue(cart);
                            //      WRMLogger.LogBuilder.AppendLine("{"+ name + "}={"+ value +"}");
                            //      }
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C1: First Trash Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

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
                // WRMLogger.Logger.logMessageAndDeltaTime("secondTrashCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
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
                                    WRMLogger.LogBuilder.AppendLine("Second Trash Cart Date received is invalid " + snSpreadsheetRow.SecondTrashCartDeliveryDate);
                                    package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 36].Value = "Second Trash Cart Date received is invalid " + snSpreadsheetRow.SecondTrashCartDeliveryDate;
                                    }
                                }
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SmallTrashCart) && snSpreadsheetRow.SmallTrashCart.Equals("1"))
                                {
                                cart.CartSize = "64 GALLON";
                                }
                            else
                                {
                                cart.CartSize = "96 GALLON";
                                }
                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.CartType = "TRASH";
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("Second Trash Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
 //                           cart.CreateDate = DateTime.Now;
 //                           cart.CreateUser = "TrashRecyclePopulation";
 //                           cart.UpdateDate = DateTime.Now;
 //                           cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C12: Second Trash Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

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
                // WRMLogger.Logger.logMessageAndDeltaTime("thirdTrashCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
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
                                    WRMLogger.LogBuilder.AppendLine("Third Trash Cart Date received is invalid " + snSpreadsheetRow.ThirdTrashCartDeliveryDate);
                                    package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 36].Value = "Third Trash Cart Date received is invalid " + snSpreadsheetRow.ThirdTrashCartDeliveryDate;
                                    }
                                }
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SmallTrashCart) && snSpreadsheetRow.SmallTrashCart.Equals("1"))
                                {
                                cart.CartSize = "64 GALLON";
                                }
                            else
                                {
                                cart.CartSize = "96 GALLON";
                                }
                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.CartType = "TRASH";
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("Third Trash Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
//                            cart.CreateDate = DateTime.Now;
//                            cart.CreateUser = "TrashRecyclePopulation";
//                            cart.UpdateDate = DateTime.Now;
//                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C3: Third Trash Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

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

        public void addFourthTrashCarts()
            {
            try
                {
                // WRMLogger.Logger.logMessageAndDeltaTime("fourthTrashCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                int cartCount = 0;
                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    if ((cartCount) > 0 && (cartCount % 1000 == 0))
                        {
                        WRMLogger.Logger.logMessageAndDeltaTime("fourthTrashCart Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        ++cartCount;

                        }
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;
                    try
                        {
                        if (!String.IsNullOrEmpty(snSpreadsheetRow.FourthTrashCartSN))
                            {
                            Cart cart = new Cart();
                            cart.CartSerialNumber = snSpreadsheetRow.FourthTrashCartSN;
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.FourthTrashCartDeliveryDate))
                                {
                                if (validateDateString(snSpreadsheetRow.FourthTrashCartDeliveryDate))
                                    {
                                    cart.SerialNumberReceivedDate = DateTime.Parse(snSpreadsheetRow.FourthTrashCartDeliveryDate);
                                    }
                                else
                                    {
                                    WRMLogger.LogBuilder.AppendLine("Fourth Trash Cart Date received is invalid " + snSpreadsheetRow.FourthTrashCartDeliveryDate);
                                    package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 36].Value = "Fourth Trash Cart Date received is invalid " + snSpreadsheetRow.FourthTrashCartDeliveryDate;
                                    }
                                }
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.SmallTrashCart) && snSpreadsheetRow.SmallTrashCart.Equals("1"))
                                {
                                cart.CartSize = "64 GALLON";
                                }
                            else
                                {
                                cart.CartSize = "96 GALLON";
                                }
                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.CartType = "TRASH";
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("Fourth Trash Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
//                            cart.CreateDate = DateTime.Now;
//                            cart.CreateUser = "TrashRecyclePopulation";
//                            cart.UpdateDate = DateTime.Now;
//                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C4: Third Trash Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

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
                // WRMLogger.Logger.logMessageAndDeltaTime("firstRecyclingCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
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
                                    WRMLogger.LogBuilder.AppendLine("First Recycling Cart Date received is invalid " + snSpreadsheetRow.FirstRecycleCartDeliveryDate);
                                    package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 37].Value = "First Recycling Cart Date received is invalid " + snSpreadsheetRow.FirstRecycleCartDeliveryDate;
                                    }
                                }

                            cart.CartSize = "64 GALLON";

                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.CartType = "RECYCLING";
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("First Recycling Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
//                            cart.CreateDate = DateTime.Now;
//                            cart.CreateUser = "TrashRecyclePopulation";
//                            cart.UpdateDate = DateTime.Now;
//                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C5: First Recycling Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

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
                // WRMLogger.Logger.logMessageAndDeltaTime("secondRecyclingCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
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
                                    WRMLogger.LogBuilder.AppendLine("Second Recycling Cart Date received is invalid " + snSpreadsheetRow.SecondRecycleCartDeliveryDate);
                                    package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 37].Value = "Second Recycling Cart Date received is invalid " + snSpreadsheetRow.SecondRecycleCartDeliveryDate;
                                    }
                                }

                            cart.CartSize = "64 GALLON";

                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.CartType = "RECYCLING";
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("Second Recycling Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
//                            cart.CreateDate = DateTime.Now;
//                            cart.CreateUser = "TrashRecyclePopulation";
//                            cart.UpdateDate = DateTime.Now;
//                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C6: Second Recycling Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

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

        public void addThirdRecyclingCarts()
            {
            try
                {
                // WRMLogger.Logger.logMessageAndDeltaTime("thirdRecyclingCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                int cartCount = 0;
                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    if ((cartCount) > 0 && (cartCount % 1000 == 0))
                        {
                        WRMLogger.Logger.logMessageAndDeltaTime("thirdRecyclingCart Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        ++cartCount;

                        }
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;
                    try
                        {
                        if (!String.IsNullOrEmpty(snSpreadsheetRow.ThirdRecycleCartSN))
                            {
                            Cart cart = new Cart();
                            cart.CartSerialNumber = snSpreadsheetRow.ThirdRecycleCartSN;
                            if (!String.IsNullOrEmpty(snSpreadsheetRow.ThirdRecycleCartDeliveryDate))
                                {
                                if (validateDateString(snSpreadsheetRow.ThirdRecycleCartDeliveryDate))
                                    {
                                    cart.SerialNumberReceivedDate = DateTime.Parse(snSpreadsheetRow.ThirdRecycleCartDeliveryDate);
                                    }
                                else
                                    {
                                    WRMLogger.LogBuilder.AppendLine("Third Recycling Cart Date received is invalid " + snSpreadsheetRow.ThirdRecycleCartDeliveryDate);
                                    package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 37].Value = "Third Recycling Cart Date received is invalid " + snSpreadsheetRow.ThirdRecycleCartDeliveryDate;
                                    }
                                }

                            cart.CartSize = "64 GALLON";

                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.CartType = "RECYCLING";
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;

                            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                {
                                throw new WRMNullValueException("Third Recycling Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                }
                            cart.AddressID = addressId;
                            cart.Note = snSpreadsheetRow.Notes;
//                            cart.CreateDate = DateTime.Now;
//                            cart.CreateUser = "TrashRecyclePopulation";
//                            cart.UpdateDate = DateTime.Now;
//                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C7: Third Recycling Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    }

                WRMLogger.Logger.logMessageAndDeltaTime("thirdRecycleCart End Population " + cartCount, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
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
                List<String> updateDatabaseAddressList = new List<String>();
                // WRMLogger.Logger.logMessageAndDeltaTime("currentRecyclingCart Start ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                int cartCount = 0;
                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    if ((cartCount) > 0 && (cartCount % 2000 == 0))
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

                        if ((!String.IsNullOrEmpty(snSpreadsheetRow.CurrentRecycleCartSN))
                            && ((snSpreadsheetRow.CurrentRecycleCartSN.ToUpper().Equals("INELIGIBLE")) || (snSpreadsheetRow.CurrentRecycleCartSN.ToUpper().Equals("WITHDRAWN"))))
                            {
                            AddressPopulation.AddressDictionary[snSpreadSheetRowPair.Key].RecyclingStatus = "NOT RECYCLING";
                            updateDatabaseAddressList.Add(snSpreadSheetRowPair.Key);
                            // package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 37].Value = "Recycling Cart INELIGIBLE";
                            continue;
                            }
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
                                    package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 37].Value = "Current Recycling Cart Date received is invalid " + snSpreadsheetRow.CurrentRecycleCartDeliveryDate;
                                    WRMLogger.LogBuilder.AppendLine("Current Recycling Cart Date received is invalid " + snSpreadsheetRow.CurrentRecycleCartDeliveryDate);
                                    }
                                }

                            cart.CartSize = "64 GALLON";

                            //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                            cart.CartType = "RECYCLING";
                            cart.CartStatus = "ACTIVE";
                            int addressId = 0;
                            string addressKey = snSpreadSheetRowPair.Key;
                            if (!addressKey.StartsWith("NOADDRESS"))
                                {

                                if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                                    {
                                    throw new WRMNullValueException("Current Recycling Cart Address cannot be found in AddressIdentiferDictionary " + addressKey);
                                    }
                                AddressPopulation.AddressDictionary[addressKey].RecyclingStatus = "RECYCLING";
                                updateDatabaseAddressList.Add(addressKey);

                                cart.AddressID = addressId;
                                }
                            else
                                {
                                WRMLogger.LogBuilder.AppendLine("Ignoring ? in line: " + snSpreadsheetRow.RowNumber);
                                }
                              cart.Note = snSpreadsheetRow.Notes;
//                            cart.CreateDate = DateTime.Now;
//                            cart.CreateUser = "TrashRecyclePopulation";
//                            cart.UpdateDate = DateTime.Now;
//                            cart.UpdateUser = "TrashRecyclePopulation";
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(cart);
                            ++cartCount;
                            }
                        }
                    catch (Exception ex) when (ex is WRMWithdrawnStatusException || ex is WRMNotSupportedException || ex is WRMNullValueException)
                        {
                        package.Workbook.Worksheets[0].Cells[snSpreadsheetRow.RowNumber, 37].Value = ex.Message;
                        WRMLogger.LogBuilder.AppendLine("ERROR C8: Current Recycling Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    }
                int maxBeforeCommit = 0;
                foreach (String addressDictionaryKey in updateDatabaseAddressList)
                    {
                    if (maxBeforeCommit % 2000 == 0)
                        {
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        }
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[addressDictionaryKey]);
                    maxBeforeCommit++;
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

        public void addOrUpdateAddressesFromSNMasterList()
            {
            try
                {
                DateTime begin = DateTime.Now;
                DateTime beforeNow = DateTime.Now;
                DateTime justNow = DateTime.Now;
                TimeSpan timeDiff = justNow - beforeNow;

                int maxToProcess = 0;

                // populate all the addresses that are new

                //update any addresses with new Address types, commercial and specialty
                // logLine = "Start Cart Address Population";
                // WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);

                foreach (KeyValuePair<String, SNSpreadsheetRow> snSpreadSheetRowPair in snMasterDictionary.AsEnumerable<KeyValuePair<String, SNSpreadsheetRow>>())
                    {
                    String dictionaryKey = snSpreadSheetRowPair.Key;
                    SNSpreadsheetRow snSpreadsheetRow = snSpreadSheetRowPair.Value;

                    try
                        {
                        if ((maxToProcess > 0) && (maxToProcess % 1000 == 0))
                            {
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                            maxToProcess++;
                            }

                        Address address = snSpreadsheetRow.Address;
                        if (address != null && address.StreetName.Contains("?"))
                            {
                            WRMLogger.LogBuilder.AppendLine("Ignoring ? in line: " + snSpreadsheetRow.RowNumber);
                            continue;
                            }
                        Address foundAddress = new Address();
                        if (AddressPopulation.AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                            {
                            // overwrite address type if populated in spreadsheet
                            AddressPopulation.AddressDictionary[dictionaryKey].TrashStatus = address.TrashStatus;
                            AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatus = address.RecyclingStatus;
                            //                            AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatusDate = DateTime.Now;
                            //                            AddressPopulation.AddressDictionary[dictionaryKey].UpdateDate = DateTime.Now;
                            //                            AddressPopulation.AddressDictionary[dictionaryKey].UpdateUser = "TrashRecyclePopulation";
                            AddressPopulation.AddressDictionary[dictionaryKey].NumberUnits = snSpreadsheetRow.MultiFamilyUnit;

                            if ((!String.IsNullOrEmpty(address.AddressType)) && !address.AddressType.Equals(foundAddress.AddressType))
                                {
                                AddressPopulation.AddressDictionary[dictionaryKey].AddressType = address.AddressType;
                                }
                            if (!addedAddressesDuringPopulation.Contains(dictionaryKey))
                                {
                                if (String.IsNullOrEmpty(snSpreadsheetRow.MultiFamilyUnit))
                                    {
                                    address.NumberUnits = "1";
                                    }
                                else
                                    {
                                    address.NumberUnits = snSpreadsheetRow.MultiFamilyUnit;
                                    }
                                // WRMLogger.LogBuilder.AppendLine("Address Cart population found Address Key: Total MilliSeconds passed : " + dictionaryKey);
                                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(address);
                                ++maxToProcess;
                                addedAddressesDuringPopulation.Add(dictionaryKey);
                                }
                            }
                        else
                            {
                            if (String.IsNullOrEmpty(snSpreadsheetRow.MultiFamilyUnit))
                                {
                                address.NumberUnits = "1";
                                }
                            else
                                {
                                address.NumberUnits = snSpreadsheetRow.MultiFamilyUnit;
                                }
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(address);
                            AddressPopulation.AddressDictionary.Add(dictionaryKey, address);
                            addedAddressesDuringPopulation.Add(dictionaryKey);
                            }

                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C9: Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    catch (WRMNotSupportedException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C10: Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);

                        }
                    catch (Exception ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C11: Cart Population At row " + snSpreadsheetRow.RowNumber + " " + ex.Message);
                        throw ex;
                        }


                        }
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                AddressPopulation.AddressDictionary.Clear();
                foreach (Address address in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Address.ToList())
                    {
                    string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                    AddressPopulation.AddressIdentiferDictionary[dictionaryKey] = address.AddressID;
                    AddressPopulation.ReverseAddressIdentiferDictionary[address.AddressID] = dictionaryKey;
                    AddressPopulation.AddressDictionary[dictionaryKey] = address;
                    if (address.AddressType.Equals("COMMERCIAL"))
                        {
                        CommercialAccount commercialAccout = CommercialAccountPopulation.buildCommercialAccountFromAddress(address);
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(commercialAccout);
                        }
                    }
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();

                justNow = DateTime.Now;
                timeDiff = justNow - beforeNow;
                WRMLogger.LogBuilder.AppendLine("End Address Cart population " + maxToProcess + " " + justNow.ToString("o", new CultureInfo("en-us")) + "Total MilliSeconds passed : " + timeDiff.TotalMilliseconds.ToString());

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

            return cart;
            }
        private void deleteAllCartsFromContext()
            {
            List<String> updateDatabaseAddressList = new List<String>();
            int maxBeforeCommit = 0;
            // WRMLogger.Logger.logMessageAndDeltaTime("Start deleteAllCartsFromContext ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
            foreach (Cart cart in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Cart.ToList())
                {
                if (maxBeforeCommit % 2000 == 0)
                    {
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    }
                String addressDictionaryKey = AddressPopulation.ReverseAddressIdentiferDictionary[cart.AddressID ?? 0];
                updateDatabaseAddressList.Add(addressDictionaryKey);

                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Remove(cart);
                maxBeforeCommit++;
                }
            maxBeforeCommit = 0;

            foreach (String addressDictionaryKey in updateDatabaseAddressList)
                {
                if (maxBeforeCommit % 2000 == 0)
                    {
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    }
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[addressDictionaryKey]);
                maxBeforeCommit++;
                }

            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);

            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Cart.ToList().Clear();

            }

        private string getAddressTypeFromWorksheet(int row)
            {

            ExcelRange commercialAccount = package.Workbook.Worksheets[0].Cells[row, 21];
            ExcelRange specialtyAccount = package.Workbook.Worksheets[0].Cells[row, 23];

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

        private Address buildAddressFromWorksheet(int row)
            {


            // First determine if address should be added or ignored. if ignored, then return null


            // only extract ;information from cell 3
            Address address = new Address();
            ExcelRange streetNumberCell = package.Workbook.Worksheets[0].Cells[row, 1];
            ExcelRange streetNameCell = package.Workbook.Worksheets[0].Cells[row, 2];
            ExcelRange streetNameNumberCell = package.Workbook.Worksheets[0].Cells[row, 3];

            if ((streetNumberCell != null && streetNameCell != null)
                && ((!(streetNumberCell.Value == null || streetNameCell.Value == null ||
                    String.IsNullOrEmpty(streetNameCell.Value.ToString()) || String.IsNullOrEmpty(streetNumberCell.Value.ToString())))))
                {
                string streetName = streetNameCell.Value.ToString();
                address.StreetName = IdentifierProvider.normalizeStreetName(streetName);

                string streetNumber = streetNumberCell.Value.ToString();
                int streetNumberInt32 = IdentifierProvider.normalizeStreetNumber(streetNumber);

                address.StreetNumber = streetNumberInt32;
                }
            else if ((streetNameNumberCell != null) && ((streetNameNumberCell.Value != null && !String.IsNullOrEmpty(streetNameNumberCell.Value.ToString()))))
                {
                string streetNameAndNumber = streetNameNumberCell.Value.ToString();
                string[] streetNameAndNumberArray = streetNameAndNumber.Split(' ', 2);
                string streetNumber = streetNameAndNumberArray[0];
                int streetNumberInt32 = IdentifierProvider.normalizeStreetNumber(streetNumber);
                address.StreetNumber = streetNumberInt32;

                string streetName = streetNameAndNumberArray[1];
                address.StreetName = IdentifierProvider.normalizeStreetName(streetName);
                }
            else
                {
                throw new WRMNullValueException("Address values for cart at row " + row + " are null or empty");
                }
            ExcelRange unitCell = package.Workbook.Worksheets[0].Cells[row, 4];
            if (unitCell != null && unitCell.Value != null && !string.IsNullOrEmpty(unitCell.Value.ToString()))
                {
                string unitNumber = unitCell.Value.ToString();
                address.UnitNumber = IdentifierProvider.normalizeUnitNumber(unitNumber);
                }
            ExcelRange zipCell = package.Workbook.Worksheets[0].Cells[row, 5];
            if (zipCell != null && zipCell.Value != null && !string.IsNullOrEmpty(zipCell.Value.ToString()))
                {

                String zipString = zipCell.Value.ToString();
                address.ZipCode = IdentifierProvider.normalizeZipCode(zipString);

                }
            else
                {
                throw new WRMNullValueException("Zip Code value for cart at row " + row + " empty");
                }

            if (AddressPopulation.populateAddressFromKGIS(ref address))
                {
                package.Workbook.Worksheets[0].Cells[row, 35].Value = "True";

                try
                    {
                    AddressPopulation.translateAddressTypeFromKGISAddressUse(address.GISAddressUseType);
                    }
                catch (WRMNotSupportedException ex)
                    {
                    package.Workbook.Worksheets[0].Cells[row, 38].Value = ex.Message;
                    }
                }
            else
                {
                package.Workbook.Worksheets[0].Cells[row, 35].Value = "False";
                }

            string addressType = getAddressTypeFromWorksheet(row);

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
                        Address address = buildAddressFromWorksheet(row);
                        if (address == null)
                            {
                            package.Workbook.Worksheets[0].Cells[row, 38].Value = "Address is null";
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
                            package.Workbook.Worksheets[0].Cells[row, 38].Value = "Uhas Duplicate Address " + dictionaryKey;
                            //WRMLogger.LogBuilder.AppendLine("Address already added " + dictionaryKey);
                            continue;
                            }



                        // ADd Current Trash Serial Number

                        ExcelRange currentTrashCartSNDeliveryDateRange = package.Workbook.Worksheets[0].Cells[row, 6];
                        snSpreadsheetRow.CurrentTrashCartDeliveryDate = convertExcelRangeToString(currentTrashCartSNDeliveryDateRange);

                        ExcelRange currentTrashCartSNRange = package.Workbook.Worksheets[0].Cells[row, 7];
                        snSpreadsheetRow.CurrentTrashCartSN = convertExcelRangeToString(currentTrashCartSNRange);

                        ExcelRange firstTrashCartSNDeliveryDateRange = package.Workbook.Worksheets[0].Cells[row, 8];
                        snSpreadsheetRow.FirstTrashCartDeliveryDate = convertExcelRangeToString(firstTrashCartSNDeliveryDateRange);

                        ExcelRange firstTrashCartSNRange = package.Workbook.Worksheets[0].Cells[row, 9];
                        snSpreadsheetRow.FirstTrashCartSN = convertExcelRangeToString(firstTrashCartSNRange);

                        ExcelRange secondTrashCartSNDeliveryDateRange = package.Workbook.Worksheets[0].Cells[row, 10];
                        snSpreadsheetRow.SecondTrashCartDeliveryDate = convertExcelRangeToString(secondTrashCartSNDeliveryDateRange);

                        ExcelRange secondTrashCartSNRange = package.Workbook.Worksheets[0].Cells[row, 11];
                        snSpreadsheetRow.SecondTrashCartSN = convertExcelRangeToString(secondTrashCartSNRange);

                        ExcelRange thirdTrashCartSNDeliveryDateRange = package.Workbook.Worksheets[0].Cells[row, 12];
                        snSpreadsheetRow.ThirdTrashCartDeliveryDate = convertExcelRangeToString(thirdTrashCartSNDeliveryDateRange);

                        ExcelRange thirdTrashCartSNRange = package.Workbook.Worksheets[0].Cells[row, 13];
                        snSpreadsheetRow.ThirdTrashCartSN = convertExcelRangeToString(thirdTrashCartSNRange);

                        ExcelRange fourthTrashCartSNDeliveryDateRange = package.Workbook.Worksheets[0].Cells[row, 14];
                        snSpreadsheetRow.FourthTrashCartDeliveryDate = convertExcelRangeToString(fourthTrashCartSNDeliveryDateRange);

                        ExcelRange fourthTrashCartSNRange = package.Workbook.Worksheets[0].Cells[row, 15];
                        snSpreadsheetRow.FourthTrashCartSN = convertExcelRangeToString(fourthTrashCartSNRange);

                        ExcelRange currentRecyclingCartSNDeliveryDateRange = package.Workbook.Worksheets[0].Cells[row, 16];
                        snSpreadsheetRow.CurrentRecycleCartDeliveryDate = convertExcelRangeToString(currentRecyclingCartSNDeliveryDateRange);

                        ExcelRange currentRecyclingCartSNRange = package.Workbook.Worksheets[0].Cells[row, 17];
                        snSpreadsheetRow.CurrentRecycleCartSN = convertExcelRangeToString(currentRecyclingCartSNRange);

                        ExcelRange firstRecyclingCartSNDeliveryDateRange = package.Workbook.Worksheets[0].Cells[row, 18];
                        snSpreadsheetRow.FirstRecycleCartDeliveryDate = convertExcelRangeToString(firstRecyclingCartSNDeliveryDateRange);

                        ExcelRange firstRecyclingCartSNRange = package.Workbook.Worksheets[0].Cells[row, 19];
                        snSpreadsheetRow.FirstRecycleCartSN = convertExcelRangeToString(firstRecyclingCartSNRange);

                        ExcelRange secondRecyclingCartSNDeliveryDateRange = package.Workbook.Worksheets[0].Cells[row, 20];
                        snSpreadsheetRow.SecondTrashCartDeliveryDate = convertExcelRangeToString(secondRecyclingCartSNDeliveryDateRange);

                        ExcelRange secondRecyclingCartSNRange = package.Workbook.Worksheets[0].Cells[row, 21];
                        snSpreadsheetRow.SecondRecycleCartSN = convertExcelRangeToString(secondRecyclingCartSNRange);

                        ExcelRange thirdRecyclingCartSNDeliveryDateRange = package.Workbook.Worksheets[0].Cells[row, 22];
                        snSpreadsheetRow.ThirdTrashCartDeliveryDate = convertExcelRangeToString(secondRecyclingCartSNDeliveryDateRange);

                        ExcelRange thirdRecyclingCartSNRange = package.Workbook.Worksheets[0].Cells[row, 23];
                        snSpreadsheetRow.ThirdRecycleCartSN = convertExcelRangeToString(secondRecyclingCartSNRange);

                        ExcelRange multifamilyUnitSNRange = package.Workbook.Worksheets[0].Cells[row, 24];
                        snSpreadsheetRow.MultiFamilyUnit = convertExcelRangeToString(multifamilyUnitSNRange);

                        ExcelRange commercialSNRange = package.Workbook.Worksheets[0].Cells[row, 25];
                        snSpreadsheetRow.CommercialAccount = convertExcelRangeToString(commercialSNRange);

                        ExcelRange smallCartSNRange = package.Workbook.Worksheets[0].Cells[row, 26];
                        snSpreadsheetRow.SmallTrashCart = convertExcelRangeToString(smallCartSNRange);

                        ExcelRange specialtySNRange = package.Workbook.Worksheets[0].Cells[row, 27];
                        snSpreadsheetRow.Specialty = convertExcelRangeToString(specialtySNRange);

                        ExcelRange demolishedSNRange = package.Workbook.Worksheets[0].Cells[row, 28];
                        snSpreadsheetRow.AddressPreviouslyDemolished = convertExcelRangeToString(demolishedSNRange);

                        ExcelRange disapprovedSNRange = package.Workbook.Worksheets[0].Cells[row, 29];
                        snSpreadsheetRow.Disapproved = convertExcelRangeToString(disapprovedSNRange);

                        ExcelRange noCartSNRange = package.Workbook.Worksheets[0].Cells[row, 31];
                        snSpreadsheetRow.NoCartHere = convertExcelRangeToString(noCartSNRange);


                        ExcelRange notesSNRange = package.Workbook.Worksheets[0].Cells[row, 34];
                        snSpreadsheetRow.Notes = convertExcelRangeToString(notesSNRange);

                        snSpreadsheetRow.Address = address;
                        if (!rejectAddress(snSpreadsheetRow))
                            {
                            if (address.StreetName.Contains("?"))
                                {
                                dictionaryKey = "NOADDRESS" + snSpreadsheetRow.CurrentTrashCartSN;
                                }
                            snMasterDictionary.Add(dictionaryKey, snSpreadsheetRow);
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR C12: Cart Population At row " + row + " " + ex.Message);
                        package.Workbook.Worksheets[0].Cells[row, 38].Value = ex.Message;
                        package.Workbook.Worksheets[0].Cells[row, 35].Value = "False";

                        }
                    catch (WRMNotSupportedException ex)
                        {
                        package.Workbook.Worksheets[0].Cells[row, 38].Value = ex.Message;
                        package.Workbook.Worksheets[0].Cells[row, 35].Value = "True";
                        WRMLogger.LogBuilder.AppendLine("ERROR C13: Cart Population At row " + row + " " + ex.Message);

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

