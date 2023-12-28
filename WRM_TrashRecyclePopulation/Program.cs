using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.IO;
using OfficeOpenXml;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste.Models;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WRM_TrashRecyclePopulation
    {


    class Program
        {


        public static DateTime beforeNow = DateTime.Now;
        public static DateTime justNow = DateTime.Now;
        public static DateTime posixEpoche = new DateTime(1970, 1, 1, 0, 0, 0);
        public static TimeSpan timeDiff;
        public static double loopMillisecondsPast = 0;
        public static string logLine;
        static void Main()
            {
            WRMLogger.Logger = new WRMLogger(ConfigurationManager.AppSettings["LOGFILEPATH"].ToString(), ConfigurationManager.AppSettings["LOGFILENAME"].ToString());
            
            DateTime startTime = DateTime.Now;


            WRMLogger.LogBuilder.AppendLine("Start");
            timeDiff = justNow - beforeNow;

            try
                {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;


                WRMLogger.LogBuilder.AppendLine("Start " + justNow.ToString("o", new CultureInfo("en-us")) + " MilliSeconds passed : " + timeDiff.TotalMilliseconds.ToString());

                // initiate the KGIS Address Cache

                WRMLogger.Logger.logMessageAndDeltaTime("Populated KGISAddressCache ", ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();

                WRM_TrashRecycle wrmTrashRecycleContext = WRM_EntityFrameworkContextCache.WrmTrashRecycleContext;
                using (wrmTrashRecycleContext)
                    {
                    // Populate all addresses in the database from Service Day Spreadsheet
                    AddressPopulation addressPopulation = new AddressPopulation();
                    addressPopulation.populateAddresses();

                    CartPopulation cartPopulation = new CartPopulation();
                    cartPopulation.populateCarts();

                    CommercialAccountPopulation commercialAccountPopulation = new CommercialAccountPopulation();
                    commercialAccountPopulation.populateCommercialAccounts();

                    SolidWaste solidWasteContext = WRM_EntityFrameworkContextCache.SolidWasteContext;

                    using (solidWasteContext)
                        {
                        RecyclingResidentAddressPopulation recyclingAddressCustomerPopulation = new RecyclingResidentAddressPopulation();

                        recyclingAddressCustomerPopulation.populateRecyclingResidentAddress();

                        BackdoorServiceResidentAddressPopulation backdoorServiceResidentAddressPopulation = new BackdoorServiceResidentAddressPopulation();
                        backdoorServiceResidentAddressPopulation.populateBackDoorPickup();

                        }

                    }

                }

            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);

                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }

                }
            justNow = DateTime.Now;
            timeDiff = justNow - startTime;
            WRMLogger.LogBuilder.AppendLine("Population End " + justNow.ToString("o", new CultureInfo("en-us")) + "Total MilliSeconds passed : " + timeDiff.TotalMilliseconds.ToString());
            WRMLogger.Logger.log();


            }

        }
    }
