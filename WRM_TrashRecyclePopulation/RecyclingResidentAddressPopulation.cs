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
        Dictionary<string,Address> RecyclingResidentAddressDictionary = new Dictionary<string,Address>();
        public bool populateRecyclingResidentAddress()
            {
            try
                {
                Program.logLine = "Begin Recycling Requests";
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();

                int numberRequestsSaved = 0;
                IEnumerable<RecyclingRequest> orderedSolidWasteRecyclingRequestList = WRM_EntityFrameworkContextCache.SolidWasteContext.RecyclingRequest.OrderBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.StreetName).ThenBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.StreetNumber).ThenBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.UnitNumber).ThenBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.Id).ToList();

                // Add all Residents

                // UPdate all old addresses

                int numberRequestsUpdates = 0;
                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if ((numberRequestsUpdates > 0) && (numberRequestsUpdates % 2000 == 0))
                        {
                        // commit all updated addresses
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        Program.logLine = "Updated Recycling Addresses: " + numberRequestsSaved;
                        WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        }

                    try
                        {
                        bool isAdded = populateAddressFromRecycleRequest(recyclingRequest);
                        if (!isAdded)
                            {
                            numberRequestsUpdates++;
                            }
                        }
                    catch (Exception ex) when (ex is WRMWithdrawnStatusException || ex is WRMNotSupportedException || ex is WRMNullValueException)
                        {
                        WRMLogger.LogBuilder.AppendLine(ex.Message);
                        }

                    }

                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                Program.logLine = "Updated Recycling Addresses: " + numberRequestsUpdates;
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();

                // Add all new addresses
                numberRequestsSaved = 0;
                foreach (String dictionaryKey in RecyclingResidentAddressDictionary.Keys)
                    {
                    if ((numberRequestsSaved > 0) && (numberRequestsSaved % 2000 == 0))
                        {
                        // commit all newly added addresses
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        Program.logLine = "Processed Recycling Requests: " + numberRequestsSaved;
                        WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();

                        }

                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(RecyclingResidentAddressDictionary[dictionaryKey]);
                    AddressPopulation.AddressDictionary[dictionaryKey] = RecyclingResidentAddressDictionary[dictionaryKey];
                    numberRequestsSaved++;
                    }
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                Program.logLine = "Added Recycling Addresses: " + numberRequestsSaved;
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();

                // Update any new addresses

                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if ((numberRequestsUpdates > 0) && (numberRequestsUpdates % 2000 == 0))
                        {
                        // commit all updates of newly added addresses
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        Program.logLine = "Updated Recycling Addresses: " + numberRequestsSaved;
                        WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        }

                    try
                        {
                        bool isUpdated = updateAddressFromRecycleRequest(recyclingRequest);
                        if (isUpdated)
                            {
                            numberRequestsUpdates++;
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
                Program.logLine = "Updated Added Recycling Addresses: " + numberRequestsUpdates;
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();


                foreach (Address address in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Address.ToList())
                    {
                    string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                    AddressPopulation.AddressIdentiferDictionary[dictionaryKey] = address.AddressID;
                    WRMLogger.LogBuilder.AppendLine(dictionaryKey + " = " + address.AddressID);
                    AddressPopulation.ReverseAddressIdentiferDictionary[address.AddressID] = dictionaryKey;
                    }

                // Add all new residents
                numberRequestsSaved = 0;
                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if ((numberRequestsSaved > 0) && (numberRequestsSaved % 2000 == 0))
                        {

                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        Program.logLine = "Processed Recycling Residents: " + numberRequestsSaved;
                        WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        }

                    try
                        {
                        bool isAdded = populateResidentFromRecycleRequest(recyclingRequest);
                        if (isAdded)
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
                Program.logLine = "Add new Residents " + numberRequestsSaved;
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();


                numberRequestsSaved = 0;
                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if ((numberRequestsSaved > 0) && (numberRequestsSaved % 2000 == 0))
                        {

                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        Program.logLine = "Processed Updating added Recycling Residents: " + numberRequestsSaved;
                        WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        }

                    try
                        {
                        bool isUpdated = updateResidentFromRecycleRequest(recyclingRequest);
                        if (isUpdated)
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
                Program.logLine = "Updated Added new Residents " + numberRequestsSaved;
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();

                foreach (Resident resident in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Resident.ToList())
                    {
                    string dictionaryKey = ReverseAddressIdentiferDictionary[resident.AddressID ?? 0];
                    ResidentAddressPopulation.ResidentIdentiferDictionary[dictionaryKey] = resident.ResidentID;
                    }

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

        // The boolean isAdded is true if the request is added to the RecyclingResidentAddressDictionary, if it is an update then it is false
        public bool populateAddressFromRecycleRequest(RecyclingRequest request)
            {
            bool isAdded = true;
            string status = request.Status.Trim();
            status = translateAddressStatus(status);
            Address address = buildResidentAddressFromRequest(request);
            populateAddressFromKGIS(ref address);

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
            Address foundAddress = new Address();
            if (AddressPopulation.AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                {
                // Found an address that is already added, update the address
                // compare dates, if request date is later than recycling dates, update
                AddressPopulation.AddressDictionary[dictionaryKey].CreateDate = request.CreationDate;
                AddressPopulation.AddressDictionary[dictionaryKey].CreateUser = request.CreatedBy;
                AddressPopulation.AddressDictionary[dictionaryKey].UpdateDate = request.LastUpdatedDate;
                AddressPopulation.AddressDictionary[dictionaryKey].UpdateUser = request.LastUpdatedBy;

                if (!status.Equals("WITHDRAWN"))
                    {
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatus = status;
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingPickup = status.Equals("APPROVED");
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatusDate = request.StatusDate;
                    }
                else
                    {
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingPickup = false;
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatus = "WITHDRAWN";
                    }
                isAdded = false;
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[dictionaryKey]);
                }
            else if (!RecyclingResidentAddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                {
                if (address != null)
                    {
                    // if an address does not already exist then do not add it if it is withdrawn
                    if (!status.Equals("WITHDRAWN"))
                        {
                        address.CreateDate = request.CreationDate;
                        address.CreateUser = request.CreatedBy;
                        address.UpdateDate = request.LastUpdatedDate;
                        address.UpdateUser = request.LastUpdatedBy;
                        address.RecyclingStatus = status;
                        address.RecyclingPickup = status.Equals("APPROVED");
                        address.RecyclingStatusDate = request.StatusDate;
                        RecyclingResidentAddressDictionary[dictionaryKey] = address;
                        }
                    

                    }

                }
            return isAdded;
            }
        public bool updateAddressFromRecycleRequest(RecyclingRequest request)
            {
            bool isUpdated = false;
            string status = request.Status.Trim();
            status = translateAddressStatus(status);
            Address address = buildResidentAddressFromRequest(request);
            populateAddressFromKGIS(ref address);

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
            Address foundAddress = new Address();

           /* THIS LOGIC IS PROBABLY WRONG */
           if (RecyclingResidentAddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                {
                if (((address.UpdateDate ?? Program.posixEpoche) > (foundAddress.UpdateDate ?? Program.posixEpoche)) ||
                    (((address.UpdateDate ?? DateTime.Now) == (foundAddress.UpdateDate ?? Program.posixEpoche))
                        && ((address.CreateDate ?? DateTime.Now) > (foundAddress.CreateDate ?? Program.posixEpoche))))
                    if ((request.LastUpdatedDate ?? Program.posixEpoche) > (foundAddress.UpdateDate ?? Program.posixEpoche))
                    {
                    RecyclingResidentAddressDictionary[dictionaryKey].CreateDate = request.CreationDate;
                    RecyclingResidentAddressDictionary[dictionaryKey].CreateUser = request.CreatedBy;
                    RecyclingResidentAddressDictionary[dictionaryKey].UpdateDate = request.LastUpdatedDate;
                    RecyclingResidentAddressDictionary[dictionaryKey].UpdateUser = request.LastUpdatedBy;
                    if (!status.Equals("WITHDRAWN"))
                        {
                        
                        RecyclingResidentAddressDictionary[dictionaryKey].RecyclingStatus = status;
                        RecyclingResidentAddressDictionary[dictionaryKey].RecyclingPickup = status.Equals("APPROVED");
                        RecyclingResidentAddressDictionary[dictionaryKey].RecyclingStatusDate = request.StatusDate;
                        }
                    else
                        {
                        RecyclingResidentAddressDictionary[dictionaryKey].RecyclingPickup = false;
                        RecyclingResidentAddressDictionary[dictionaryKey].RecyclingStatus = "WITHDRAWN";

                        }
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(RecyclingResidentAddressDictionary[dictionaryKey]);
                    isUpdated = true;
                    }
                }
           return isUpdated;
            }
        public bool populateResidentFromRecycleRequest(RecyclingRequest request)
            {
            bool isAdded = false;
            string status = request.Status.Trim();
            status = translateAddressStatus(status);
            if (status.Equals("WITHDRAWN"))
                {
                return isAdded;
                }
            Address address = buildResidentAddressFromRequest(request);
            populateAddressFromKGIS(ref address);

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
            Resident resident = buildResidentFromRequest(request);
            Resident foundResident = new Resident();
            if (!ResidentAddressPopulation.ResidentDictionary.TryGetValue(dictionaryKey, out foundResident))

                {
                int foundAddressId;
                if (AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out foundAddressId))
                    {
                    resident.AddressID = foundAddressId;
                    ResidentAddressPopulation.ResidentDictionary.Add(dictionaryKey, resident);
                    WRMLogger.LogBuilder.AppendLine("Resident added at " + dictionaryKey);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(ResidentAddressPopulation.ResidentDictionary[dictionaryKey]);
                    isAdded = true;
                    }
                else
                    {
                    throw new WRMNullValueException("Address Id is not found for new resident at " + dictionaryKey);
                    }
                }
            else
                {
                WRMLogger.LogBuilder.AppendLine("Resident already exists at " + dictionaryKey);
                }
            return isAdded;
            }
        public bool updateResidentFromRecycleRequest(RecyclingRequest request)
            {
            bool isUpdated = false;
            string status = request.Status.Trim();
            status = translateAddressStatus(status);
            if (status.Equals("WITHDRAWN"))
                {
                return isUpdated;
                }
            Address address = buildResidentAddressFromRequest(request);
            populateAddressFromKGIS(ref address);

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
            Resident resident = buildResidentFromRequest(request);
            Resident foundResident = new Resident();
            if (ResidentAddressPopulation.ResidentDictionary.TryGetValue(dictionaryKey, out foundResident))
                {
                if (((resident.UpdateDate ?? Program.posixEpoche) > (foundResident.UpdateDate ?? Program.posixEpoche)) ||
                     (((resident.UpdateDate ?? DateTime.Now) == (foundResident.UpdateDate ?? Program.posixEpoche))
                        && ((resident.CreateDate ?? DateTime.Now) > (foundResident.CreateDate ?? Program.posixEpoche))))
                    {
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].FirstName = resident.FirstName;
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].LastName = resident.LastName;
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].Email = resident.Email;
                    if (!String.IsNullOrWhiteSpace(resident.Phone))
                        {
                        string phoneNumber = reducePhoneNumber.Replace(request.PhoneNumber, string.Empty).Trim();
                        ResidentAddressPopulation.ResidentDictionary[dictionaryKey].Phone = phoneNumber;
                        }
                    if (String.IsNullOrEmpty(request.SendEmailNewsletter))
                        {
                        ResidentAddressPopulation.ResidentDictionary[dictionaryKey].SendEmailNewsletter = false;
                        }
                    else
                        {
                        if (request.SendEmailNewsletter.Equals("N"))
                            {
                            ResidentAddressPopulation.ResidentDictionary[dictionaryKey].SendEmailNewsletter = false;
                            }
                        else
                            {
                            ResidentAddressPopulation.ResidentDictionary[dictionaryKey].SendEmailNewsletter = true;
                            }
                        }
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].CreateDate = request.CreationDate;
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].CreateUser = request.CreatedBy;
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].UpdateDate = request.LastUpdatedDate;
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].UpdateUser = request.LastUpdatedBy;
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(ResidentAddressPopulation.ResidentDictionary[dictionaryKey]);
                    isUpdated = true;

                    }
                }
            return isUpdated;
            }
        private string translateAddressStatus(string status)
            {
            switch (status)
                {
                case "APPROVED":
                    status = "APPROVED";
                    break;
                case "REQUESTED":
                    status = "REQUESTED";
                    break;
                case "DISAPPROVED":
                    status = "NOT APPROVED";
                    break;
                case "WITHDRAWN":
                    status = "WITHDRAWN";
                    break;
                //       throw new WRMWithdrawnStatusException("Ignore Recycling request with status of WITHDRAWN");
                default:
                    throw new Exception(" Invalid Recycling status :" + status);
                }
            return status;
            }

        }

    }
