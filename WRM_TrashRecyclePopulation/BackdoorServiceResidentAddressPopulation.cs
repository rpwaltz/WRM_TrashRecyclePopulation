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

        private Dictionary<string, BackDoorPickup> backDoorPickupDictionary = new Dictionary<string, BackDoorPickup>();

        public Dictionary<string, BackDoorPickup> BackDoorPickupDictionary { get => backDoorPickupDictionary; set => backDoorPickupDictionary = value; }


        public bool populateBackdoorServiceAddressCustomer()
            {
            try
                {
                String logLine = "populateBackdoorServiceAddressCustomer";
                int maxToProcess = 0;

                IEnumerable<BackdoorServiceRequest> orderedSolidWasteBackdoorRequestList = WRM_TrashRecycleQueries.retrieveBackdoorRequestList();

                foreach (BackdoorServiceRequest backdoorRequest in orderedSolidWasteBackdoorRequestList)
                    {
                    if (maxToProcess % 100 == 0)
                        {
                        Program.logLine = "Processed Backdoor Requests: " + maxToProcess;
                        WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        }
                    ++maxToProcess;
                    if (backdoorRequest == null)
                        {
                        throw new Exception("backdoor request is null");
                        }
                    logLine = "foreach Backdoor Request";

                    populateResidentAddressFromRequest(backdoorRequest);

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
                }
            return false;
            }
        override public void buildAndSaveTrashRecycleEntitiesFromRequest(dynamic backdoorRequest, IEnumerator<Kgisaddress> foundKgisResidentAddressEnumerator)
            {

            if (foundKgisResidentAddressEnumerator.Current == null)
                {
                foundKgisResidentAddressEnumerator.MoveNext();

                }

            Kgisaddress kgisCityResidentAddress = foundKgisResidentAddressEnumerator.Current;

            Address address = builBackdoorAddress(backdoorRequest, kgisCityResidentAddress);
            address = saveAddress(address);


            Resident resident = buildRequestResident(backdoorRequest, address.AddressId);

            resident = saveResident(resident);


            BackDoorPickup backDoorPickup = buildRequestBackdoorService(backdoorRequest, address.AddressId, resident.ResidentId);

            saveBackdoorPickup(backDoorPickup);

            }
        override public void buildAndSaveTrashRecycleEntitiesFromRequestWithUnits(dynamic backdoorRequest, IEnumerator<Kgisaddress> foundKgisResidentAddressEnumerator)
            {
            if (foundKgisResidentAddressEnumerator.Current == null)
                {
                foundKgisResidentAddressEnumerator.MoveNext();

                }

            Kgisaddress kgisCityResidentAddress = foundKgisResidentAddressEnumerator.Current;

            Address address = builBackdoorAddress(backdoorRequest, kgisCityResidentAddress);
            address.UnitNumber = backdoorRequest.UnitNumber;
            address = saveAddress(address);

            Resident resident = buildRequestResident(backdoorRequest, address.AddressId);

            resident = saveResident(resident);

            BackDoorPickup backDoorPickup = buildRequestBackdoorService(backdoorRequest, address.AddressId, resident.ResidentId);

            saveBackdoorPickup(backDoorPickup);

            }


        public Address builBackdoorAddress(dynamic backdoorRequest, Kgisaddress kgisCityResidentAddress)
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
            string backdoorDictionaryKey = backdoorPickup.AddressId.ToString();
//            WRMLogger.LogBuilder.AppendLine("backdoorDictionaryKey " + backdoorDictionaryKey);

            if (backDoorPickupDictionary.TryGetValue(backdoorDictionaryKey, out foundBackdoorPickup))
                {
                if ((backdoorPickup.UpdateDate ?? Program.posixEpoche) > (foundBackdoorPickup.UpdateDate ?? Program.posixEpoche))
                    {
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Remove(foundBackdoorPickup);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(backdoorPickup);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    backdoorPickup = foundBackdoorPickup;
                    }
                else
                    {
                    
                    string logMessage = string.Format("Unable to determine if Backdoor Pickup changed from Name {0} {1} Address {2}  {3} With Status {4} created {5} updated {6} TO Name {7} {8} Address {9}  {10} With Status {11} created {12} updated {13}\n",
                        backdoorPickup.Resident.FirstName, backdoorPickup.Resident.LastName, backdoorPickup.Address.StreetNumber, backdoorPickup.Address.StreetName, backdoorPickup.Address.UnitNumber, backdoorPickup.BackdoorStatus, backdoorPickup.CreateDate.ToString(), backdoorPickup.UpdateDate.ToString(),
                        foundBackdoorPickup.Resident.FirstName, foundBackdoorPickup.Resident.LastName, foundBackdoorPickup.Address.StreetNumber, foundBackdoorPickup.Address.StreetName, foundBackdoorPickup.Address.UnitNumber, foundBackdoorPickup.BackdoorStatus, foundBackdoorPickup.CreateDate.ToString(), foundBackdoorPickup.UpdateDate.ToString());
                    WRMLogger.LogBuilder.Append(logMessage);
                    }
                }
            else
                {
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(backdoorPickup);
                // logBuilder.AppendLine("Add " + request.StreetNumber + "  " + request.StreetName);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                backDoorPickupDictionary.Add(backdoorDictionaryKey, backdoorPickup);
                }
            return backdoorPickup;
            }

        }
    }
