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


        public bool populateRecyclingResidentAddress()
            {
            try
                {
                Program.logLine = "Begin Recycling Requests";
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                
                int maxToProcess = 0;
                IEnumerable<Kgisaddress> kgisCityResidentAddressList = WRM_TrashRecycleQueries.retrieveKgisCityResidentAddressList();


                IEnumerable<RecyclingRequest> orderedSolidWasteRecyclingRequestList = WRM_TrashRecycleQueries.retrieveRecyclingRequestList();

                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if (maxToProcess%100 == 0)
                        {
                        Program.logLine = "Processed Recycling Requests: " + maxToProcess;
                        WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        }
                    ++maxToProcess;

                    populateResidentAddressFromRequest(recyclingRequest);

                    }

                Program.logLine = "Finished Recycling Requests";
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                return true;
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendFormat("Exception:{0} : {1} : {2} : {3}{4}", ex.HResult, ex.Message, ex.TargetSite, ex.HelpLink, Environment.NewLine);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine( ex.StackTrace);
                WRMLogger.Logger.log();
                }
            return false;
            }

        override public void buildAndSaveTrashRecycleEntitiesFromRequest(dynamic recyclingRequest, IEnumerator<Kgisaddress> foundKgisResidentAddressEnumerator)
            {

            if (foundKgisResidentAddressEnumerator.Current == null)
                {
                foundKgisResidentAddressEnumerator.MoveNext();

                }

            Kgisaddress kgisCityResidentAddress = foundKgisResidentAddressEnumerator.Current;

            Address address = buildRecyclingAddress(recyclingRequest, kgisCityResidentAddress);
            address = saveAddress(address);

            Resident resident = buildRequestResident(recyclingRequest, address.AddressId);

            saveResident(resident);


            }
        override public void buildAndSaveTrashRecycleEntitiesFromRequestWithUnits(dynamic recyclingRequest, IEnumerator<Kgisaddress> foundKgisResidentAddressEnumerator)
            {
            if (foundKgisResidentAddressEnumerator.Current == null)
                {
                foundKgisResidentAddressEnumerator.MoveNext();

                }

            Kgisaddress kgisCityResidentAddress = foundKgisResidentAddressEnumerator.Current;

            Address address = buildRecyclingAddress(recyclingRequest, kgisCityResidentAddress);
            address.UnitNumber = recyclingRequest.UnitNumber;
            address = saveAddress(address);

            Resident resident = buildRequestResident(recyclingRequest, address.AddressId);


            resident = saveResident(resident);

            }


        private Address buildRecyclingAddress(dynamic recyclingRequest, Kgisaddress kgisCityResidentAddress)
            {
            Address address = buildRequestAddress(recyclingRequest, kgisCityResidentAddress);

            address.RecyclingPickup = recyclingRequest.Status.Equals("APPROVED");
            string status = (string)recyclingRequest.Status.Trim();
            switch (status)
                {
                case "APPROVED":
                    address.RecyclingStatus = "APPROVED";
                    break;
                case "REQUESTED":
                    address.RecyclingStatus = "REQUESTED";
                    break;
                case "WITHDRAWN":
                    address.RecyclingStatus = "WITHDRAWN";
                    break;
                case "DISAPPROVED":
                    address.RecyclingStatus = "REJECTED";
                    break;
                default:
                    throw new Exception(" Recycling status :" + recyclingRequest.Status + ": undefined");
                }


            address.RecyclingStatusDate = recyclingRequest.StatusDate;
            address.RecyclingRequestedDate = recyclingRequest.CreationDate;
            address.NumberUnits = "1";
            return address;
            }

        }


    }
