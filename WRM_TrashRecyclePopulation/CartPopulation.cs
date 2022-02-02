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

        public Dictionary<int, Cart> cartPopulationDictionary;

        public Dictionary<int, Cart> recyclingCartPopulationDictionary;

        public List<int> cartsAddedDuringPopulation = new List<int>();

        public List<string> ignoreCartDuringPopulation = new List<string>();

        public static string xlsxSNMasterlistPath = @"C:\Users\rwaltz\Documents\SolidWaste\SN_MASTERLIST_Current.xlsx";
        // static private IEnumerable<KGISAddress> kgisCityResidentAddressList;
        private ExcelWorksheet worksheet;
        private int totalRowsWorksheet;
        private int firstWorksheetColumn;
        private int lastWorksheetColumn;
        //private ServiceTrashDayImporter serviceTrashDayImporter;

        // static public IEnumerable<KGISAddress> KgisCityResidentAddressList { get => kgisCityResidentAddressList; set => kgisCityResidentAddressList = value; }


        public CartPopulation()
            {

            xlsxSNMasterlistFileInfo = new FileInfo(xlsxSNMasterlistPath);
            if (!xlsxSNMasterlistFileInfo.Exists)
                {
                throw new Exception(xlsxSNMasterlistPath + "does not exist");
                }
            cartPopulationDictionary = new Dictionary<int, Cart>();
            recyclingCartPopulationDictionary = new Dictionary<int, Cart>();
            ExcelPackage package = new ExcelPackage(xlsxSNMasterlistFileInfo);
            worksheet = package.Workbook.Worksheets[0];
            totalRowsWorksheet = worksheet.Dimension.End.Row;
            firstWorksheetColumn = worksheet.Dimension.Start.Column;
            lastWorksheetColumn = worksheet.Dimension.End.Column;
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
                WRMLogger.Logger.logMessageAndDeltaTime("addAddressesFromSNMasterList ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                addAddressesFromSNMasterList();
                WRMLogger.Logger.logMessageAndDeltaTime("addFirstTrashCarts ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                addFirstTrashCarts();
//                WRMLogger.LogBuilder.AppendLine("addSecondTrashCarts");
                //          addSecondTrashCarts();
//                WRMLogger.LogBuilder.AppendLine("addThirdTrashCarts");
                //          addThirdTrashCarts();
//                WRMLogger.LogBuilder.AppendLine("addCurrentTrashCarts");
                //          addCurrentTrashCarts();
//                WRMLogger.LogBuilder.AppendLine("addFirstRecyclingCarts");

                //          addFirstRecyclingCarts();
//                WRMLogger.LogBuilder.AppendLine("addSecondRecyclingCarts");
                //          addSecondRecyclingCarts();
//                WRMLogger.LogBuilder.AppendLine("addCurrentTrashCarts");
                //          addCurrentRecyclingCarts();
                //update any addresses with new Address types, commercial and specialty


                // initial inserts..  the fields will be different depending on how many carts have been issued to an address

                // delete initial inserts and insert new cart round 2

                //delete round 2 carts and insert new cart round 3



                /*
logLine = "Start Cart Population";
WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
for (int row = 2; row <= totalRowsWorksheet; row++)
{
try
{
if (maxToProcess % 200 == 0)
    {
    logLine = "Cart Population " + maxToProcess;
    WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
    WRMLogger.Logger.log();
    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
    //         break;
    ++maxToProcess;
    }
// has the default manner to 
Address address = createAddressFromWorksheet(worksheet, row);
string addressType = populateAddressTypeFromWorksheet(worksheet, row);
int foundAddressId = 0;
if (address == null)
    {
    continue;
    }
string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);

if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out foundAddressId))
    {
    throw new WRMNullValueException("AddressId not found in AddressIdentiferDictionary " + dictionaryKey);
    }

string trashdictionaryKey = dictionaryKey + "TRASH";

if (!this.cartPopulationDictionary.ContainsKey(trashdictionaryKey))
    {
    //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
    Cart trashCartHistory = buildCart(row, foundAddressId, 8, 9, false, true);
    if (trashCartHistory != null)
        {
        saveAndDeleteCart(trashCartHistory);
        }
    trashCartHistory = buildCart(row, foundAddressId, 10, 11, false, true);
    if (trashCartHistory != null)
        {
        saveAndDeleteCart(trashCartHistory);
        }
    trashCartHistory = buildCart(row, foundAddressId, 12, 13, false, true);
    if (trashCartHistory != null)
        {
        saveAndDeleteCart(trashCartHistory);
        }
    Cart trashcart = buildCart(row, foundAddressId, 6, 7, false, false);
    if (trashcart != null)
        {

        trashcart = saveCart(trashcart);
        this.cartPopulationDictionary.Add(trashdictionaryKey, trashcart);
        }
    else
        {
        WRMLogger.LogBuilder.AppendLine("NO TRASH CART FOR ADDRESS [" + address.StreetNumber + "] [" + address.StreetName + "] [" + address.UnitNumber + "] [" + address.ZipCode + "] \n");
        }
    }
string recycledictionaryKey = dictionaryKey + "RECYCLE";
if (!this.cartPopulationDictionary.ContainsKey(recycledictionaryKey))
    {
    //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
    Cart recyclingCartHistory = buildCart(row, address.AddressID, 16, 17, true, true);
    if (recyclingCartHistory != null)
        {
        recyclingCartHistory = saveAndDeleteCart(recyclingCartHistory);
        }

    recyclingCartHistory = buildCart(row, address.AddressID, 18, 19, true, true);
    if (recyclingCartHistory != null)
        {
        recyclingCartHistory = saveAndDeleteCart(recyclingCartHistory);
        }
    Cart recyclingCart = buildCart(row, address.AddressID, 14, 15, true, true);
    if (recyclingCart != null)
        {
        recyclingCart = saveCart(recyclingCart);
        this.cartPopulationDictionary.Add(recycledictionaryKey, recyclingCart);

        }
    }

                }
            catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Cart Population At row " + row + " " + ex.Message);

                        }
                    catch (WRMNotSupportedException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Cart Population At row " + row + " " + ex.Message );

                        }
                    //                    WRMLogger.Logger.log();
                    }

                */
                justNow = DateTime.Now;
                timeDiff = justNow - beforeNow;
                WRMLogger.LogBuilder.AppendLine("End " + justNow.ToString("o", new CultureInfo("en-us")) + "Total MilliSeconds passed : " + timeDiff.TotalMilliseconds.ToString());
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

                addCarts(6, 7, false, false);


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

                //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                addCarts(8, 9, false, true);

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

                addCarts(10, 11, false, true);
                //                    WRMLogger.Logger.log();
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
                addCarts(12, 13, false, true);

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
                addCarts(14, 15, true, false);
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
        public void addFirstRecyclingCarts()
            {
            try
                {

                //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                addCarts(16, 17, true, true);

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

                addCarts(18, 19, true, true);
                //                    WRMLogger.Logger.log();

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
                for (int row = 2; row <= totalRowsWorksheet; row++)
                    {
                    try
                        {
                        if (maxToProcess % 1000 == 0)
                            {
                            logLine = "Cart Population " + maxToProcess;
                            WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                            WRMLogger.Logger.log();
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);

                            }

                        // has the default manner to 
                        Address address = createAddressFromWorksheet(worksheet, row);
                        if (address == null)
                            {
                            continue;
                            }
                        string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                        if (rejectAddress(worksheet, row))
                            {
                            ignoreCartDuringPopulation.Add(dictionaryKey);
                            continue;
                            }
                        string addressType = populateAddressTypeFromWorksheet(worksheet, row);

                        Address foundAddress = new Address();

                        if (AddressPopulation.AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                            {
                            // overwrite address type if populated in spreadsheet
                            if (!(String.IsNullOrEmpty(addressType)) && !addressType.Equals(foundAddress.AddressType))

                                {

                                AddressPopulation.AddressDictionary[dictionaryKey].AddressType = addressType;
                                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[dictionaryKey]);
                                ++maxToProcess;
                                }

                            }
                        else
                            {
                            if (String.IsNullOrEmpty(address.AddressType) && String.IsNullOrEmpty(addressType))
                                {
                                throw new WRMNullValueException("Address Type is null for " + dictionaryKey);
                                }
                            else if (!String.IsNullOrEmpty(addressType) && !addressType.Equals(address.AddressType))
                                {
                                address.AddressType = addressType;
                                }
                            ++maxToProcess;
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(address);

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
                WRMLogger.LogBuilder.AppendLine("End " + justNow.ToString("o", new CultureInfo("en-us")) + "Total MilliSeconds passed : " + timeDiff.TotalMilliseconds.ToString());
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

        public bool rejectAddress(ExcelWorksheet worksheet, int row)
            {
            bool rejectAddress = false;

            ExcelRange addressDemolished = worksheet.Cells[row, 24];
            if (addressDemolished != null && addressDemolished.Value != null)
                {
                if (!String.IsNullOrEmpty(addressDemolished.Value.ToString()) && addressDemolished.Value.ToString().Equals("1"))
                    {
                    rejectAddress = true;
                    }
                }
            ExcelRange addressDisapproved = worksheet.Cells[row, 25];
            if (addressDisapproved != null && addressDisapproved.Value != null)
                {
                if (!String.IsNullOrEmpty(addressDisapproved.Value.ToString()) && addressDisapproved.Value.ToString().Equals("1"))
                    {
                    rejectAddress = true;
                    }
                }

            ExcelRange addressRefusedService = worksheet.Cells[row, 26];
            if (addressRefusedService != null && addressRefusedService.Value != null)
                {
                if (!String.IsNullOrEmpty(addressRefusedService.Value.ToString()) && addressRefusedService.Value.ToString().Equals("1"))
                    {
                    rejectAddress = true;
                    }
                }

            return rejectAddress;
            }

        public void addCarts(int trashDeliveryDateCellId, int cartSNCellId, bool isRecyclingCart, bool failUnknown)
            {
            try
                {

                cartsAddedDuringPopulation.Clear();

                int maxToProcess = 0;
                for (int row = 2; row <= totalRowsWorksheet; row++)
                    {
                    try
                        {

                        if (maxToProcess % 1000 == 0)
                            {
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);

                            }
                        Address address = createAddressFromWorksheet(worksheet, row);
                        if (address == null)
                            {
                            throw new WRMNullValueException("Address null in worksheet ");
                            }
                        int addressId = 0;
                        string addressKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);


                        if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(addressKey, out addressId))
                            {
                            throw new WRMNullValueException("Address cannot be found in AddressIdentiferDictionary " + addressKey);
                            }
                        if ((ignoreCartDuringPopulation.IndexOf(addressKey) >= 0) || (cartsAddedDuringPopulation.IndexOf(addressId) >= 0))
                            {
                            WRMLogger.LogBuilder.AppendLine("Ignoring cart at Address: " + addressKey);
                            continue;
                            }
                        Cart cart = buildCart(row, addressId, trashDeliveryDateCellId, cartSNCellId, isRecyclingCart, failUnknown); 
                        if (cart == null)
                            {
                            WRMLogger.LogBuilder.AppendLine("Build cart failed at Address: " + addressKey);
                            continue;
                            }

                        saveCart(cart);

                            cartsAddedDuringPopulation.Add(addressId);

                        ++maxToProcess;
                        }
                    catch (WRMNullValueException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Cart Population At row " + row + " " + ex.Message);

                        }
                    catch (WRMNotSupportedException ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Cart Population At row " + row + " " + ex.Message);

                        }
                    //                    WRMLogger.Logger.log();
                    }

                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
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
        public Cart buildCart(dynamic row, int addressId, int trashDeliveryDateCellId, int trashCartSNCellId, bool isRecyclingCart, bool failUnknown)
            {

            bool unknown = false;

            ExcelRange currentTrashCartSN = worksheet.Cells[row, trashCartSNCellId];

            // This the exception to the failUnknown being true
            // The spreadsheet indicates there was a cart at the address, but there no longer is one.  So, we should create the cart instead of return null
            if (currentTrashCartSN.Style.Fill.BackgroundColor.LookupColor().Equals("#FFFFFF00"))
                {
                WRMLogger.LogBuilder.AppendLine("Found line that is an unknown cart AddressID = " + addressId);
                //This color means unable to identify correct/current serial number since IPL delivered 2 in the original rollout-- need to look into in person. Could also mean we need to know what SN is there because nothing was recorded.
                unknown = true;

                }


            Cart cart = new Cart();

            if (unknown)
                {
                cart.CartSerialNumber = "UNKNOWN";
                }
            else
                {
                if (currentTrashCartSN != null && currentTrashCartSN.Value != null)
                    {
                    string serialNumber = currentTrashCartSN.Value.ToString();
                    if (!String.IsNullOrWhiteSpace(serialNumber))
                        {
                        if (serialNumber.Equals("NO TRASH"))
                            {
                            throw new WRMNullValueException("Serial Number is NO TRASH in row " + row);
                            }
                        else
                            {
                            cart.CartSerialNumber = currentTrashCartSN.Value.ToString();
                            }
                        }
                    else
                        {
                        if (failUnknown)
                            {
                            return null;
                            }
                        cart.CartSerialNumber = "UNKNOWN";
                        }
                    }
                else
                    {
                    if (failUnknown)
                        {
                        return null;
                        }
                    cart.CartSerialNumber = "UNKNOWN";
                    }

                }

            ExcelRange currentTrashDeliveryDate = worksheet.Cells[row, trashDeliveryDateCellId];
            if (currentTrashDeliveryDate != null && currentTrashDeliveryDate.Value != null)
                {
                string currentTrashDeliveryDateString = currentTrashDeliveryDate.Value.ToString();
                try
                    {
                    if (validateDateString(currentTrashDeliveryDateString))
                        {
                        cart.SerialNumberReceivedDate = DateTime.Parse(currentTrashDeliveryDateString);
                        }
                    else
                        {
                        WRMLogger.LogBuilder.AppendLine("Cart Date recieved is invalid " + currentTrashDeliveryDateString);
                        }
                    }
                catch (FormatException ex)
                    {
                    WRMLogger.LogBuilder.AppendLine(ex.Message);
                    WRMLogger.LogBuilder.AppendLine(ex.ToString());
                    Exception inner = ex.InnerException;
                    if (inner != null)
                        {
                        WRMLogger.LogBuilder.AppendLine(inner.Message);
                        WRMLogger.LogBuilder.AppendLine(inner.ToString());
                        }
                    }
                }
            if (isRecyclingCart)
                {
                cart.IsRecyclingCart = true;
                }
            else
                {
                cart.IsRecyclingCart = false;
                }

            cart.AddressID = addressId;
            cart.CreateDate = DateTime.Now;
            cart.CreateUser = "WRM_TrashRecyclePopulation";
            cart.UpdateDate = DateTime.Now;
            cart.UpdateUser = "WRM_TrashRecyclePopulation";

            return cart;
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
        /*
        public Cart saveAndDeleteCart(Cart cart, int cartDictionaryKey)
            {
            cart = saveCart(cart);
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Remove(cartPopulationDictionary[cartDictionaryKey]);

            return cart;
            }
        */
        private string populateAddressTypeFromWorksheet(ExcelWorksheet worksheet, int row)
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

        private Address createAddressFromWorksheet(ExcelWorksheet worksheet, int row)
            {


            // First determine if address should be added or ignored. if ignored, then return null
            for (int i = firstWorksheetColumn; i <= lastWorksheetColumn; ++i)
                {

                ExcelRange excelCell = worksheet.Cells[row, i];
                if (excelCell.Style.Font.Strike)
                    {
                    return null;
                    }
                if (excelCell.Style.Fill.BackgroundColor.LookupColor() != null)
                    {
                    switch (excelCell.Style.Fill.BackgroundColor.LookupColor())
                        {
                        // ignore white background
                        case "#FF000000":

                            if (excelCell.Style.Font.Color.LookupColor().Equals("#FFFF0000"))
                                {
                                // this color only affects the cell/ not the full row
                                //This color means this cart has been landfilled/packed
                                return null;
                                }
                            break;
                        case "#FF7F6000":
                            // this means that trash pickup box is checked.
                            //This color means the home owner no longer wanted to use City waste services and decided to privately contract on their own. SR (if available) given in Notes.
                            return null;
                            break;

                        case "#FFFF9999":
                            // This color means that two carts were improperly at this address so WC removed the highlighted cart. 
                            return null;
                            break;

                        case "#FF0070C0":
                            // This color means address is not eligible for City services and the cart has been retrieved (different from other blue highlight in that we have confirmation the cart has been retrieved)
                            return null;
                            break;

                        default:
                            break;
                        }

                    }

                }


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

            return address;
            }
        static bool validateDateString(string dateValue)
            {
            string[] formats = { "MM/dd/yyyy" };
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

        }

    }