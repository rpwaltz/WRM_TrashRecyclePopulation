using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste.Models;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation
    {
    class RecyclingResidentAddressPopulation : ResidentAddressPopulation
        {
        private SolidWaste solidWasteContext;
        private WRM_TrashRecycle wrmTrashRecycleContext;


        public RecyclingResidentAddressPopulation(SolidWaste solidWasteContext, WRM_TrashRecycle wrmTrashRecycleContext) : base(solidWasteContext, wrmTrashRecycleContext)
            {
            this.solidWasteContext = solidWasteContext;
            this.wrmTrashRecycleContext = wrmTrashRecycleContext;

            }

        public bool populateRecyclingResidentAddress()
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
                List<RecyclingRequest> solidWasteRecyclingRequestList = solidWasteContext.RecyclingRequest.ToList();


                logLine = "solidWasteContext.RecyclingRequest.ToList";
//                WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref beforeNow, ref justNow, ref loopMillisecondsPast);


                IEnumerable<KgisResidentAddressView> kgisCityResidentAddressList = Wrm_TrashRecycleQueries.retrieveKgisCityResidentAddress();
                logLine = "wrm_TrashRecycleQueries.retrieveKgisCityResidentAddress()";
//                WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref beforeNow, ref justNow, ref loopMillisecondsPast);

                List<String> requestStatuses = new List<String>() { "APPROVED", "REQUESTED" };

                IEnumerable<RecyclingRequest> orderedSolidWasteRecyclingRequestList = solidWasteRecyclingRequestList.Where(swrrl => requestStatuses.Contains(swrrl.Status)).OrderBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.StreetName).ThenBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.StreetNumber).ThenBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.UnitNumber);
 //               WRMLogger.LogBuilder.AppendLine(kgisCityResidentAddressList.ToList().Count() + " requests " + orderedSolidWasteRecyclingRequestList.Count());
                timeDiff = justNow - begin;
                logLine = "solidWasteRecyclingRequestList.Where().solidWasteRecyclingRequestList.OrderBy";
//                WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref beforeNow, ref justNow, ref loopMillisecondsPast);

                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if (maxToProcess >= 1000)
                        {

                        break;
                        }
                    ++maxToProcess;
                    logLine = "foreach recycling Request";
//                    WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref beforeNow, ref justNow, ref loopMillisecondsPast);

                    populateResidentAddressFromRequest(recyclingRequest);
//                    WRMLogger.LogBuilder.AppendLine("Loop Total MilliSeconds passed : " + loopMillisecondsPast.ToString());
                    beforeNow = justNow;
//                    WRMLogger.Logger.log();

                    }
                /*
                WRMLogger.LogBuilder.AppendLine("Before Loop Total MilliSeconds passed : " + timeDiff.TotalMilliseconds);
                beforeNow = justNow;


                justNow = DateTime.Now;
                timeDiff = justNow - beforeNow;
                WRMLogger.LogBuilder.AppendLine("End " + justNow.ToString("o", new CultureInfo("en-us")) + "Total MilliSeconds passed : " + timeDiff.TotalMilliseconds.ToString());
                beforeNow = justNow;
                */

                wrmTrashRecycleContext.SaveChanges();
                wrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRMLogger.Logger.log();
                return true;
                }
            catch (Exception e)
                {
                WRMLogger.LogBuilder.AppendLine( e.ToString());
                WRMLogger.Logger.log();
                }
            return false;
            }

        //override public Address buildRequestResidentAddress(RecyclingRequest recyclingRequest, IEnumerator<KgisResidentAddressView> foundKgisResidentAddressEnumerator) { }
        override public void buildAndSaveTrashRecycleEntitiesFromRequest(dynamic recyclingRequest, IEnumerator<KgisResidentAddressView> foundKgisResidentAddressEnumerator)
            {

            if (foundKgisResidentAddressEnumerator.Current == null)
                {
                foundKgisResidentAddressEnumerator.MoveNext();

                }

            KgisResidentAddressView kgisCityResidentAddress = foundKgisResidentAddressEnumerator.Current;

            Address address = buildRecyclingAddress(recyclingRequest, kgisCityResidentAddress);
            address = saveAddress(address);

            Resident resident = buildRequestResident(recyclingRequest, address.AddressId);

            resident = saveResident(resident);

            }
        override public void buildAndSaveTrashRecycleEntitiesFromRequestWithUnits(dynamic recyclingRequest, IEnumerator<KgisResidentAddressView> foundKgisResidentAddressEnumerator)
            {
            if (foundKgisResidentAddressEnumerator.Current == null)
                {
                foundKgisResidentAddressEnumerator.MoveNext();

                }

            KgisResidentAddressView kgisCityResidentAddress = foundKgisResidentAddressEnumerator.Current;

            Address address = buildRecyclingAddress(recyclingRequest, kgisCityResidentAddress);
            address.UnitNumber = recyclingRequest.UnitNumber;
            address = saveAddress(address);

            Resident resident = buildRequestResident(recyclingRequest, address.AddressId);


            resident = saveResident(resident);

            }


        private Address buildRecyclingAddress(dynamic recyclingRequest, KgisResidentAddressView kgisCityResidentAddress)
            {
            Address address = buildRequestAddress(recyclingRequest, kgisCityResidentAddress);

            address.RecyclingPickup = recyclingRequest.Status.Equals("APPROVED");
            switch (recyclingRequest.Status)
                {
                case "APPROVED":
                    address.RecyclingStatus = "APPROVED";
                    break;

                case "REQUESTED":
                    address.RecyclingStatus = "REQUESTED";
                    break;

                default:
                    throw new Exception("Recycling status undefined");
                }


            address.RecyclingStatusDate = recyclingRequest.StatusDate;
            address.RecyclingRequestedDate = recyclingRequest.CreationDate;
            address.NumberUnits = "1";
            return address;
            }

        }


    }
