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

        private Dictionary<string, Address> addressDictionary;

        private ExcelWorksheet trashServiceDayWorksheet = null;
        public ExcelWorksheet TrashServiceDayWorksheet { get => trashServiceDayWorksheet; set => trashServiceDayWorksheet = value; }
        

        private ExcelWorksheet recycleServiceDayWorksheet = null;
        public ExcelWorksheet RecycleServiceDayWorksheet { get => recycleServiceDayWorksheet; set => recycleServiceDayWorksheet = value; }

        private string xlsxServiceDayTrashAndRecyclingPath = @"C:\Users\rwaltz\Documents\SolidWaste\Service_Day_Trash_and_Recycling.xlsx";
       

       
        public ServiceTrashDayImporter()
            {

            xlsxServiceDayTrashAndRecyclingFileInfo = new FileInfo(xlsxServiceDayTrashAndRecyclingPath);
            addressDictionary = new Dictionary<string, Address>();
            ExcelPackage package = new ExcelPackage(xlsxServiceDayTrashAndRecyclingFileInfo);
            trashServiceDayWorksheet = package.Workbook.Worksheets[0];
            recycleServiceDayWorksheet = package.Workbook.Worksheets[1];
            }



        public Address createAddressFromServiceDayWorksheet(ExcelWorksheet worksheet, int row)
            {
            // not a good way to make the following repeated code a method
            Address address = new Address();
            ExcelRange streetNameCell = worksheet.Cells[row, 1];
            if (streetNameCell != null && streetNameCell.Value != null && !string.IsNullOrEmpty(streetNameCell.Value.ToString()))
                {
                string streetName = streetNameCell.Value.ToString();
                address.StreetName = IdentifierProvider.normalizeStreetName(streetName);

                }
            else
                {
                throw new WRMNullValueException("createAddressFromServiceDayWorksheet Street Name is null at row " + row);
                }

            ExcelRange streetNumberCell = worksheet.Cells[row, 2];
            if (streetNumberCell != null && streetNumberCell.Value != null && !string.IsNullOrEmpty(streetNumberCell.Value.ToString()))
                {
                string streetNumber = streetNumberCell.Value.ToString();
                int streetNumberInt32 = IdentifierProvider.normalizeStreetNumber(streetNumber);

                address.StreetNumber = streetNumberInt32;
                }
            else
                {
                throw new WRMNullValueException("createAddressFromServiceDayWorksheet Street Number is null at row " + row);
                }
            ExcelRange unitCell = worksheet.Cells[row, 3];
            if (unitCell != null && unitCell.Value != null && !string.IsNullOrEmpty(unitCell.Value.ToString()))
                {
                string unit = unitCell.Value.ToString();

                address.UnitNumber = unit;
                }

            ExcelRange zipCell = worksheet.Cells[row, 4];
            if (zipCell != null && zipCell.Value != null && !string.IsNullOrEmpty(zipCell.Value.ToString()))
                {

                String zipString = zipCell.Value.ToString();
                String zipCode = IdentifierProvider.normalizeZipCode(zipString);
                if (!string.IsNullOrEmpty(zipCode))
                    {
                    address.ZipCode = zipCode;
                    }
                else
                    {
                    throw new WRMNullValueException("createAddressFromServiceDayWorksheet Zip Code is null at row " + row);

                    }
                }
            else
                {
                throw new WRMNullValueException("createAddressFromServiceDayWorksheet Zip Code is null at row " + row);
                }
            return address;
            }

        }



    }
