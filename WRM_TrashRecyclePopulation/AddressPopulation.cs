using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;
using OfficeOpenXml;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;
using Newtonsoft.Json;

namespace WRM_TrashRecyclePopulation
    {
    class AddressPopulation
        {


        private static ServiceTrashDayImporter serviceTrashDayImporter = null;
        private static Dictionary<string, Address> addressDictionary = new Dictionary<string, Address>();

        public static Dictionary<string, Address> AddressDictionary { get => addressDictionary; set => addressDictionary = value; }


        private static Dictionary<string, int> addressIdentiferDictionary = new Dictionary<string, int>();
        public static Dictionary<string, int> AddressIdentiferDictionary { get => addressIdentiferDictionary; set => addressIdentiferDictionary = value; }

        private static Dictionary<int, string> reverseAddressIdentiferDictionary = new Dictionary<int, string>();
        public static Dictionary<int, string> ReverseAddressIdentiferDictionary { get => reverseAddressIdentiferDictionary; set => reverseAddressIdentiferDictionary = value; }
        public AddressPopulation()
            {
            if (serviceTrashDayImporter == null)
                {
                serviceTrashDayImporter = new ServiceTrashDayImporter();

                }

            }
        public void populateAddresses()
            {
            Program.logLine = "ADDRESS POPULATION:  ";
            WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
            WRMLogger.Logger.log();

            addTrashServiceAddressesFromWorksheetIntoDictionary(serviceTrashDayImporter.TrashServiceDayWorksheet);
            addRecycleServiceAddressesFromWorksheetIntoDictionary(serviceTrashDayImporter.RecycleServiceDayWorksheet);
            int bulkSaveCount = 0;
            foreach (Address address in addressDictionary.Values)
                {
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(address);
                if (bulkSaveCount % 1000 == 0)
                    {
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    Program.logLine = "ADDRESS POPULATION: Save Count: " + bulkSaveCount;
                    WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                    WRMLogger.Logger.log();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);

                    }
                ++bulkSaveCount;

                }

            Program.logLine = "ADDRESS POPULATION: Total Address Count: " + bulkSaveCount;
            WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
            WRMLogger.Logger.log();
            foreach (Address address in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Address.ToList())
                {
                string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                addressIdentiferDictionary[dictionaryKey] = address.AddressID;
                ReverseAddressIdentiferDictionary[address.AddressID] = dictionaryKey;
                }

            }


        private void addTrashServiceAddressesFromWorksheetIntoDictionary(ExcelWorksheet worksheet)
            {

            int rowCount = worksheet.Dimension.End.Row;     //get row count
                                                            //    Program.logLine = "ADDRESS POPULATION: TrashAddress Count: " + rowCount;
                                                            //    WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                                                            //    WRMLogger.Logger.log();
            for (int row = 2; row <= rowCount; row++)
                {
                Address address = null;
                string dictionaryKey = null;
                try
                    {
                    address = serviceTrashDayImporter.createAddressFromServiceDayWorksheet(worksheet, row);
                    populateAddressFromKGIS(ref address);
                    dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                    }
                catch (WRMNotSupportedException ex)
                    {
                    WRMLogger.LogBuilder.AppendLine("TRASH ADDRESS POPULATION ERROR: At row " + row + " " + ex.Message);
                    continue;
                    }
                catch (WRMNullValueException ex)
                    {
                    WRMLogger.LogBuilder.AppendLine("TRASH ADDRESS POPULATION ERROR: At row " + row + " " + ex.Message);
                    continue;
                    }
                try
                    {
                    /* IEnumerable<KGISAddress> foundKgisResidentAddress =
                        from req in kgisCityResidentAddressList
                        where Decimal.ToInt32(req.ADDRESS_NUM ?? 0) == address.StreetNumber && req.STREET_NAME.Equals(address.StreetName)
                            && req.ZIP_CODE.ToString().Substring(1, 5) == address.ZipCode && req.JURISDICTION == 1 && req.ADDRESS_STATUS == 2
                        select req; */

                    Address foundAddress = new Address();
                    if (!AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                        {
                        ExcelRange trashSchedule = worksheet.Cells[row, 5];
                        if (trashSchedule != null && trashSchedule.Value != null)
                            {
                            if (validateDayOfWeek(trashSchedule.Value.ToString()))
                                {
                                address.TrashPickup = true;
                                address.TrashDayOfWeek = trashSchedule.Value.ToString().ToUpper();
                                }
                            else
                                {
                                address.TrashPickup = false;
                                throw new WRMNotSupportedException("TRASH ADDRESS POPULATION: Invalid TrashSchedule day of Week");
                                }
                            }
                        else
                            {
                            address.TrashPickup = false;
                            WRMLogger.LogBuilder.AppendLine("WARNING: At row " + row + " TRASH ADDRESS POPULATION: Service Date Trash Service not found " + dictionaryKey);
                            }
                        
                        if (address == null)
                            {
                            throw new WRMNullValueException("TRASH ADDRESS POPULATION: Address is null");
                            }
                        else
                            {
                            AddressDictionary.Add(dictionaryKey, address);
                            }
                        }
                    else
                        {
                        throw new WRMNotSupportedException("TRASH ADDRESS POPULATION: In Trash Service Invalid status for KGIS Address " + dictionaryKey);
                        }


                    }
                catch (WRMNullValueException ex)
                    {
                    WRMLogger.LogBuilder.AppendLine("ERROR: At row " + row + " " + ex.Message + " " + dictionaryKey);
                    /*
                    WRMLogger.LogBuilder.AppendLine(ex.StackTrace);

                    Exception inner = ex.InnerException;
                    if (inner != null)
                        {
                        WRMLogger.LogBuilder.AppendLine(inner.Message);
                        WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                        }
                    */
                    }
                catch (WRMNotSupportedException ex)
                    {
                    WRMLogger.LogBuilder.AppendLine("ERROR: At row " + row + " " + ex.Message + " " + dictionaryKey);
                    /*
                    WRMLogger.LogBuilder.AppendLine(ex.StackTrace);

                    Exception inner = ex.InnerException;
                    if (inner != null)
                        {
                        WRMLogger.LogBuilder.AppendLine(inner.Message);
                        WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                        }
                    */
                    }
                }
            }
        private void addRecycleServiceAddressesFromWorksheetIntoDictionary(ExcelWorksheet worksheet)
            {

            int rowCount = worksheet.Dimension.End.Row;     //get row count

            for (int row = 2; row <= rowCount; row++)
                {
                Address address = null;
                string dictionaryKey = null;
                try
                    {
                    address = serviceTrashDayImporter.createAddressFromServiceDayWorksheet(worksheet, row);
                    populateAddressFromKGIS(ref address);
                    dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                    }
                catch (WRMNotSupportedException ex)
                    {
                    WRMLogger.LogBuilder.AppendLine("ERROR: At row " + row + " RECYCLING ADDRESS POPULATION " + ex.Message);
                    continue;
                    }
                catch (WRMNullValueException ex)
                    {
                    WRMLogger.LogBuilder.AppendLine("ERROR: At row " + row + " RECYCLING ADDRESS POPULATION " + ex.Message);
                    continue;
                    }
                try
                    {

                    ExcelRange recycleSchedule = worksheet.Cells[row, 5];
                    ExcelRange recycleFrequency = worksheet.Cells[row, 7];
                    ExcelRange addressType = worksheet.Cells[row, 8];
                    Address foundAddress = new Address();
                    if (AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                        {
                        if (recycleSchedule != null && recycleSchedule.Value != null)
                            {
                            if (validateDayOfWeek(recycleSchedule.Value.ToString()))
                                {
                                AddressDictionary[dictionaryKey].RecyclingPickup = true;
                                AddressDictionary[dictionaryKey].RecycleDayOfWeek = recycleSchedule.Value.ToString().ToUpper();
                                if (recycleFrequency != null && recycleFrequency.Value != null && validateRecycleFrequency(recycleFrequency.Value.ToString()))
                                    {
                                    AddressDictionary[dictionaryKey].RecycleFrequency = recycleFrequency.Value.ToString();
                                    }
                                else
                                    {
                                    throw new WRMNotSupportedException("RECYCLING ADDRESS POPULATION: Invalid Recycling Frequency");
                                    }

                                }
                            else
                                {
                                throw new WRMNotSupportedException("RECYCLING ADDRESS POPULATION: Invalid TrashSchedule day of Week " + recycleSchedule.Value.ToString());

                                }
                            }
                        else
                            {
                            AddressDictionary[dictionaryKey].RecyclingPickup = false;
                            WRMLogger.LogBuilder.AppendLine("ERROR: RECYCLING ADDRESS POPULATION: Entry in Recycling without a Recyling day " + dictionaryKey);
                            }

                        }
                    else
                        {

                        if (address == null)
                            {
                            throw new WRMNullValueException("RECYCLING ADDRESS POPULATION: Address is null");
                            }
                        else
                            {
                            if (recycleSchedule.Value != null)
                                {
                                if (!validateDayOfWeek(recycleSchedule.Value.ToString()))
                                    {
                                    throw new WRMNotSupportedException("RECYCLING ADDRESS POPULATION: Recycle Day of Week is not valid " + recycleSchedule.Value.ToString());
                                    }
                                else
                                    {
                                    address.RecyclingPickup = true;
                                    address.RecycleDayOfWeek = recycleSchedule.Value.ToString();
                                    if (recycleFrequency != null && recycleFrequency.Value != null && validateRecycleFrequency(recycleFrequency.Value.ToString()))
                                        {
                                        address.RecycleFrequency = recycleFrequency.Value.ToString();
                                        }
                                    else
                                        {
                                        throw new WRMNotSupportedException("RECYCLING ADDRESS POPULATION: Invalid RecycleFrequency");
                                        }
                                    }
                                }
                            else
                                {
                                address.RecyclingPickup = false;
                                WRMLogger.LogBuilder.AppendLine("ERROR: At row " + row + " RECYCLING ADDRESS POPULATION: Entry in Recycling without a Recyling day " + dictionaryKey);
                                }
                            AddressDictionary.Add(dictionaryKey, address);
                            }


                        }

                    }
                catch (WRMNullValueException ex)
                    {
                    WRMLogger.LogBuilder.AppendLine("ERROR: At row " + row + " " + ex.Message + " " + dictionaryKey);
                    /*
                    WRMLogger.LogBuilder.AppendLine(ex.StackTrace);

                    Exception inner = ex.InnerException;
                    if (inner != null)
                        {
                        WRMLogger.LogBuilder.AppendLine(inner.Message);
                        WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                        }
                    */
                    }
                catch (WRMNotSupportedException ex)
                    {
                    WRMLogger.LogBuilder.AppendLine("ERROR: At row " + row + " " + ex.Message + " " + dictionaryKey);
                    /*
                    WRMLogger.LogBuilder.AppendLine(ex.StackTrace);

                    Exception inner = ex.InnerException;
                    if (inner != null)
                        {
                        WRMLogger.LogBuilder.AppendLine(inner.Message);
                        WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                        }
                    */
                    }
                catch (Exception ex)
                    {
                    throw ex;
                    }
                }
            }


        public static string translateAddressTypeFromKGISAddressUse(KGISAddress kgisCityResidentAddress)
            {
            string addressType = null;
            switch (kgisCityResidentAddress.ADDRESS_USE_TYPE)
                {
                case "DWELLING, MULTI-FAMILY":
                case "ACCESSORY DWELLING UNIT":
                case "PRIMARY BUILDING ADDRESS":
                case "DWELLING, SINGLE-FAMILY":
                case "DWELLING, TWO-FAMILY":
                case "DWELLING, TOWNHOUSE":
                case "DWELLING, APT UNIT":
                    addressType = "RESIDENTIAL";
                    break;

                case "COMMUNITY CENTER":
                case "PUBLIC PARK":
                case "FIRE STATION":
                case "POLICE STATION":
                case "GREENWAY":
                case "RECREATIONAL FACILITY, PUBLIC":
                    addressType = "SPECIALTY";
                    break;

                case "RECREATIONAL FACILITY, PRIVATE":

                case "PARKING LOT/STRUCTURE":
                case "CEMETERY":
                case "BUSINESS":
                case "RESIDENTIAL CARE FACILITY":
                case "GOVERNMENT OFFICE/FACILITY":
                case "LODGE/MEETING HALL":
                case "PLACE OF WORSHIP":
                case "GROUP HOME":
                    addressType = "COMMERCIAL";
                    break;

                default:
                    throw new WRMNotSupportedException("ADDRESS POPULATION: Invalid Property Type " + kgisCityResidentAddress.ADDRESS_USE_TYPE);
                }
            if (String.IsNullOrEmpty(addressType))
                {
                throw new WRMNotSupportedException("ADDRESS POPULATION: Property Type may not be null ");
                }
            return addressType;
            }

        private Boolean validateDayOfWeek(string dayOfWeek)
            {
            Boolean validated = false;
            switch (dayOfWeek.ToUpper())
                {
                case "MONDAY":
                case "TUESDAY":
                case "WEDNESDAY":
                case "THURSDAY":
                case "FRIDAY":
                case "SATURDAY":
                case "SUNDAY":
                    validated = true;
                    break;

                }
            return validated;

            }
        private Boolean validateRecycleFrequency(string frequency)
            {
            Boolean validated = false;
            switch (frequency)
                {
                case "A":
                case "B":
                    validated = true;
                    break;

                }
            return validated;

            }


        public int addAddressToWRM_TrashRecycle(Address address)
            {
            int addressId = -1;
            populateAddressFromKGIS(ref address);
            
            if (address != null)
                {
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(address);
                string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                AddressPopulation.AddressDictionary[dictionaryKey] = address;
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();

                Address addedAddress = WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Address.Where(a => a.StreetName == address.StreetName && a.StreetNumber == address.StreetNumber && a.UnitNumber == address.UnitNumber && a.ZipCode == address.ZipCode).ToList().First();

                addressId = addedAddress.AddressID;
                AddressPopulation.AddressIdentiferDictionary[dictionaryKey] = addressId;
                }
            else
                {
                throw new WRMNullValueException("ADDRESS POPULATION: populateAddressFromKGIS: Address is Null");
                }
            return addressId;
            }

        /* if you recieve a NotSupportedException trying to find an address type, address is assigned null, an error is printed, and processing goes on to the next record */
        private void populateAddressFromKGIS(ref Address address)
            {
            if (address == null || String.IsNullOrEmpty(address.StreetName))
                {
                throw new WRMNullValueException("ADDRESS POPULATION: populateAddressFromKGIS: Address or Street is Null");
                }
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
            //KgisCityResidentAddressList


            KGISAddress kgisAddress = new KGISAddress();
            if (KGISAddressImporter.getKGISAddressCache().TryGetValue(dictionaryKey, out kgisAddress))
                {
                if (Decimal.ToInt32(kgisAddress.JURISDICTION ?? 0) == 1 && Decimal.ToInt32(kgisAddress.ADDRESS_STATUS ?? 0) == 2)
                    {
                    try
                        {

                        address.GISParcelID = kgisAddress.PARCELID;



                        try
                            {
                            address.AddressType = AddressPopulation.translateAddressTypeFromKGISAddressUse(kgisAddress);
                            }
                        catch (Exception ex)
                            {
                            address.AddressType = "RESIDENTIAL";
                            }


                        address.GISLatitude = kgisAddress.LATITUDE;
                        address.GISLongitude = kgisAddress.LONGITUDE;
                        address.GISPointX = kgisAddress.POINT_X;
                        address.GISPointY = kgisAddress.POINT_Y;
                        }
                    catch (WRMNotSupportedException nex)
                        {
                        WRMLogger.LogBuilder.AppendLine(nex.Message);
                        WRMLogger.LogBuilder.AppendLine(nex.ToString());
                        Exception inner = nex.InnerException;
                        if (inner != null)
                            {
                            WRMLogger.LogBuilder.AppendLine(inner.Message);
                            WRMLogger.LogBuilder.AppendLine(inner.ToString());
                            }
                        throw new WRMNotSupportedException("ADDRESS POPULATION: populateAddressFromKGIS: Address Type not supported " + address.AddressType);
                        }
                    }
                else
                    {
                    if (Decimal.ToInt32(kgisAddress.JURISDICTION ?? 0) != 1)
                        {
                        throw new WRMNotSupportedException("ADDRESS POPULATION: Out of Jurisdiction FOR ADDRESS [" + address.StreetNumber + "] [" + address.StreetName + "] [" + address.UnitNumber + "] [" + address.ZipCode + "]! \n");
                        }
                    else if (Decimal.ToInt32(kgisAddress.ADDRESS_STATUS ?? 0) != 2)
                        {

                        throw new WRMNotSupportedException("ADDRESS POPULATION: Invalid KGIS Address Status [" + address.StreetNumber + "] [" + address.StreetName + "] [" + address.UnitNumber + "] [" + address.ZipCode + "]!");
                        }
                    }
                // request.FirstName; request.LastName, request.PhoneNumber; request.Email;


                }
            else
                {
                address.AddressType = "RESIDENTIAL";
                WRMLogger.LogBuilder.AppendLine("ADDRESS POPULATION: WARNING:  ADDRESS IS NOT FOUND IN KGIS. [" + address.StreetNumber + "][" + address.StreetName + "][" + address.UnitNumber + "][" + address.ZipCode + "] does not exist!");
                }

            }
        }
    }