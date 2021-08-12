using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using OfficeOpenXml;

namespace WRM_TrashRecyclePopulation
    {
    class ColorXcelFinder
        {

        private ExcelWorksheet worksheet;

        public ColorXcelFinder()
            {
            string xlsxSNMasterlistPath = @"C:\Users\rwaltz\Documents\SolidWasteData\SN_MASTERLIST_Current.xlsm";
            FileInfo xlsxSNMasterlistFileInfo = new FileInfo(xlsxSNMasterlistPath);
            if (xlsxSNMasterlistFileInfo.Exists)
                {
                ExcelPackage package = new ExcelPackage(xlsxSNMasterlistFileInfo);
                System.Console.WriteLine(package.Workbook.Worksheets.Count());
                worksheet = package.Workbook.Worksheets[2];
                }
            else throw new Exception("FILE NOT FOUND!");

            }

        public void findColors()
            {
 
                int rowCount = worksheet.Dimension.End.Row;
 

            for (int row = 1; row <= rowCount; row++)
                    {

                    extractColorFromWorksheet(worksheet, row);

                    }
            }
  

        private void extractColorFromWorksheet(ExcelWorksheet worksheet, int row)
            {
            // not a good way to make the following repeated code a method

            int firstColumn = worksheet.Dimension.Start.Column;
            int lastColumn = worksheet.Dimension.End.Column;
            for (int i = firstColumn; i <= lastColumn; ++i) {

                System.Console.WriteLine(row + " " + i);
                ExcelRange colorCell = worksheet.Cells[row, i];
                if (colorCell.Style.Fill.BackgroundColor.LookupColor() != null) 
                    {
                    System.Console.WriteLine(colorCell.Style.Fill.BackgroundColor.LookupColor());
                    }
                if (colorCell.Style.Font.Strike)
                    {
                    System.Console.WriteLine(colorCell.Style.Font.Strike);
                    }
                if (colorCell.Style.Font.Color.LookupColor() != null)
                    {
                    System.Console.WriteLine(colorCell.Style.Font.Color.LookupColor());
                    }
                }
            }
        }
    }
