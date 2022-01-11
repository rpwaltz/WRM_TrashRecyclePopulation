using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OfficeOpenXml;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation
    {
    class ServiceTrashDayImporter
        {
        private FileInfo xlsxServiceDayTrashAndRecyclingFileInfo;

        public Dictionary<string, Address> addressDictionary;


        public static string xlsxServiceDayTrashAndRecyclingPath = @"C:\Users\rwaltz\Documents\SolidWaste\Service_Day_Trash_and_Recycling.xlsx";
        public static ServiceTrashDayImporter serviceTrashDayImporter;

        public static ServiceTrashDayImporter getServiceTrashDayImporter()
            {
            if (serviceTrashDayImporter == null)
                {
                serviceTrashDayImporter = new ServiceTrashDayImporter();
                }
            return serviceTrashDayImporter;
            }
        private ServiceTrashDayImporter()
            {

            xlsxServiceDayTrashAndRecyclingFileInfo = new FileInfo(xlsxServiceDayTrashAndRecyclingPath);
            addressDictionary = new Dictionary<string, Address>();
            ExcelPackage package = new ExcelPackage(xlsxServiceDayTrashAndRecyclingFileInfo);
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            extractTrashAddressesFromWorksheetIntoDictionary(worksheet);
            worksheet = package.Workbook.Worksheets[1];
            extractRecycleAddressesFromWorksheetIntoDictionary(worksheet);

            }

        private void extractTrashAddressesFromWorksheetIntoDictionary(ExcelWorksheet worksheet)
            {

            int rowCount = worksheet.Dimension.End.Row;     //get row count

            for (int row = 2; row <= rowCount; row++)
                {
                Address address = extractAddressFromWorksheet(worksheet, row);
                string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                if (!this.addressDictionary.ContainsKey(dictionaryKey))
                    {
                    address.TrashPickup = true;
                    ExcelRange trashSchedule = worksheet.Cells[row, 5];
                    if ((trashSchedule.Value != null) && validateDayOfWeek(trashSchedule.Value.ToString()))
                        {
                        address.TrashDayOfWeek = trashSchedule.Value.ToString();
                        }
                    else
                        {
                        address.TrashDayOfWeek = null;
                        }
                    addressDictionary.Add(dictionaryKey, address);

                    }

                }
            }
        private void extractRecycleAddressesFromWorksheetIntoDictionary(ExcelWorksheet worksheet)
            {

            int rowCount = worksheet.Dimension.End.Row;     //get row count

            for (int row = 2; row <= rowCount; row++)
                {
                Address address = extractAddressFromWorksheet(worksheet, row);
                string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);

                ExcelRange recycleSchedule = worksheet.Cells[row, 5];
                ExcelRange recycleFrequency = worksheet.Cells[row, 7];
                ExcelRange addressType = worksheet.Cells[row, 8];

                if (addressDictionary.ContainsKey(dictionaryKey))
                    {
                    address = null;
                    if (addressDictionary.TryGetValue(dictionaryKey, out address))
                        {
                        completeAddressScheduling(ref address, recycleSchedule, recycleFrequency, addressType);

                        }
                    else
                        {
                        throw new Exception("Address Dictionary contains key " + dictionaryKey + ", but unable to retreive address");
                        }
                    }
                else
                    {
                    completeAddressScheduling(ref address, recycleSchedule, recycleFrequency, addressType);
                    addressDictionary.Add(dictionaryKey, address);
                    }

                }
            }

        private void completeAddressScheduling(ref Address address, ExcelRange recycleSchedule, ExcelRange recycleFrequency, ExcelRange addressType)
            {
            if ((recycleSchedule.Value != null) && validateDayOfWeek(recycleSchedule.Value.ToString()))
                {
                address.RecycleDayOfWeek = recycleSchedule.Value.ToString();
                }
            else
                {
                address.RecycleDayOfWeek = null;
                }

            if (recycleFrequency.Value != null)
                {
                address.RecycleFrequency = recycleFrequency.Value.ToString();
                }
            }

        private Address extractAddressFromWorksheet(ExcelWorksheet worksheet, int row)
            {
            // not a good way to make the following repeated code a method
            Address address = new Address();
            ExcelRange streetNameCell = worksheet.Cells[row, 1];
            if (streetNameCell.Value != null)
                {
                string streetName = streetNameCell.Value.ToString();
                address.StreetName = IdentifierProvider.normalizeStreetName(streetName);

                }
            ExcelRange streetNumberCell = worksheet.Cells[row, 2];
            if (streetNumberCell.Value != null)
                {
                string streetNumber = streetNumberCell.Value.ToString();
                int streetNumberInt32 = IdentifierProvider.normalizeStreetNumber(streetNumber);

                address.StreetNumber = streetNumberInt32;
                }
            ExcelRange unitCell = worksheet.Cells[row, 3];
            if (unitCell.Value != null)
                {
                string unitNumber = unitCell.Value.ToString();
                address.UnitNumber = IdentifierProvider.normalizeUnitNumber(unitNumber);
                }
            ExcelRange zipCell = worksheet.Cells[row, 4];
            if (zipCell.Value != null)
                {

                String zipString = zipCell.Value.ToString();
                address.ZipCode = IdentifierProvider.normalizeZipCode(zipString);

                }
            return address;
            }
        private Boolean validateDayOfWeek(string dayOfWeek)
            {
            Boolean validated = false;
            switch (dayOfWeek)
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
        }



    }
