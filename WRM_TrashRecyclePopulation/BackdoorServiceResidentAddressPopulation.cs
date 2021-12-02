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

    class BackdoorServiceResidentAddressPopulation : ResidentAddressPopulation
        {

        private SolidWaste solidWasteContext;
        private WRM_TrashRecycle wrmTrashRecycleContext;
        private Dictionary<string, BackDoorPickup> backDoorPickupDictionary = new Dictionary<string, BackDoorPickup>();

        public Dictionary<string, BackDoorPickup> BackDoorPickupDictionary { get => backDoorPickupDictionary; set => backDoorPickupDictionary = value; }

        public BackdoorServiceResidentAddressPopulation(SolidWaste solidWasteContext, WRM_TrashRecycle wrmTrashRecycleContext) : base(solidWasteContext, wrmTrashRecycleContext)
            {
            this.solidWasteContext = solidWasteContext;
            this.wrmTrashRecycleContext = wrmTrashRecycleContext;

            }

        public bool populateBackdoorServiceAddressCustomer()
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
                logLine = "populateBackdoorServiceAddressCustomer";
//                WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref beforeNow, ref justNow, ref loopMillisecondsPast);

                List<BackdoorServiceRequest> solidWasteBackdoorRequestList = solidWasteContext.BackdoorServiceRequest.ToList();

                List<String> requestStatuses = new List<String>() { "PAY FOR SERVICE", "MEDICAL NEED/OVER 75", "REQUESTED" };

                IEnumerable<BackdoorServiceRequest> orderedSolidWasteBackdoorRequestList = solidWasteBackdoorRequestList.Where(swrrl => requestStatuses.Contains(swrrl.Status)).OrderBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.StreetName).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.StreetNumber).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.UnitNumber);



                foreach (BackdoorServiceRequest backdoorRequest in orderedSolidWasteBackdoorRequestList)
                    { 
                //    if (maxToProcess >= 1000)
                //        {
                //
                //        break;
                //        }
                    ++maxToProcess;
                    if (backdoorRequest == null)
                        {
                        throw new Exception("backdoor request is null");
                        }
                    logLine = "foreach Backdoor Request";
//                    WRMLogger.Logger.logMessageAndDeltaTime(logLine, ref beforeNow, ref justNow, ref loopMillisecondsPast);
                    populateResidentAddressFromRequest(backdoorRequest);

 //                   WRMLogger.LogBuilder.AppendLine("Backdoor Loop Total MilliSeconds passed : " + loopMillisecondsPast.ToString());
                    beforeNow = justNow;
//                    WRMLogger.Logger.log();
                    }

                WRMLogger.Logger.log();
                return true;
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendFormat("Exception:{0} : {1} : {2} : {3}{4}",
                    ex.HResult, ex.Message, ex.TargetSite, ex.HelpLink, Environment.NewLine);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                WRMLogger.LogBuilder.AppendLine( ex.StackTrace);
                WRMLogger.Logger.log();
                throw ex;
                }
            return false;
            }
        override public void buildAndSaveTrashRecycleEntitiesFromRequest(dynamic backdoorRequest, IEnumerator<KgisResidentAddressView> foundKgisResidentAddressEnumerator)
            {

            if (foundKgisResidentAddressEnumerator.Current == null)
                {
                foundKgisResidentAddressEnumerator.MoveNext();

                }

            KgisResidentAddressView kgisCityResidentAddress = foundKgisResidentAddressEnumerator.Current;

            Address address = builBackdoorAddress(backdoorRequest, kgisCityResidentAddress);
            address = saveAddress(address);

            Resident resident = buildRequestResident(backdoorRequest, address.AddressId);

            resident = saveResident(resident);

            BackDoorPickup backDoorPickup = buildRequestBackdoorService(backdoorRequest, address.AddressId, resident.ResidentId);

            saveBackdoorPickup(backDoorPickup);
            }
        override public void buildAndSaveTrashRecycleEntitiesFromRequestWithUnits(dynamic backdoorRequest, IEnumerator<KgisResidentAddressView> foundKgisResidentAddressEnumerator)
            {
            if (foundKgisResidentAddressEnumerator.Current == null)
                {
                foundKgisResidentAddressEnumerator.MoveNext();

                }

            KgisResidentAddressView kgisCityResidentAddress = foundKgisResidentAddressEnumerator.Current;

            Address address = builBackdoorAddress(backdoorRequest, kgisCityResidentAddress);
            address.UnitNumber = backdoorRequest.UnitNumber;
            address = saveAddress(address);

            Resident resident = buildRequestResident(backdoorRequest, address.AddressId);

            resident = saveResident(resident);

            BackDoorPickup backDoorPickup = buildRequestBackdoorService(backdoorRequest, address.AddressId, resident.ResidentId);

            saveBackdoorPickup(backDoorPickup);

            }


        public Address builBackdoorAddress(dynamic backdoorRequest, KgisResidentAddressView kgisCityResidentAddress)
            {
            Address address = buildRequestAddress(backdoorRequest, kgisCityResidentAddress);

            address.RecyclingPickup = backdoorRequest.Status.Equals("APPROVED");
            String recycler = backdoorRequest.Recycler;

            if (String.IsNullOrWhiteSpace(recycler))
                {
                recycler = "NO";
                }
            switch (recycler.ToUpper())
                {
                case "YES":
                    address.RecyclingPickup = true;
                    address.RecyclingStatusDate = backdoorRequest.StatusDate;
                    address.RecyclingRequestedDate = backdoorRequest.CreationDate;
                    break;

                case "NO":
                    address.RecyclingPickup = false;
                    break;

                default:
                    throw new Exception("Recycling status undefined");
                }


            address.NumberUnits = "1";
            return address;
            }



        public BackDoorPickup buildRequestBackdoorService(dynamic backdoorServiceRequest, int addressId, int residentId)
            {
            BackDoorPickup backdoorPickup = new BackDoorPickup();

            backdoorPickup.AddressId = addressId;
            backdoorPickup.ResidentId = residentId;

            switch (backdoorServiceRequest.Status)
                {
                case "PAY FOR SERVICE":
                    backdoorPickup.BackdoorType = "PAY FOR SERVICE";
                    backdoorPickup.BackdoorStatus = "APPROVED";
                    break;
                case "MEDICAL NEED/OVER 75":
                    String medicalNeed = backdoorServiceRequest.MedicalNeedForBackdoorService;

                    if (String.IsNullOrWhiteSpace(medicalNeed))
                        {
                        medicalNeed = "NO";
                        }
                    if (medicalNeed.ToUpper().Equals("YES"))
                        {
                        backdoorPickup.BackdoorType = "MEDICAL NEED";
                        }

                    String over75 = backdoorServiceRequest.MedicalNeedForBackdoorService;

                    if (String.IsNullOrWhiteSpace(over75))
                        {
                        over75 = "NO";
                        }
                    if (over75.ToUpper().Equals("YES"))
                        {
                        backdoorPickup.BackdoorType = "OVER 75";
                        }

                    backdoorPickup.BackdoorStatus = "APPROVED";
                    break;
                case "REQUESTED":
                    backdoorPickup.BackdoorStatus = "REQUESTED";
                    break;

                case "WITHDRAWN":
                    backdoorPickup.BackdoorStatus = "WITHDRAWN";
                    break;
                default:
                    throw new Exception("Backdoor status undefined " + backdoorServiceRequest.Status);
                }
            backdoorPickup.BackdoorStatusDate = backdoorServiceRequest.StatusDate;
            backdoorPickup.Note = backdoorServiceRequest.Comments;
            return backdoorPickup;

            }
        public BackDoorPickup saveBackdoorPickup(BackDoorPickup backdoorPickup)
            {
            BackDoorPickup foundBackdoorPickup;
            string backdoorDictionaryKey = backdoorPickup.AddressId + "-" + backdoorPickup.ResidentId;
//            WRMLogger.LogBuilder.AppendLine("backdoorDictionaryKey " + backdoorDictionaryKey);

            if (backDoorPickupDictionary.TryGetValue(backdoorDictionaryKey, out foundBackdoorPickup))
                {
                backdoorPickup = foundBackdoorPickup;
                }
            else
                {
                WrmTrashRecycleContext.Add(backdoorPickup);
                // logBuilder.AppendLine("Add " + request.StreetNumber + "  " + request.StreetName);
                WrmTrashRecycleContext.SaveChanges();
                WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                backDoorPickupDictionary.Add(backdoorDictionaryKey, backdoorPickup);
                }
            return backdoorPickup;
            }
        }
    }
