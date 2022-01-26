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
                    if (numberRequestsSaved % 100 == 0)
                        {
                        Program.logLine = "Processed BackDoorPickup Requests: " + numberRequestsSaved;
                        WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        }

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
            string status = backdoorRequest.Status.Trim();
            status = translateBackdoorStatus(status);
            Address address = buildAndAddResidentAddressFromRequest(backdoorRequest);
            Resident resident = addOrUpdateResidentFromRequestToWRM_TrashRecycle(backdoorRequest);
            
            string backdoorRequestKey = address.AddressID + ":" + resident.ResidentID;
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
            BackDoorPickup foundBackDoorPickup = new BackDoorPickup();
            if (BackDoorPickupDictionary.TryGetValue(backdoorRequestKey, out foundBackDoorPickup))
                {
                if ((backdoorRequest.LastUpdatedDate ?? Program.posixEpoche) > (foundBackDoorPickup.UpdateDate ?? Program.posixEpoche))
                    {
                    //backdoor request is more recent than address table, update common address fields
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    buildRequestBackdoorService(backdoorRequest, ref foundBackDoorPickup);
                    backDoorPickupDictionary[backdoorRequestKey] = foundBackDoorPickup;
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(backDoorPickupDictionary[backdoorRequestKey]);
                    AddressPopulation.AddressDictionary[dictionaryKey].CreateDate = backdoorRequest.CreationDate;
                    AddressPopulation.AddressDictionary[dictionaryKey].CreateUser = backdoorRequest.CreatedBy;
                    AddressPopulation.AddressDictionary[dictionaryKey].UpdateDate = backdoorRequest.LastUpdatedDate;
                    AddressPopulation.AddressDictionary[dictionaryKey].UpdateUser = backdoorRequest.LastUpdatedBy;
                    }
                if (((backdoorRequest.LastUpdatedDate ?? Program.posixEpoche) > address.UpdateDate) &&
                    !string.IsNullOrEmpty(backdoorRequest.WantsRecyclingCart) &&
                    (backdoorRequest.WantsRecyclingCart.ToUpper().Equals("YES")))
                    {
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingPickup = true;
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingComment = "Modified by ETL " + AddressPopulation.AddressDictionary[dictionaryKey].RecyclingComment;
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatusDate = DateTime.Now;
                    }
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[dictionaryKey]);
                }
            else
                {
                BackDoorPickup backdoorPickup = new BackDoorPickup();
                buildRequestBackdoorService(backdoorRequest, ref backdoorPickup);
                backdoorPickup.AddressID = address.AddressID;
                backdoorPickup.ResidentID = resident.ResidentID;
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(backdoorPickup);
                if (((backdoorRequest.LastUpdatedDate ?? Program.posixEpoche) > address.UpdateDate) &&
                        !string.IsNullOrEmpty(backdoorRequest.WantsRecyclingCart) &&
                        (backdoorRequest.WantsRecyclingCart.ToUpper().Equals("YES")))
                    {
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingPickup = true;
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingComment = "Modified by ETL " + AddressPopulation.AddressDictionary[dictionaryKey].RecyclingComment;
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatusDate = DateTime.Now;
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[dictionaryKey]);
                    }
                backDoorPickupDictionary.Add(backdoorRequestKey, backdoorPickup);
                }

            }




        public BackDoorPickup buildRequestBackdoorService(BackdoorServiceRequest backdoorServiceRequest, ref BackDoorPickup backdoorPickup)
            {
            
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
