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
using OfficeOpenXml.Drawing.Theme;

namespace WRM_TrashRecyclePopulation
    {
    class AddressPopulation
        {

        private static ServiceTrashDayImporter serviceTrashDayImporter = null;
        private static Dictionary<string, Address> addressDictionary = new Dictionary<string, Address>();

        public static Dictionary<string, Address> AddressDictionary { get => addressDictionary; set => addressDictionary = value; }

        private static Dictionary<string, int> addressRowDictionary = new Dictionary<string, int>();
        public static Dictionary<string, int> AddressRowDictionary { get => addressRowDictionary; set => addressRowDictionary = value; }

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
            Program.logLine = "START ADDRESS POPULATION:  ";
            WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
            WRMLogger.Logger.log();

            addTrashServiceAddressesFromWorksheetIntoDictionary(serviceTrashDayImporter.TrashServiceDayWorksheet);
            addRecycleServiceAddressesFromWorksheetIntoDictionary(serviceTrashDayImporter.RecycleServiceDayWorksheet);

            serviceTrashDayImporter.Save();
            Program.logLine = "END ADDRESS POPULATION: with " + addressDictionary.Count() + " records ";
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

            for (int row = 2; row <= rowCount; row++)
                {
                Address address = null;
                String dictionaryKey = null;
                try
                    {
                    address = serviceTrashDayImporter.createAddressFromServiceDayWorksheet(worksheet, row);
                    if (populateAddressFromKGIS(ref address))
                        {
                        try
                            {
                            address.AddressType = AddressPopulation.translateAddressTypeFromKGISAddressUse(address.GISAddressUseType);
                            }
                        catch (WRMNotSupportedException ex)
                            {
                            serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 9].Value = ex.Message;
                            }
                        }
                    /* We don't need to log this anymore since there are hundreds like this, and it's not an error
                     * else
                        {
                        WRMLogger.LogBuilder.AppendLine("TRASH ADDRESS POPULATION ERROR: NOT FOUND IN KGIS: At row " + row  + address.ToString());
                        }
                    */
                    dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                    // create a warning if the KGIS address type is not translatable

                    }
                catch (WRMNotSupportedException ex)
                    {
                    serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 9].Value = ex.Message;
                    continue;
                    }
                catch (WRMIgnoreRowException ex)
                    {
                    continue;
                    }

                catch (WRMNullValueException ex)
                    {
                    // serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 8].Value = "False";
                    WRMLogger.LogBuilder.AppendLine("TRASH ADDRESS Null Value: At row " + row + " " + ex.Message);
                    serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 9].Value = ex.Message + " :TRASH ADDRESS: at row " + row;
                    continue;
                    }
                if (! String.IsNullOrEmpty(dictionaryKey))
                    {
                    // WRMLogger.LogBuilder.AppendLine(dictionaryKey);
                    try
                        {

                        Address foundAddress = new Address();
                        if (!AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                            {
                            ExcelRange trashSchedule = worksheet.Cells[row, 5];
                            if (trashSchedule != null && trashSchedule.Value != null)
                                {
                                string trashDayOfWeek = trashSchedule.Value.ToString().ToUpper().Trim();
                                if (validateDayOfWeek(trashDayOfWeek))
                                    {
                                    address.TrashStatus = "ELIGIBLE";
                                    address.TrashDayOfWeek = trashDayOfWeek;
                                    }
                                else if (trashDayOfWeek.Equals("INELIGIBLE"))
                                    {
                                    address.TrashStatus = "INELIGIBLE";
                                    }
                                else
                                    {
                                    address.TrashStatus = "INELIGIBLE";
                                    throw new WRMNotSupportedException("Invalid TrashSchedule day of Week '" + trashDayOfWeek + "'");
                                    }
                                }
                            else
                                {
                                address.TrashStatus = "INELIGIBLE";
                                serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 9].Value = "Service Date Trash Service not found";
                                // WRMLogger.LogBuilder.AppendLine("WARNING: At row " + row + " Service Date Trash Service not found " + dictionaryKey);
                                }

                            if (address == null)
                                {

                                throw new WRMNullValueException("Trash Address is null");
                                }
                            else
                                {
                                
                                AddressDictionary.Add(dictionaryKey, address);
                                AddressRowDictionary.Add(dictionaryKey, row);
                                }
                            }
                        else
                            {
                            throw new WRMNotSupportedException("Duplicate Trash Cart Address from Row " + AddressRowDictionary[dictionaryKey]);
                            }
                        }
                    catch (WRMNullValueException ex)
                        {
                        serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 9].Value = ex.Message ;
                        WRMLogger.LogBuilder.AppendLine("Address Population ERROR A1: At row " + row + " " + ex.Message + " " + dictionaryKey);
                        }
                    catch (WRMNotSupportedException ex)
                        {
                        serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 9].Value = ex.Message + " :TRASH ADDRESS: at row " + row; ;
                        WRMLogger.LogBuilder.AppendLine("Address Population ERROR A2: At row " + row + " " + ex.Message + " " + dictionaryKey);

                        }
                    catch (Exception ex)
                        {
                        serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 9].Value = ex.Message;
                        throw ex;
                        }
                    }
                else
                    {
                    WRMLogger.LogBuilder.AppendLine("Address Population ERROR A3: ADDRESS NULL DICTIONARY KEY: At row " + row );
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
                    if (populateAddressFromKGIS(ref address))
                        {
                        try
                            {
                            AddressPopulation.translateAddressTypeFromKGISAddressUse(address.GISAddressUseType);
                            }
                        catch (WRMNotSupportedException ex)
                            {
                            serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 10].Value = ex.Message;
                            }
                        }

                    dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                    }
                catch (WRMNotSupportedException ex)
                    {
                    serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = ex.Message ; 
                    continue;
                    }
                catch (WRMNullValueException ex)
                    {
                    //serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 9].Value = "False";
                    serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = ex.Message + " :RECYCLE ADDRESS: null value at row " + row;
                    continue;
                    }
                catch (WRMIgnoreRowException ex)
                    {
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
                            String recycleDayOfWeekString = recycleSchedule.Value.ToString().ToUpper().Trim();
                            if (validateDayOfWeek(recycleDayOfWeekString))
                                {
                                AddressDictionary[dictionaryKey].RecycleDayOfWeek = recycleDayOfWeekString;
                                if (recycleFrequency != null && recycleFrequency.Value != null)
                                    {
                                    String recycleFrequencyString = recycleFrequency.Value.ToString().ToUpper().Trim();
                                    if (validateRecycleFrequency(recycleFrequencyString))
                                        {
                                        AddressDictionary[dictionaryKey].RecycleFrequency = recycleFrequencyString;
                                        }
                                    else
                                        {
                                        serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = "Invalid Recycling Frequency '" + recycleFrequencyString + "'";
                                        throw new WRMNotSupportedException("Invalid Recycling Frequency'" + recycleFrequencyString + "'");
                                        }
                                    }
                                }
                            else if (recycleDayOfWeekString.Equals("INELIGIBLE"))
                                {
                                AddressDictionary[dictionaryKey].RecycleDayOfWeek = "INELIGIBLE";
                                AddressDictionary[dictionaryKey].TrashStatus = "INELIGIBLE";
                                }
                            else
                                {
                                serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = "Invalid Recycling Day of Week '" + recycleDayOfWeekString + "'";
                                throw new WRMNotSupportedException("Invalid Recycling Schedule Day of Week '" + recycleDayOfWeekString + "'");

                                }
                            }
                        else
                            {
                            serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = "Entry in Recycling without a Recyling day ";
                            }

                        }
                    else
                        {

                        if (address == null)
                            {
                            serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = "Address is null";
                            throw new WRMNullValueException("Recycling Address is null");
                            }
                        else
                            {
                            if (recycleSchedule != null && recycleSchedule.Value != null)
                                {
                                String recycleDayOfWeekString = recycleSchedule.Value.ToString().ToUpper().Trim();
                                if (validateDayOfWeek(recycleDayOfWeekString))
                                    {
                                    address.RecycleDayOfWeek = recycleDayOfWeekString;
                                    if (recycleFrequency != null && recycleFrequency.Value != null)
                                        {
                                        String recycleFrequencyString = recycleFrequency.Value.ToString().ToUpper().Trim();
                                        if (validateRecycleFrequency(recycleFrequencyString))
                                            {
                                            address.RecycleFrequency = recycleFrequencyString;
                                            }
                                        else
                                            {
                                            serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = "Invalid RecycleFrequency:'" + recycleFrequencyString + "'";
                                            throw new WRMNotSupportedException("Invalid RecycleFrequency:'" + recycleFrequencyString + "'");
                                            }
                                        }
                                    else
                                        {
                                        serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = "Invalid NULL RecycleFrequency";
                                        throw new WRMNotSupportedException("Invalid NULL RecycleFrequency");
                                        }
                                    }
                                else if (recycleDayOfWeekString.Equals("INELIGIBLE"))
                                    {
                                    address.RecycleDayOfWeek = "INELIGIBLE";
                                    address.TrashStatus = "INELIGIBLE";
                                    }
                                else
                                    {
                                    serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = "Recycle Day of Week is not valid '" + recycleDayOfWeekString + "'";
                                    throw new WRMNotSupportedException("Recycle Day of Week is not valid '" + recycleDayOfWeekString + "'");
                                    }
                                }
                            else
                                {
                                serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = "Entry in Recycling without a Recyling day";

                                }
                            AddressDictionary.Add(dictionaryKey, address);
                            AddressRowDictionary.Add(dictionaryKey, row);
                            }

                        }

                    }
                catch (WRMNullValueException ex)
                    {
                    serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = ex.Message + " :RECYCLE ADDRESS: at row " + row;
                    WRMLogger.LogBuilder.AppendLine("Recycling Service ERROR A3: At row " + row + " " + ex.Message + " " + dictionaryKey);

                    }
                catch (WRMNotSupportedException ex)
                    {
                    serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = ex.Message + " :RECYCLE ADDRESS: at row " + row;
                    WRMLogger.LogBuilder.AppendLine("Recycling Service ERROR A4: At row " + row + " " + ex.Message + " " + dictionaryKey);

                    }
                catch (Exception ex)
                    {
                    serviceTrashDayImporter.RecycleServiceDayWorksheet.Cells[row, 10].Value = ex.Message + " :RECYCLE ADDRESS: at row " + row;
                    throw ex;
                    }
                }
            }


        public static string translateAddressTypeFromKGISAddressUse(string ADDRESS_USE_TYPE)
            {
            string addressType = null;
            switch (ADDRESS_USE_TYPE)
                {
                case "DWELLING, MULTI-FAMILY":
                case "ACCESSORY DWELLING UNIT":
                case "PRIMARY BUILDING ADDRESS":
                case "DWELLING, SINGLE-FAMILY":
                case "DWELLING, TWO-FAMILY":
                case "DWELLING, TOWNHOUSE":
                case "DWELLING, APT UNIT":
                case "MOBILE HOME":
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

                case "HEALTHCARE FACILITY":
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
                    throw new WRMNotSupportedException("KGIS Invalid Property Type " + ADDRESS_USE_TYPE);
                }
            if (String.IsNullOrEmpty(addressType))
                {
                throw new WRMNotSupportedException("KGIS Property Type may not be null ");
                }
            return addressType;
            }

        public static Boolean validateDayOfWeek(string dayOfWeek)
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
        public static Boolean validateRecycleFrequency(string frequency)
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
        public Address buildResidentAddressFromRequest(dynamic request)
            {
            Address address = new Address();

            string streetName = request.StreetName;
            int streetNumber = request.StreetNumber ?? 0;
            string zipCode = request.ZipCode;
            string unitNumber = null;
            if (request.UnitNumber != null)
                unitNumber = request.UnitNumber;
            Address foundAddress = new Address();
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);


            address.StreetName = streetName;
            address.StreetNumber = streetNumber;
            address.UnitNumber = unitNumber;
            address.ZipCode = zipCode;
            address.CreateDate = request.CreationDate;
            address.CreateUser = request.CreatedBy;
            address.UpdateDate = request.LastUpdatedDate;
            address.UpdateUser = request.LastUpdatedBy;

            return address;
            }



        /* if you recieve a NotSupportedException trying to find an address type, address is assigned null, an error is printed, and processing goes on to the next record */
        public static bool populateAddressFromKGIS(ref Address address)
            {
            bool isValid = false;
            if (address == null || String.IsNullOrEmpty(address.StreetName))
                {
                throw new WRMNullValueException("PopulateAddressFromKGIS: Address or Street is Null");
                }
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);


            KGISAddress kgisAddress = new KGISAddress();
            if (KGISAddressImporter.getKGISAddressCache().TryGetValue(dictionaryKey, out kgisAddress))
                {
                if (Decimal.ToInt32(kgisAddress.JURISDICTION ?? 0) == 1 && Decimal.ToInt32(kgisAddress.ADDRESS_STATUS ?? 0) == 2)
                    {

                    address.GISAddressUseType = kgisAddress.ADDRESS_USE_TYPE;
                    address.GISParcelID = kgisAddress.PARCELID;
                    try
                        {
                        address.AddressType = AddressPopulation.translateAddressTypeFromKGISAddressUse(kgisAddress.ADDRESS_USE_TYPE);
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
                else
                    {
                    if (Decimal.ToInt32(kgisAddress.JURISDICTION ?? 0) != 1)
                        {
                        throw new WRMNotSupportedException("KGIS Out of Jurisdiction " + kgisAddress.JURISDICTION + " FOR ADDRESS [" + address.StreetNumber + " " + address.StreetName + " " + address.UnitNumber + " " + address.ZipCode + "]! \n");
                        }
                    else if (Decimal.ToInt32(kgisAddress.ADDRESS_STATUS ?? 0) != 2)
                        {

                        throw new WRMNotSupportedException("KGIS Invalid Address Status " + kgisAddress.ADDRESS_STATUS + "[" + address.StreetNumber + " " + address.StreetName + " " + address.UnitNumber + " " + address.ZipCode + "]!");
                        }
                    }
                // request.FirstName; request.LastName, request.PhoneNumber; request.Email;
                isValid = true;

                }
            else
                {
                address.AddressType = "RESIDENTIAL";
                }
            if (AddressPopulation.AddressDictionary.ContainsKey(dictionaryKey))
                {
                address.TrashDayOfWeek = AddressPopulation.AddressDictionary[dictionaryKey].TrashDayOfWeek;
                address.TrashStatus = AddressPopulation.AddressDictionary[dictionaryKey].TrashStatus;   
                address.RecycleFrequency = AddressPopulation.AddressDictionary[dictionaryKey].RecycleFrequency;
                address.RecycleDayOfWeek = AddressPopulation.AddressDictionary[dictionaryKey].RecycleDayOfWeek;

                }
            return isValid;
            }
         }
    }