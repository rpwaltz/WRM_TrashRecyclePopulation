using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using OfficeOpenXml;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation
    {

    class CartPopulation
        {
        private FileInfo xlsxSNMasterlistFileInfo;

        public Dictionary<string, Cart> cartPopulationDictionary;

        public static string xlsxSNMasterlistPath = @"C:\Users\rwaltz\Documents\SolidWaste\SN_MASTERLIST_Current.xlsm";
        static private IEnumerable<KgisResidentAddressView> kgisCityResidentAddressList;
        private ExcelWorksheet worksheet;
        private int totalRowsWorksheet;
        private int firstWorksheetColumn;
        private int lastWorksheetColumn;
        private WRM_EntityFramework.WRM_TrashRecycle.WRM_TrashRecycle wrmTrashRecycleContext;
        private ServiceTrashDayImporter serviceTrashDayImporter;
        private WRM_TrashRecycleQueries wrm_TrashRecycleQueries;
        private int cartHistoryCartID = 0;
        static public IEnumerable<KgisResidentAddressView> KgisCityResidentAddressList { get => kgisCityResidentAddressList; set => kgisCityResidentAddressList = value; }
        private WRM_TrashRecycleQueries Wrm_TrashRecycleQueries { get => wrm_TrashRecycleQueries; set => wrm_TrashRecycleQueries = value; }

        public CartPopulation(WRM_EntityFramework.WRM_TrashRecycle.WRM_TrashRecycle wrmTrashRecycleContext)
            {
            this.wrmTrashRecycleContext = wrmTrashRecycleContext;
            Wrm_TrashRecycleQueries = new WRM_TrashRecycleQueries(wrmTrashRecycleContext);
            if (KgisCityResidentAddressList == null)
                {
                KgisCityResidentAddressList = Wrm_TrashRecycleQueries.retrieveKgisCityResidentAddress();
                }
            this.serviceTrashDayImporter = ServiceTrashDayImporter.getServiceTrashDayImporter();
            xlsxSNMasterlistFileInfo = new FileInfo(xlsxSNMasterlistPath);
            if (!xlsxSNMasterlistFileInfo.Exists)
                {
                throw new Exception(xlsxSNMasterlistPath + "does not exist");
                }
            cartPopulationDictionary = new Dictionary<string, Cart>();

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

                logLine = "begin populateCarts";
                //               WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref beforeNow, ref justNow, ref loopMillisecondsPast);
                for (int row = 2; row <= totalRowsWorksheet; row++)
                    {
                                  //     if (maxToProcess >= 1000)
                                  //         {
                    //
                                   //       break;
                                   //     }
                    ++maxToProcess;

                    // has the default manner to 
                    Address address = extractAddressFromWorksheet(worksheet, row);

                    if (address == null)
                        {
                        continue;
                        }
                    string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);

                    if (AddressPopulation.AddressDictionary.ContainsKey(dictionaryKey))
                        {
                        address = AddressPopulation.AddressDictionary[dictionaryKey];
                        }
                    else
                        {
                        address = populateAddress(dictionaryKey, address);

                        if (address == null)
                            {
                            continue;
                            }

                        }

                    string trashdictionaryKey = dictionaryKey + "TRASH";
                    if (!this.cartPopulationDictionary.ContainsKey(trashdictionaryKey))
                        {
                        //adding to Cart and then deleting allows for the trigger to create a cart history id for the record
                        Cart trashCartHistory = buildCart(row, address.AddressId, 8, 9, false, true);
                        if (trashCartHistory != null)
                            {
                            saveAndDeleteCart(trashCartHistory);
                            }
                        trashCartHistory = buildCart(row, address.AddressId, 10, 11, false, true);
                        if (trashCartHistory != null)
                            {
                            saveAndDeleteCart(trashCartHistory);
                            }
                        trashCartHistory = buildCart(row, address.AddressId, 12, 13, false, true);
                        if (trashCartHistory != null)
                            {
                            saveAndDeleteCart(trashCartHistory);
                            }
                        Cart trashcart = buildCart(row, address.AddressId, 6, 7, false, false);
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
                        Cart recyclingCartHistory = buildCart(row, address.AddressId, 16, 17, true, true);
                        if (recyclingCartHistory != null)
                            {
                            recyclingCartHistory = saveAndDeleteCart(recyclingCartHistory);
                            }

                        recyclingCartHistory = buildCart(row, address.AddressId, 18, 19, true, true);
                        if (recyclingCartHistory != null)
                            {
                            recyclingCartHistory = saveAndDeleteCart(recyclingCartHistory);
                            }
                        Cart recyclingCart = buildCart(row, address.AddressId, 14, 15, true, true);
                        if (recyclingCart != null)
                            {
                            recyclingCart = saveCart(recyclingCart);
                            this.cartPopulationDictionary.Add(recycledictionaryKey, recyclingCart);
                            }
                        }
                    //                    WRMLogger.Logger.log();
                    }
                logLine = "end populateCarts";
                //                WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref beforeNow, ref justNow, ref loopMillisecondsPast);
                WRMLogger.Logger.log();
                }
            catch (Exception e)
                {

                WRMLogger.LogBuilder.AppendLine(e.StackTrace);
                Exception inner = e.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }
                WRMLogger.Logger.log();
                throw e;
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
                //This color means unable to identify correct/current serial number since IPL delivered 2 in the original rollout-- need to look into in person. Could also mean we need to know what SN is there because nothing was recorded.
                unknown = true;
               
                }


            Cart cart = new Cart();
            ExcelRange currentTrashDeliveryDate = worksheet.Cells[row, trashDeliveryDateCellId];
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
                            return null;
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


            if (currentTrashDeliveryDate != null && currentTrashDeliveryDate.Value != null)
                {
                string currentTrashDeliveryDateString = currentTrashDeliveryDate.Value.ToString();
                try
                    {
                    cart.SerialNumberReceivedDate = DateTime.Parse(currentTrashDeliveryDateString);
                    }
                catch (FormatException ex)
                    {
                    WRMLogger.LogBuilder.AppendLine(ex.ToString());
                    Exception inner = ex.InnerException;
                    if (inner != null)
                        {

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
            
            cart.AddressId = addressId;
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
                return null;
                }
            wrmTrashRecycleContext.Add(cart);
            // logBuilder.AppendLine("Add " + request.StreetNumber + "  " + request.StreetName);
            wrmTrashRecycleContext.SaveChanges();
            wrmTrashRecycleContext.ChangeTracker.DetectChanges();


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
                    additionalCart.AddressId = cart.AddressId;
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

        public Cart saveAndDeleteCart(Cart cart)
            {
            cart = saveCart(cart);
            wrmTrashRecycleContext.Remove(cart);
            // logBuilder.AppendLine("Add " + request.StreetNumber + "  " + request.StreetName);
            wrmTrashRecycleContext.SaveChanges();
            wrmTrashRecycleContext.ChangeTracker.DetectChanges();
            return cart;
            }
        private Address extractAddressFromWorksheet(ExcelWorksheet worksheet, int row)
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
            if (streetNameNumberCell == null || streetNameNumberCell.Value == null || streetNameCell == null || streetNameCell.Value == null || String.IsNullOrEmpty(streetNameCell.Value.ToString()) || String.IsNullOrEmpty(streetNumberCell.Value.ToString()))
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
            else if (!(streetNumberCell == null || streetNumberCell.Value == null || streetNameCell == null || streetNameCell.Value == null || 
                String.IsNullOrEmpty(streetNameCell.Value.ToString()) && String.IsNullOrEmpty(streetNumberCell.Value.ToString())))
                {
                string streetName = streetNameCell.Value.ToString();
                address.StreetName = IdentifierProvider.normalizeStreetName(streetName);

                string streetNumber = streetNumberCell.Value.ToString();
                int streetNumberInt32 = IdentifierProvider.normalizeStreetNumber(streetNumber);

                address.StreetNumber = streetNumberInt32;
                }
            else
                {
                return null;
                }
            ExcelRange unitCell = worksheet.Cells[row, 4];
            if (unitCell.Value != null)
                {
                string unitNumber = unitCell.Value.ToString();
                address.UnitNumber = IdentifierProvider.normalizeUnitNumber(unitNumber);
                }
            ExcelRange zipCell = worksheet.Cells[row, 5];
            if (zipCell.Value != null)
                {

                String zipString = zipCell.Value.ToString();
                address.ZipCode = IdentifierProvider.normalizeZipCode(zipString);

                }
            return address;
            }

        /* if you recieve a NotSupportedException trying to find an address type, address is assigned null, an error is printed, and processing goes on to the next record */
        private Address populateAddress(String dictionaryKey, Address address)
            {
            address = getAddressFromGIS(address);
            if (address != null)
                {
                wrmTrashRecycleContext.Add(address);
                //                WRMLogger.LogBuilder.AppendLine("Adding " + dictionaryKey);
                wrmTrashRecycleContext.SaveChanges();
                wrmTrashRecycleContext.ChangeTracker.DetectChanges();

                AddressPopulation.AddressDictionary[dictionaryKey] = address;
                }
            return address;
            }
        /* if you recieve a NotSupportedException trying to find an address type, address is assigned null, an error is printed, and processing goes on to the next record */
        private Address getAddressFromGIS(Address address)
            {
            if (address == null || String.IsNullOrEmpty(address.StreetName))
                {
                return null;
                }
            address.StreetName = address.StreetName.ToUpper().Trim();
            //KgisCityResidentAddressList
            //  IEnumerable<KgisResidentAddressView> foundKgisResidentAddress = kgisCityResidentAddressList.Where(req => req.StreetName.ToUpper().Equals(request.StreetName.ToUpper()));
            IEnumerable<KgisResidentAddressView> foundKgisResidentAddress =
                from req in kgisCityResidentAddressList
                where Decimal.ToInt32(req.AddressNum ?? 0) == address.StreetNumber && req.StreetName.Equals(address.StreetName) && req.Jurisdiction == 1 && req.AddressStatus == 2
                select req;

            IEnumerator<KgisResidentAddressView> foundKgisResidentAddressEnumerator = foundKgisResidentAddress.GetEnumerator();


            int countFoundAddresses = foundKgisResidentAddress.Count();

            switch (countFoundAddresses)
                {
                case 0:
                    if (Wrm_TrashRecycleQueries.determineAddressFailure(address))
                        {
                        WRMLogger.LogBuilder.AppendLine(" FOR ADDRESS [" + address.StreetNumber + "] [" + address.StreetName + "] [" + address.UnitNumber + "] [" + address.ZipCode + "]! \n");

                        }
                    else
                        {
                        WRMLogger.LogBuilder.AppendLine("ADDRESS DOES NOT EXIST [" + address.StreetNumber + "] [" + address.StreetName + "] [" + address.UnitNumber + "] [" + address.ZipCode + "] does not exist! \n");
                        }
                    address = null;
                    break;
                case 1:
                    try
                        {
                        address = this.buildAddressFromEnumerator(address, foundKgisResidentAddress);
                        }
                    catch (NotSupportedException nex)
                        {

                        WRMLogger.LogBuilder.AppendLine("TYPE ADDRESS IS NOT VALID. [" + address.StreetNumber + "][" + address.StreetName + "][" + address.UnitNumber + "][" + address.AddressType + "]  ");
                        address = null;
                        WRMLogger.Logger.log();
                        }
                    catch (Exception ex)
                        {
                        WRMLogger.LogBuilder.AppendLine(ex.ToString());
                        Exception inner = ex.InnerException;
                        if (inner != null)
                            {
                            WRMLogger.LogBuilder.AppendLine(inner.ToString());
                            }
                        if (address != null)
                            {
                            WRMLogger.LogBuilder.AppendLine("ADDRESS IS NOT VALID. [" + address.StreetNumber + "][" + address.StreetName + "][" + address.UnitNumber + "][" + address.ZipCode + "] does not exist!");
                            }
                        WRMLogger.Logger.log();
                        throw ex;
                        }

                    // request.FirstName; request.LastName, request.PhoneNumber; request.Email;
                    break;
                case 2:
                case 3:
                case 4:
                    try
                        {
                        address = this.buildAddressWithUnitFromEnumerator(address, foundKgisResidentAddress);

                        }
                    catch (NotSupportedException nex)
                        {

                        WRMLogger.LogBuilder.AppendLine("TYPE ADDRESS IS NOT VALID. [" + address.StreetNumber + "][" + address.StreetName + "][" + address.UnitNumber + "] ");
                        address = null;
                        }
                    catch (Exception ex)
                        {
                        WRMLogger.LogBuilder.AppendLine(ex.ToString());
                        Exception inner = ex.InnerException;
                        if (inner != null)
                            {
                            WRMLogger.LogBuilder.AppendLine(inner.ToString());
                            }
                        if (address != null)
                            {
                            WRMLogger.LogBuilder.AppendLine("ADDRESS IS NOT VALID. [" + address.StreetNumber + "][" + address.StreetName + "][" + address.UnitNumber + "][" + address.ZipCode + "] does not exist!");
                            }
                        WRMLogger.Logger.log();
                        throw ex;
                        }
                    break;
                default:

                    WRMLogger.LogBuilder.AppendLine("MORE THAN FOUR UNITS [" + address.StreetNumber + "] [" + address.StreetName + "] [" + address.UnitNumber + "] has more than 4  units!");
                    address = null;
                    break;
                }
            return address;
            }
        //  //  IEnumerable<KgisResidentAddressView> foundKgisResidentAddress

        private Address buildAddressWithUnitFromEnumerator(Address address, IEnumerable<KgisResidentAddressView> foundKgisResidentAddress)
            {


            if (foundKgisResidentAddress.Count() > 0)
                {
                if (!String.IsNullOrEmpty(address.UnitNumber))
                    {
                    string UnitNumber = address.UnitNumber.ToUpper();
                    // Need to make certain that KgisResidentAddressView does have a Unit. Just because we find one residence has a unit does not mean all addresses at the street number have units.
                    // there might be a primary address that has two apartments and each have their own cart
                    IEnumerable<KgisResidentAddressView> foundKgisCityResidentAddressUnitEnumerable =
                        from req in foundKgisResidentAddress
                        where (!(String.IsNullOrEmpty(req.Unit)) && req.Unit.Equals(UnitNumber))
                        select req;


                    if (foundKgisCityResidentAddressUnitEnumerable.Count() == 1)
                        {

                        KgisResidentAddressView kgisCityResidentAddressUnit = foundKgisCityResidentAddressUnitEnumerable.First();
                        address = buildAddress(kgisCityResidentAddressUnit);
                        //                        WRMLogger.LogBuilder.AppendLine("FOUND IN KGIS [" + address.StreetNumber + "][" + address.StreetName + "][" + address.UnitNumber + "] ");
                        }
                    else
                        {
                        // Did not Find the Unit Address in KGIS . Use the Address parsed from Excel.


                        /*
                                    IEnumerable<KgisResidentAddressView> foundKgisResidentAddressUnit =
                            from req in foundKgisResidentAddressEnumerator
                            where Decimal.ToInt32(req.AddressNum ?? 0) == address.StreetNumber && req.StreetName.Equals(address.StreetName)
                            select req;
                        */

                        // this should loop to find all the addresses.

                        Address newAddress = buildAddress(foundKgisResidentAddress.First());
                        newAddress.UnitNumber = address.UnitNumber;
                        // Now that we have a GIS address, attempt to find it again.
                        string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(newAddress.StreetName, newAddress.StreetNumber, newAddress.UnitNumber, newAddress.ZipCode);

                        if (AddressPopulation.AddressDictionary.ContainsKey(dictionaryKey))
                            {
                            address = AddressPopulation.AddressDictionary[dictionaryKey];
                            }
                        else
                            {
                            address = newAddress;
                            }
                        //                       WRMLogger.LogBuilder.AppendLine("IN KGIS [" + address.StreetNumber + "][" + address.StreetName + "] with [" + address.UnitNumber + "] does not exist! ");
                        }
                    }
                else
                    {
                    address = buildAddressFromEnumerator(address, foundKgisResidentAddress);
                    }
                }
            else
                {
                throw new Exception("foundKgisResidentAddress.Count is 0");
                }
            return address;
            // potentially multiple address, create one for each

            }
        private Address buildAddressFromEnumerator(Address address, IEnumerable<KgisResidentAddressView> foundKgisResidentAddress)
            {

            IEnumerator<KgisResidentAddressView> foundKgisResidentAddressEnumerator = foundKgisResidentAddress.GetEnumerator();

            if (foundKgisResidentAddressEnumerator.Current == null)
                {
                foundKgisResidentAddressEnumerator.MoveNext();

                }

            KgisResidentAddressView kgisCityResidentAddress = foundKgisResidentAddressEnumerator.Current;
            if (kgisCityResidentAddress == null) throw new Exception("Can not find KgisResidentAddressView");
            address = buildAddress(kgisCityResidentAddress);
            return address;


            }
        public Address buildAddress(KgisResidentAddressView kgisCityResidentAddress)
            {
            Address address = new Address();
            address.AddressType = AddressPopulation.translateAddressTypeFromKGISAddressUse(kgisCityResidentAddress);

            address.GisparcelId = kgisCityResidentAddress.Parcelid;

            address.StreetName = IdentifierProvider.normalizeStreetName(kgisCityResidentAddress.StreetName);
            address.StreetNumber = Convert.ToInt32(kgisCityResidentAddress.AddressNum);
            address.ZipCode = kgisCityResidentAddress.ZipCode.ToString();
            address.GisaddressUseType = kgisCityResidentAddress.AddressUseType;

            address.Gislatitude = kgisCityResidentAddress.Latitude;
            address.Gislongitude = kgisCityResidentAddress.Longitude;
            address.GispointX = kgisCityResidentAddress.PointX;
            address.GispointY = kgisCityResidentAddress.PointY;

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
            //            WRMLogger.LogBuilder.AppendLine(dictionaryKey + " has a type of " + address.AddressType);
            if (this.serviceTrashDayImporter.addressDictionary.ContainsKey(dictionaryKey))
                {

                Address serviceDayAddress = this.serviceTrashDayImporter.addressDictionary[dictionaryKey];
                if (serviceDayAddress.RecyclingPickup ?? false)
                    {
                    address.RecycleDayOfWeek = serviceDayAddress.RecycleDayOfWeek;
                    address.RecycleFrequency = serviceDayAddress.RecycleFrequency;
                    }

                if (address.TrashPickup ?? false)
                    {
                    address.TrashPickup = serviceDayAddress.TrashPickup;
                    address.TrashDayOfWeek = serviceDayAddress.TrashDayOfWeek;
                    }

                }
            else
                {
                // log that the address does not have a service day
                }

            return address;

            }
        }

    }
