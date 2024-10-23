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
                // WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                // WRMLogger.Logger.log();

                int numberRequestsSaved = 0;
                IEnumerable<BackdoorServiceRequest> orderedSolidWasteBackdoorRequestList = WRM_EntityFrameworkContextCache.SolidWasteContext.BackdoorServiceRequest.OrderBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.StreetName).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.StreetNumber).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.UnitNumber).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.BackdoorId).ToList();

                // Add new Residents
                numberRequestsSaved = 0;
                foreach (BackdoorServiceRequest backdoorRequest in orderedSolidWasteBackdoorRequestList)
                    {
                    if ((numberRequestsSaved > 0) && (numberRequestsSaved % 1000 == 0))
                        {
                        // commit all newly added addresses
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        // Program.logLine = "Added Backdoor Resident Requests: " + numberRequestsSaved;
                        // WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        // WRMLogger.Logger.log();

                        }
                    try
                        {
                        if (buildBackDoorPickupResidentsFromRequest(backdoorRequest))
                            {
                            ++numberRequestsSaved;
                            }
                        }
                    catch (Exception ex) when (ex is WRMWithdrawnStatusException || ex is WRMNotSupportedException || ex is WRMNullValueException)
                        {
                        WRMLogger.LogBuilder.AppendLine(ex.Message);
                        }

                    }
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                foreach (Resident resident in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Resident.ToList())
                    {
                    string dictionaryKey = ReverseAddressIdentiferDictionary[resident.AddressID];
                    ResidentAddressPopulation.ResidentIdentiferDictionary[dictionaryKey] = resident.ResidentID;
                    }

                // Update Existing Residents
                numberRequestsSaved = 0;
                foreach (BackdoorServiceRequest backdoorRequest in orderedSolidWasteBackdoorRequestList)
                    {
                    if ((numberRequestsSaved > 0) && (numberRequestsSaved % 1000 == 0))
                        {
                        // commit all newly added addresses
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        // Program.logLine = "Added Backdoor Resident Requests: " + numberRequestsSaved;
                        // WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        // WRMLogger.Logger.log();

                        }
                    try
                        {
                        if (updateBackDoorPickupResidentsFromRequest(backdoorRequest))
                            {
                            ++numberRequestsSaved;
                            }
                        }
                    catch (Exception ex) when (ex is WRMWithdrawnStatusException || ex is WRMNotSupportedException || ex is WRMNullValueException)
                        {
                        WRMLogger.LogBuilder.AppendLine(ex.Message);
                        }

                    }
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();

                // Add new Backdoor entries
                numberRequestsSaved = 0;
                foreach (BackdoorServiceRequest backdoorRequest in orderedSolidWasteBackdoorRequestList)
                    {
                    if ((numberRequestsSaved > 0) && (numberRequestsSaved % 1000 == 0))
                        {
                        // commit all newly added addresses
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        // Program.logLine = "Added Backdoor Resident Requests: " + numberRequestsSaved;
                        // WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        // WRMLogger.Logger.log();

                        }
                    try
                        {
                        if (buildBackDoorPickupFromRequest(backdoorRequest))
                            {
                            ++numberRequestsSaved;
                            }
                        }
                    catch (Exception ex) when (ex is WRMWithdrawnStatusException || ex is WRMNotSupportedException || ex is WRMNullValueException)
                        {
                        WRMLogger.LogBuilder.AppendLine(ex.Message);
                        }

                    }
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();

                // Delete and Add duplicate entries (keep all entries)



                foreach (BackdoorServiceRequest backdoorRequest in orderedSolidWasteBackdoorRequestList)
                    {

                    try
                        {
                        updateBackDoorPickupFromRequest(backdoorRequest);

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

        
        public bool buildBackDoorPickupFromRequest(BackdoorServiceRequest backdoorRequest)
            {
            bool isAdded = false;
            string streetName = backdoorRequest.StreetName;
            int streetNumber = backdoorRequest.StreetNumber ?? 0;
            string zipCode = backdoorRequest.ZipCode;
            string unitNumber = backdoorRequest.UnitNumber;
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
            int foundAddressId = 0;
            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out foundAddressId))
                {

                throw new WRMNullValueException("Backdoor Unable to find Address1 : [" + streetNumber + " " + streetName + " " + unitNumber + " " + zipCode + "]");
                }

            BackDoorPickup backdoorPickup = buildRequestBackdoorService(backdoorRequest);
            
            backdoorPickup.AddressID = foundAddressId;

            string backdoorRequestKey = foundAddressId.ToString();
            
            BackDoorPickup foundBackDoorPickup = new BackDoorPickup();

            if (!BackDoorPickupDictionary.TryGetValue(backdoorRequestKey, out foundBackDoorPickup))
                {
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(backdoorPickup);
                
                BackDoorPickupDictionary[backdoorRequestKey] = backdoorPickup;
                isAdded = true;
                }
            return isAdded;
            }


        public void updateBackDoorPickupFromRequest(BackdoorServiceRequest backdoorRequest)
            {
            string streetName = backdoorRequest.StreetName;
            int streetNumber = backdoorRequest.StreetNumber ?? 0;
            string zipCode = backdoorRequest.ZipCode;
            string unitNumber = backdoorRequest.UnitNumber;
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
            int foundAddressId = 0;
            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out foundAddressId))
                {

                throw new WRMNullValueException("Update Backdoor Unable to find Address2\r\n: [" + streetNumber + " " + streetName + " " + unitNumber + " " + zipCode + "]");
                }
        BackDoorPickup backdoorPickup = buildRequestBackdoorService(backdoorRequest);

            backdoorPickup.AddressID = foundAddressId;

            string backdoorRequestKey = foundAddressId.ToString();

            BackDoorPickup foundBackDoorPickup = new BackDoorPickup();

            if (BackDoorPickupDictionary.TryGetValue(backdoorRequestKey, out foundBackDoorPickup))
                {
                // if the new backdoor request is newer than the previous one
                if ( ((backdoorPickup.UpdateDate ?? Program.posixEpoche) > (foundBackDoorPickup.UpdateDate ?? Program.posixEpoche) )
                    || (((backdoorPickup.UpdateDate ?? DateTime.Now) == (foundBackDoorPickup.UpdateDate ?? Program.posixEpoche)) &&
                    ((backdoorPickup.CreateDate ?? DateTime.Now) > (foundBackDoorPickup.CreateDate ?? Program.posixEpoche))) )
                    {
                    
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Remove(BackDoorPickupDictionary[backdoorRequestKey]);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(backdoorPickup);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    BackDoorPickupDictionary[backdoorRequestKey] = backdoorPickup;
                    }
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
                case "WITHDRAWN":
                    status = "WITHDRAWN";
                    break;
                default:
                    throw new WRMWithdrawnStatusException(" Invalid Recycling status :" + status);
                }
            return status;
            }
        public bool buildBackDoorPickupResidentsFromRequest(dynamic request)
            {
            bool isAdded = false;
            string streetName = request.StreetName;
            int streetNumber = request.StreetNumber ?? 0;
            string zipCode = request.ZipCode;
            string unitNumber = null;
            if (request.UnitNumber != null)
                unitNumber = request.UnitNumber;

            Resident foundResident = new Resident();

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
            int addressId = 0;
            /* 
             * I don't remember why I wanted to throw an error if the address is not already in the database, 
             * presumably because if the address had a cart then they would already be in the database
             * So that any request we have in the database that hasn't a cart issued, but is active, needs to be looked into
             * Of course the address may be entered incorrectly
             */
             
            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out addressId))
                {
                throw new WRMNullValueException("Backdoor Unable to find Address3 [" + streetNumber + " " + streetName + " " + unitNumber + " " + zipCode + "]");
                }
                
            if (!ResidentAddressPopulation.ResidentDictionary.TryGetValue(dictionaryKey, out foundResident))
                {

                Resident resident = buildResidentFromRequest(request);
                resident.AddressID = addressId;
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(resident);

                ResidentAddressPopulation.ResidentDictionary[dictionaryKey] = resident;
                isAdded = true;
                }
            return isAdded;
            }
        public bool updateBackDoorPickupResidentsFromRequest(dynamic request)
            {
            bool isAdded = false;
            string streetName = request.StreetName;
            int streetNumber = request.StreetNumber ?? 0;
            string zipCode = request.ZipCode;
            string unitNumber = null;
            if (request.UnitNumber != null)
                unitNumber = request.UnitNumber;

            Resident foundResident = new Resident();

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
            int addressId = 0;
            if (!AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out addressId))
                {
                throw new WRMNullValueException("Backdoor Unable to find Address4 [" + streetNumber + " " + streetName + " " + unitNumber + " " + zipCode + "]");
                }

            if (ResidentAddressPopulation.ResidentDictionary.TryGetValue(dictionaryKey, out foundResident))
                {
                List<Resident> existingResidentList = WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Resident.Where(a => a.AddressID == addressId).ToList();
                if (existingResidentList.Count == 0)
                    {
                    throw new WRMNullValueException("Resident exists in ResidentDictionary but is not found in Database " + foundResident.LastName + " " + dictionaryKey);
                    }
                //Resident existingResident = existingResidentList.First();
                Resident resident = buildResidentFromRequest(request);
                foreach (Resident existingResident in existingResidentList)
                    {
                    // compare dates, if request date is later than resident dates, update

                    if (((resident.UpdateDate ?? Program.posixEpoche) > (existingResident.UpdateDate ?? Program.posixEpoche))
                        || (((resident.UpdateDate ?? DateTime.Now) == (existingResident.UpdateDate ?? Program.posixEpoche)) &&
                        ((resident.CreateDate ?? DateTime.Now) > (existingResident.CreateDate ?? Program.posixEpoche))))
                        {
                        
                        existingResident.Email = resident.Email;
                        existingResident.FirstName = resident.FirstName;
                        existingResident.LastName = resident.LastName;

                        existingResident.Phone = resident.Phone;

                        existingResident.CreateDate = resident.CreateDate;
                        existingResident.CreateUser = resident.CreateUser;
                        existingResident.UpdateDate = resident.UpdateDate;
                        existingResident.UpdateUser = resident.UpdateUser;
                        existingResident.SendEmailNewsletter = resident.SendEmailNewsletter;
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(existingResident);
                        isAdded = true;
                        }
                    }
                }
            return isAdded;
            }
        }

    }
