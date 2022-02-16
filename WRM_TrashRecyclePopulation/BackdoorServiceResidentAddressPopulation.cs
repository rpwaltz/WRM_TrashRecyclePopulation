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
    //    Dictionary<string, Address> BackdoorResidentAddressDictionary = new Dictionary<string, Address>();
        public Dictionary<string, BackDoorPickup> BackDoorPickupDictionary { get => backDoorPickupDictionary; set => backDoorPickupDictionary = value; }
            public bool populateBackDoorPickup()
            {
            try
                {
                Program.logLine = "Begin BackDoorPickup Requests";
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();

                int numberRequestsSaved = 0;
                IEnumerable<BackdoorServiceRequest> orderedSolidWasteBackdorrRequestList = WRM_EntityFrameworkContextCache.SolidWasteContext.BackdoorServiceRequest.OrderBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.StreetName).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.StreetNumber).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.UnitNumber).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.BackdoorId).ToList();

                foreach (BackdoorServiceRequest backdoorRequest in orderedSolidWasteBackdorrRequestList)
                    {

                    try
                        {
                        buildAndSaveBackDoorPickupEntitiesFromRequest(backdoorRequest);
                        ++numberRequestsSaved;
                        }
                    catch (Exception ex) when (ex is WRMWithdrawnStatusException || ex is WRMNotSupportedException || ex is WRMNullValueException)
                        {
                        WRMLogger.LogBuilder.AppendLine(ex.Message);
                        }

                    }

                Program.logLine = "Finished BackDoorPickup Requests " + numberRequestsSaved;
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();

                return true;
                }
            catch (Exception ex)
                {
                WRMLogger.LogBuilder.AppendLine(ex.Message);
                WRMLogger.LogBuilder.AppendLine(ex.ToString());
                Exception inner = ex.InnerException;
                if (inner != null)
                    {
                    WRMLogger.LogBuilder.AppendLine(inner.Message);
                    WRMLogger.LogBuilder.AppendLine(inner.ToString());
                    }
                }
            return false;
            }

        
        public void buildAndSaveBackDoorPickupEntitiesFromRequest(BackdoorServiceRequest backdoorRequest)
            {
            string streetName = backdoorRequest.StreetName;
            int streetNumber = backdoorRequest.StreetNumber ?? 0;
            string zipCode = backdoorRequest.ZipCode;
            string unitNumber = backdoorRequest.UnitNumber;
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
            int foundAddressId = 0;
            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out foundAddressId))
                {

                throw new WRMNullValueException("Unable to find Address : " + dictionaryKey);
                }
            addOrUpdateResidentFromRequestToWRM_TrashRecycle(backdoorRequest);
            int foundResidentId = 0;
            if (!ResidentAddressPopulation.ResidentIdentiferDictionary.TryGetValue(dictionaryKey, out foundResidentId))
                {
                throw new WRMNullValueException("Unable to find Resident : " + dictionaryKey);
                }
            BackDoorPickup backdoorPickup = buildRequestBackdoorService(backdoorRequest);
            
            backdoorPickup.AddressID = foundAddressId;
            backdoorPickup.ResidentID = foundResidentId;
            string backdoorRequestKey = foundAddressId + ":" + foundResidentId;
            
            BackDoorPickup foundBackDoorPickup = new BackDoorPickup();

            if (BackDoorPickupDictionary.TryGetValue(backdoorRequestKey, out foundBackDoorPickup))
                {
                // if the new backdoor request is newer than the previous one
                if ((backdoorRequest.LastUpdatedDate ?? Program.posixEpoche) > (foundBackDoorPickup.UpdateDate ?? Program.posixEpoche))
                    {

                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Remove(BackDoorPickupDictionary[dictionaryKey]);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    }
                else
                    {

                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(backdoorPickup);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Remove(backdoorPickup);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    }

                }
            else
                {
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(backdoorPickup);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                BackDoorPickupDictionary[dictionaryKey] = backdoorPickup;
                }
            }

            




        public BackDoorPickup buildRequestBackdoorService(BackdoorServiceRequest backdoorServiceRequest)
            {
            BackDoorPickup backdoorPickup = new BackDoorPickup();
            backdoorPickup.BackdoorStatus = translateBackdoorStatus(backdoorServiceRequest.Status);
            backdoorPickup.BackdoorStatusDate = backdoorServiceRequest.StatusDate;

            if (!string.IsNullOrEmpty(backdoorServiceRequest.MedicalNeedForBackdoorService) &&
                (backdoorServiceRequest.MedicalNeedForBackdoorService.ToUpper().Equals("YES")))
                {
                backdoorPickup.BackdoorType = "MEDICAL NEED";

                }
            else if (!string.IsNullOrEmpty(backdoorServiceRequest.Over75NoOneToTransportCans) &&
                (backdoorServiceRequest.Over75NoOneToTransportCans.ToUpper().Equals("YES")))
                {
                backdoorPickup.BackdoorType = "OVER 75";
                }
            else if (!string.IsNullOrEmpty(backdoorServiceRequest.WantsToEnrollInFeeBasedService) &&
                (backdoorServiceRequest.WantsToEnrollInFeeBasedService.ToUpper().Equals("YES")))
                {
                backdoorPickup.BackdoorType = "PAY FOR SERVICE";
                }

            backdoorPickup.CreateDate = backdoorServiceRequest.CreationDate;
            backdoorPickup.CreateUser = backdoorServiceRequest.CreatedBy;
            backdoorPickup.UpdateDate = backdoorServiceRequest.LastUpdatedDate;
            backdoorPickup.UpdateUser = backdoorServiceRequest.LastUpdatedBy;

            backdoorPickup.BackdoorStatusDate = backdoorServiceRequest.StatusDate;
            backdoorPickup.Note = backdoorServiceRequest.Comments;
            return backdoorPickup;

            }
        private string translateBackdoorStatus(string status)
            {
            switch (status)
                {
                case "PAY FOR SERVICE":
                    status = "APPROVED";
                    break;
                case "MEDICAL NEED/OVER 75":
                    status = "APPROVED";
                    break;
                case "REQUESTED":
                    status = "REQUESTED";
                    break;
                case "WITHDRAWN":
                    status = "WITHDRAWN";
                    break;
                default:
                    throw new Exception(" Invalid Recycling status :" + status);
                }
            return status;
            }


        }

    }
