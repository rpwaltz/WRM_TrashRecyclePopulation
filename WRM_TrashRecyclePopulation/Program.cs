﻿using System;
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

namespace WRM_TrashRecyclePopulation
    {


    class Program
        {

        
        static void Main()
            {
            WRMLogger.Logger = new WRMLogger(@"C:\Users\rwaltz\Documents\SolidWaste", "wrm_TrashRecycle.log");




            WRMLogger.LogBuilder.AppendLine("Start");


            /*
WRM_EntityFramework.WRM_TrashRecycle.WRM_TrashRecycle wrmTrashRecycleContext = new WRM_EntityFramework.WRM_TrashRecycle.WRM_TrashRecycle();
CartPopulation cartPopulation = new CartPopulation(wrmTrashRecycleContext);
cartPopulation.populateCarts();

ColorXcelFinder colorxcel = new ColorXcelFinder();
colorxcel.findColors();
*/
            DateTime begin = DateTime.Now;
            DateTime beforeNow = DateTime.Now;
            DateTime justNow = DateTime.Now;
            TimeSpan timeDiff = justNow - beforeNow;

            try
                    {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;



                    WRMLogger.LogBuilder.AppendLine("Start " + justNow.ToString("o", new CultureInfo("en-us")) + " MilliSeconds passed : " + timeDiff.TotalMilliseconds.ToString());

                    WRM_EntityFramework.WRM_TrashRecycle.WRM_TrashRecycle wrmTrashRecycleContext = new WRM_EntityFramework.WRM_TrashRecycle.WRM_TrashRecycle();

                    // DateTime then = DateTime.Now;
                    // need the solid wast context to query all the addresses in the recycing request table.
                    // for every valid recycling request table entry, fill in an address and save it to the database
                    SolidWaste solidWasteContext = new SolidWaste();
                    using (solidWasteContext)
                        {

                        using (wrmTrashRecycleContext)
                            {

                            RecyclingResidentAddressPopulation recyclingAddressCustomerPopulation = new RecyclingResidentAddressPopulation(solidWasteContext, wrmTrashRecycleContext);

                            if (recyclingAddressCustomerPopulation.populateRecyclingResidentAddress() ) 
                                {
                                BackdoorServiceResidentAddressPopulation backdoorServiceResidentAddressPopulation = new BackdoorServiceResidentAddressPopulation(solidWasteContext, wrmTrashRecycleContext);
                                if (backdoorServiceResidentAddressPopulation.populateBackdoorServiceAddressCustomer())
                                    {
                                    CartPopulation cartPopulation = new CartPopulation(wrmTrashRecycleContext);
                                    cartPopulation.populateCarts();
                                    }
                                }

                            wrmTrashRecycleContext.SaveChanges();
                            wrmTrashRecycleContext.ChangeTracker.DetectChanges();

                            }

                        }
                    
                }

            catch (Exception ex)
                {

                WRMLogger.LogBuilder.AppendFormat("Exception:{0} : {1} : {2} : {3}{4}", ex.HResult, ex.Message, ex.TargetSite, ex.HelpLink, Environment.NewLine);
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                
                Exception inner = ex.InnerException;
                if (inner != null) { 
                WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                    }

                }
            justNow = DateTime.Now;
            timeDiff = justNow - beforeNow;
            WRMLogger.LogBuilder.AppendLine("End " + justNow.ToString("o", new CultureInfo("en-us")) + "Total MilliSeconds passed : " + timeDiff.TotalMilliseconds.ToString());
            WRMLogger.Logger.log();


            }

        }
    }
