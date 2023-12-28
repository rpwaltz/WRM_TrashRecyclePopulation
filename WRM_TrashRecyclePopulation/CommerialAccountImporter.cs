using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OfficeOpenXml;
using System.Configuration;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation
    {
    class CommercialAccountImporter
        {
        private FileInfo xlsxServiceDayTrashAndRecyclingFileInfo;

        private Dictionary<string, Address> addressDictionary;

        private ExcelWorksheet activeCommercialAccountWorksheet = null;
        public ExcelWorksheet ActiveCommercialAccountWorksheet { get => activeCommercialAccountWorksheet; set => activeCommercialAccountWorksheet = value; }
        

        private ExcelWorksheet terminatedCommercialAccountWorksheet = null;
        public ExcelWorksheet TerminatedCommercialAccountWorksheet { get => terminatedCommercialAccountWorksheet; set => terminatedCommercialAccountWorksheet = value; }

        private string xlsxServiceDayTrashAndRecyclingPath = ConfigurationManager.AppSettings["Small_Commercial_Accounts_Spreadsheet"].ToString();


        private ExcelWorksheet downtownPickupCrewCommercialAccountWorksheet = null;
        public ExcelWorksheet DowntownPickupCrewCommercialAccountWorksheet { get => downtownPickupCrewCommercialAccountWorksheet; set => downtownPickupCrewCommercialAccountWorksheet = value; }


        ExcelPackage package;


        public CommercialAccountImporter()
            {

            xlsxServiceDayTrashAndRecyclingFileInfo = new FileInfo(xlsxServiceDayTrashAndRecyclingPath);
            addressDictionary = new Dictionary<string, Address>();
            package = new ExcelPackage(xlsxServiceDayTrashAndRecyclingFileInfo);
            int workcount = package.Workbook.Worksheets.Count;

            activeCommercialAccountWorksheet = package.Workbook.Worksheets[0];

            terminatedCommercialAccountWorksheet = package.Workbook.Worksheets[2];

            downtownPickupCrewCommercialAccountWorksheet = package.Workbook.Worksheets[3];
            }


        public void Save()
            {
 /*           ExcelPackage package = new ExcelPackage();
            var trashSheet = package.Workbook.Worksheets.Add("Trash");
            trashSheet = ActiveCommercialAccountWorksheet;
            var recyclingSheet = package.Workbook.Worksheets.Add("Recycling");
            recyclingSheet = TerminatedCommercialAccountWorksheet; */
            package.SaveAs(@"C:\Users\rwaltz\Documents\SolidWaste\Small_Commercial_Account_WithErrors.xlsx");
            }

        public CommercialAccountRow populateActiveCommercialAccountDictionary(ExcelWorksheet worksheet, int row)
            {

            // not a good way to make the following repeated code a method
            CommercialAccountRow commercialAccountRow = new CommercialAccountRow();
            ExcelRange billingNoteCell = worksheet.Cells[row, 1];
            ExcelRange dateAddedCell = worksheet.Cells[row, 2];
            ExcelRange statusCell = worksheet.Cells[row, 3];
            ExcelRange isRecyclerCell = worksheet.Cells[row, 4];
            ExcelRange serviceDaysCell = worksheet.Cells[row, 5];
            ExcelRange customerNumberCell = worksheet.Cells[row, 6];
            ExcelRange customerNameCell = worksheet.Cells[row, 7];
            ExcelRange serviceAddressCell = worksheet.Cells[row, 8];
            ExcelRange serviceZipCodeCell = worksheet.Cells[row, 9];
            ExcelRange billingStreetNumberCell = worksheet.Cells[row, 10];
            ExcelRange billingCityCell = worksheet.Cells[row, 11];
            ExcelRange billingStateCell = worksheet.Cells[row, 12];
            ExcelRange billingZipCodeCell = worksheet.Cells[row, 13];
            ExcelRange personOfContactCell = worksheet.Cells[row, 14];
            ExcelRange contactPhoneNumberCell = worksheet.Cells[row, 15];
            ExcelRange contactEmailAddressCell = worksheet.Cells[row, 16];
            ExcelRange accountNotesCell = worksheet.Cells[row, 17];

            commercialAccountRow.BillingNote = convertExcelRangeToString(billingNoteCell);
            commercialAccountRow.DateAdded = convertExcelRangeToString(dateAddedCell);
            commercialAccountRow.Status = convertExcelRangeToString(statusCell);
            commercialAccountRow.IsRecycler =  convertExcelRangeToString(isRecyclerCell);
            commercialAccountRow.ServiceDays = convertExcelRangeToString(serviceDaysCell);
            commercialAccountRow.CustomerNumber = convertExcelRangeToString(customerNumberCell);
            commercialAccountRow.CustomerName = convertExcelRangeToString(customerNameCell);
            commercialAccountRow.ServiceAddress = convertExcelRangeToString(serviceAddressCell);
            commercialAccountRow.ServiceZipCode = convertExcelRangeToString(serviceZipCodeCell);
            commercialAccountRow.BillingStreetNumber = convertExcelRangeToString(billingStreetNumberCell);
            commercialAccountRow.BillingCity = convertExcelRangeToString(billingCityCell);
            commercialAccountRow.BillingState = convertExcelRangeToString(billingStateCell);
            commercialAccountRow.BillingZipCode = convertExcelRangeToString(billingZipCodeCell);
            commercialAccountRow.PersonOfContact = convertExcelRangeToString(personOfContactCell);
            commercialAccountRow.ContactPhoneNumber = convertExcelRangeToString(contactPhoneNumberCell);
            commercialAccountRow.ContactEmailAddress = convertExcelRangeToString(contactEmailAddressCell);
            commercialAccountRow.AccountNotes = convertExcelRangeToString(accountNotesCell);
            commercialAccountRow.AccountNotes = convertExcelRangeToString(accountNotesCell);
            commercialAccountRow.BillingRate = "";
            return commercialAccountRow;
            }

        public CommercialAccountRow populateTerminatedCommercialAccountDictionary(ExcelWorksheet worksheet, int row)
            {

            // not a good way to make the following repeated code a method
            CommercialAccountRow commercialAccountRow = new CommercialAccountRow();
            ExcelRange outStandingBalanceOwed = worksheet.Cells[row, 1];
           // ExcelRange billingNoteCell = worksheet.Cells[row, 1];
            ExcelRange dateAddedCell = worksheet.Cells[row, 2];
            ExcelRange dateTerminatedCell = worksheet.Cells[row, 3];
            ExcelRange statusCell = worksheet.Cells[row, 4];
            ExcelRange isRecyclerCell = worksheet.Cells[row, 5];
            ExcelRange serviceDaysCell = worksheet.Cells[row, 6];
            ExcelRange customerNumberCell = worksheet.Cells[row, 7];
            ExcelRange customerNameCell = worksheet.Cells[row, 8];
            ExcelRange serviceAddressCell = worksheet.Cells[row, 9];
            ExcelRange serviceZipCodeCell = worksheet.Cells[row, 10];
            ExcelRange billingStreetNumberCell = worksheet.Cells[row, 11];
            ExcelRange billingCityCell = worksheet.Cells[row, 12];
            ExcelRange billingStateCell = worksheet.Cells[row, 13];
            ExcelRange billingZipCodeCell = worksheet.Cells[row, 14];
            ExcelRange personOfContactCell = worksheet.Cells[row, 15];
            ExcelRange contactPhoneNumberCell = worksheet.Cells[row, 16];
            ExcelRange contactEmailAddressCell = worksheet.Cells[row, 17];
            ExcelRange accountNotesCell = worksheet.Cells[row, 18];

            commercialAccountRow.OutstandingBalanceOwned = convertExcelRangeToString(outStandingBalanceOwed);
            commercialAccountRow.DateAdded = convertExcelRangeToString(dateAddedCell);
            commercialAccountRow.DateTerminated = convertExcelRangeToString(dateTerminatedCell);
            commercialAccountRow.Status = convertExcelRangeToString(statusCell);
            commercialAccountRow.IsRecycler = convertExcelRangeToString(isRecyclerCell);
            commercialAccountRow.ServiceDays = convertExcelRangeToString(serviceDaysCell);
            commercialAccountRow.CustomerNumber = convertExcelRangeToString(customerNumberCell);
            commercialAccountRow.CustomerName = convertExcelRangeToString(customerNameCell);
            commercialAccountRow.ServiceAddress = convertExcelRangeToString(serviceAddressCell);
            commercialAccountRow.ServiceZipCode = convertExcelRangeToString(serviceZipCodeCell);
            commercialAccountRow.BillingStreetNumber = convertExcelRangeToString(billingStreetNumberCell);
            commercialAccountRow.BillingCity = convertExcelRangeToString(billingCityCell);
            commercialAccountRow.BillingState = convertExcelRangeToString(billingStateCell);
            commercialAccountRow.BillingZipCode = convertExcelRangeToString(billingZipCodeCell);
            commercialAccountRow.PersonOfContact = convertExcelRangeToString(personOfContactCell);
            commercialAccountRow.ContactPhoneNumber = convertExcelRangeToString(contactPhoneNumberCell);
            commercialAccountRow.ContactEmailAddress = convertExcelRangeToString(contactEmailAddressCell);
            commercialAccountRow.AccountNotes = convertExcelRangeToString(accountNotesCell);
            commercialAccountRow.BillingRate = "";
            return commercialAccountRow;
            }

        public CommercialAccountRow populateDowntownPickupCrewCommercialAccountDictionary(ExcelWorksheet worksheet, int row)
            {

            // not a good way to make the following repeated code a method
            CommercialAccountRow commercialAccountRow = new CommercialAccountRow();
            ExcelRange billingNoteCell = worksheet.Cells[row, 1];
            ExcelRange dateAddedCell = worksheet.Cells[row, 2];
            ExcelRange statusCell = worksheet.Cells[row, 3];
            ExcelRange isRecyclerCell = worksheet.Cells[row, 4];
            ExcelRange serviceDaysCell = worksheet.Cells[row, 5];
            ExcelRange customerNumberCell = worksheet.Cells[row, 6];
            ExcelRange customerNameCell = worksheet.Cells[row, 7];
            ExcelRange serviceAddressCell = worksheet.Cells[row, 8];
            ExcelRange serviceZipCodeCell = worksheet.Cells[row, 9];
            ExcelRange billingStreetNumberCell = worksheet.Cells[row, 10];
            ExcelRange billingCityCell = worksheet.Cells[row, 11];
            ExcelRange billingStateCell = worksheet.Cells[row, 12];
            ExcelRange billingZipCodeCell = worksheet.Cells[row, 13];
            ExcelRange personOfContactCell = worksheet.Cells[row, 14];
            ExcelRange contactPhoneNumberCell = worksheet.Cells[row, 15];
            ExcelRange contactEmailAddressCell = worksheet.Cells[row, 16];
            ExcelRange billingRateCell = worksheet.Cells[row, 17];   
            ExcelRange numberOfTrashCartsCell = worksheet.Cells[row, 18];
            ExcelRange numberOfRecyclingCartsCell = worksheet.Cells[row, 19];
            ExcelRange accountNotesCell = worksheet.Cells[row, 20];

            commercialAccountRow.BillingNote = convertExcelRangeToString(billingNoteCell);
            commercialAccountRow.DateAdded = convertExcelRangeToString(dateAddedCell);
            commercialAccountRow.Status = convertExcelRangeToString(statusCell);
            commercialAccountRow.IsRecycler = convertExcelRangeToString(isRecyclerCell);
            commercialAccountRow.ServiceDays = convertExcelRangeToString(serviceDaysCell);
            commercialAccountRow.CustomerNumber = convertExcelRangeToString(customerNumberCell);
            commercialAccountRow.CustomerName = convertExcelRangeToString(customerNameCell);
            commercialAccountRow.ServiceAddress = convertExcelRangeToString(serviceAddressCell);
            commercialAccountRow.ServiceZipCode = convertExcelRangeToString(serviceZipCodeCell);
            commercialAccountRow.BillingStreetNumber = convertExcelRangeToString(billingStreetNumberCell);
            commercialAccountRow.BillingCity = convertExcelRangeToString(billingCityCell);
            commercialAccountRow.BillingState = convertExcelRangeToString(billingStateCell);
            commercialAccountRow.BillingZipCode = convertExcelRangeToString(billingZipCodeCell);
            commercialAccountRow.PersonOfContact = convertExcelRangeToString(personOfContactCell);
            commercialAccountRow.ContactPhoneNumber = convertExcelRangeToString(contactPhoneNumberCell);
            commercialAccountRow.ContactEmailAddress = convertExcelRangeToString(contactEmailAddressCell);
            commercialAccountRow.BillingRate = convertExcelRangeToString(billingRateCell);
            commercialAccountRow.NumberOfTrashCarts = convertExcelRangeToString(numberOfTrashCartsCell);
            commercialAccountRow.NumberOfRecyclingCarts = convertExcelRangeToString(numberOfRecyclingCartsCell);
            commercialAccountRow.AccountNotes = convertExcelRangeToString(accountNotesCell);

            return commercialAccountRow;
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
