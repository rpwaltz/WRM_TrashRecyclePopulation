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

                // UPdate all old addresses, add any new addresses to a dictionary

                int numberRequestsUpdates = 0;
                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if ((numberRequestsUpdates > 0) && (numberRequestsUpdates % 2000 == 0))
                        {
                        // commit all updated addresses
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                       // Program.logLine = "Updated Recycling Addresses: " + numberRequestsSaved;
                       // WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                       // WRMLogger.Logger.log();
                        }

                    try
                        {
                        bool isAdded = populateAddressFromRecycleRequest(recyclingRequest);
                        if (isAdded)
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


               Program.logLine = "Adding Recycling Addresses: " + numberRequestsUpdates;
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
 
                foreach (Address address in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Address.ToList())
                    {
                    string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                    AddressPopulation.AddressIdentiferDictionary[dictionaryKey] = address.AddressID;
                    AddressPopulation.ReverseAddressIdentiferDictionary[address.AddressID] = dictionaryKey;
                    AddressPopulation.AddressDictionary[dictionaryKey] = address;
                    }
                // Update any new addresses
                // There can be two entries for a single address in the table.
                // figure out which entry is last and update the address with that information

                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if ((numberRequestsUpdates > 0) && (numberRequestsUpdates % 2000 == 0))
                        {
                        // commit all updates of newly added addresses
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        // Program.logLine = "Updated Recycling Addresses: " + numberRequestsSaved;
                        // WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        // WRMLogger.Logger.log();
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
 //               Program.logLine = "Updated Added Recycling Addresses: " + numberRequestsUpdates;
 //               WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
 //               WRMLogger.Logger.log();


                foreach (Address address in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Address.ToList())
                    {
                    string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
                    AddressPopulation.AddressIdentiferDictionary[dictionaryKey] = address.AddressID;
                    AddressPopulation.ReverseAddressIdentiferDictionary[address.AddressID] = dictionaryKey;
                    AddressPopulation.AddressDictionary[dictionaryKey] = address;
                    // WRMLogger.LogBuilder.AppendLine(dictionaryKey + " = " + address.AddressID);
                    // AddressPopulation.ReverseAddressIdentiferDictionary[address.AddressID] = dictionaryKey;
                    }
                // Add all Residents
                // Add all new residents
                numberRequestsSaved = 0;
                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if ((numberRequestsSaved > 0) && (numberRequestsSaved % 2000 == 0))
                        {

                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        // Program.logLine = "Processed Recycling Residents: " + numberRequestsSaved;
                        // WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        // WRMLogger.Logger.log();
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
 //               Program.logLine = "Add new Residents " + numberRequestsSaved;
 //               WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
 //               WRMLogger.Logger.log();


                numberRequestsSaved = 0;
                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if ((numberRequestsSaved > 0) && (numberRequestsSaved % 2000 == 0))
                        {

                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        // Program.logLine = "Processed Updating added Recycling Residents: " + numberRequestsSaved;
                        // WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        // WRMLogger.Logger.log();
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
//                Program.logLine = "Updated Added new Residents " + numberRequestsSaved;
//                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
//                WRMLogger.Logger.log();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                Dictionary<int, Resident> residentIdentiferForCommercialAccountDictionary = new Dictionary<int, Resident>();
                foreach (Resident resident in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Resident.ToList())
                    {
                    // update Commercial Account with the name of the name in the resident
                    string dictionaryKey = ReverseAddressIdentiferDictionary[resident.AddressID];
                    ResidentAddressPopulation.ResidentIdentiferDictionary[dictionaryKey] = resident.ResidentID;
                    residentIdentiferForCommercialAccountDictionary[resident.AddressID] = resident;
                    }
                /*
                foreach (CommercialAccount commercialAccount in CommercialAccountPopulation.CommercialAccountList)
                    {
                    Resident foundResident = new Resident();
                    if (residentIdentiferForCommercialAccountDictionary.TryGetValue(commercialAccount.AddressID, out foundResident))
                        {
                        commercialAccount.CommercialAccountName = foundResident.FirstName + " " + foundResident.LastName;
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(commercialAccount);
                        }
                    }
                */
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
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
                throw new Exception("populateAddressFromRecycleRequest Failed");
                }
            }

        // The boolean isAdded is true if the request is added to the RecyclingResidentAddressDictionary, if it is an update then it is false
        public bool populateAddressFromRecycleRequest(RecyclingRequest request)
            {
//            WRMLogger.LogBuilder.AppendLine("Dates of exising Recycling Request " +
//            System.Environment.NewLine + "Request Update Date " + request.LastUpdatedDate.ToString() +
//            System.Environment.NewLine + "Request Create Date " + request.CreationDate.ToString());

            bool isAdded = false;
            string status = request.Status.Trim();
            status = translateAddressRecyclingStatus(status);
            if (status.Equals("NOT RECYCLING"))
                {
                return isAdded;
                }
            string streetName = request.StreetName;
            int streetNumber = request.StreetNumber ?? 0;
            string zipCode = request.ZipCode;
            string unitNumber = null;
            if (request.UnitNumber != null)
                unitNumber = request.UnitNumber;

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);

//            WRMLogger.LogBuilder.AppendLine("PopulateAddressFromRecycleRequest " + dictionaryKey);
            Address foundAddress = new Address();
            if (AddressPopulation.AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                {

                // Found an address that is already added, update the address
                // compare dates, if request date is later than recycling dates, update

                // How does foundAddress.CreateDate equal address.CreateDate on first run?

                // add back String.IsNullOrEmpty(foundAddress.RecyclingStatus) ||
                if (!String.IsNullOrEmpty(request.Comments))
                    {
                    foundAddress.Comment = foundAddress.Comment + request.Comments;
                    }
                if ( String.IsNullOrEmpty(foundAddress.RecyclingStatus) ||
                    (((request.LastUpdatedDate ?? Program.posixEpoche) > (foundAddress.UpdateDate ?? Program.posixEpoche)) ||
                    (((request.LastUpdatedDate ?? Program.posixEpoche) == (foundAddress.UpdateDate ?? Program.posixEpoche))
                       && ((request.CreationDate ?? Program.posixEpoche) > (foundAddress.CreateDate ?? Program.posixEpoche)))))
                    {
 //                   WRMLogger.LogBuilder.AppendLine("Update existing Recycling Request at " + dictionaryKey + " for ID " + AddressPopulation.AddressDictionary[dictionaryKey].AddressID);
                    AddressPopulation.AddressDictionary[dictionaryKey].CreateDate = request.CreationDate;
                    AddressPopulation.AddressDictionary[dictionaryKey].CreateUser = request.CreatedBy;
                    AddressPopulation.AddressDictionary[dictionaryKey].UpdateDate = request.LastUpdatedDate;
                    AddressPopulation.AddressDictionary[dictionaryKey].UpdateUser = request.LastUpdatedBy;

                    if (!status.Equals("NOT RECYCLING"))
                        {
                        // confirm that a recycling cart exists, if it does not create one with SN UNKNOWN
                        AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatus = status;
                        AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatusDate = request.StatusDate;
                        }

                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[dictionaryKey]);
                    }
                else
                    {
                    WRMLogger.LogBuilder.AppendLine("Skip exising Recycling Request at " + dictionaryKey +
                        System.Environment.NewLine + "Request Recycling Status " + status +
                        System.Environment.NewLine + "Found Address Recycling Status " + foundAddress.RecyclingStatus +
                        System.Environment.NewLine + "Request Update Date " + request.LastUpdatedDate.ToString() +
                        System.Environment.NewLine + "Found Address Update Date " + foundAddress.UpdateDate.ToString() +
                        System.Environment.NewLine + "Request Create Date " + request.CreationDate.ToString() +
                        System.Environment.NewLine + "Found Address Create Date " + foundAddress.CreateDate.ToString() +
                        System.Environment.NewLine + "Epoche " + Program.posixEpoche.ToShortDateString());
                        ;
                    }
                }
            else if (!RecyclingResidentAddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                {
                Address address = buildResidentAddressFromRequest(request);
                if (address != null)
                    {
                    populateAddressFromKGIS(ref address);
                    // if an address does not already exist then do not add it if it is withdrawn
                    if (!status.Equals("NOT RECYCLING"))
                        {
//                        WRMLogger.LogBuilder.AppendLine("New Recycling Request at " + dictionaryKey);
                        address.CreateDate = request.CreationDate;
                        address.CreateUser = request.CreatedBy;
                        address.UpdateDate = request.LastUpdatedDate;
                        address.UpdateUser = request.LastUpdatedBy;
                        address.RecyclingStatus = status;
                        address.RecyclingStatusDate = request.StatusDate;
                        Address foundAddressPopulation = new Address();
                        if (AddressPopulation.AddressDictionary.TryGetValue((string)dictionaryKey, out foundAddressPopulation))
                            {
                            address.RecycleDayOfWeek = AddressPopulation.AddressDictionary[dictionaryKey].RecycleDayOfWeek;
                            address.RecycleFrequency = AddressPopulation.AddressDictionary[dictionaryKey].RecycleFrequency;
                            address.TrashDayOfWeek = AddressPopulation.AddressDictionary[dictionaryKey].TrashDayOfWeek;
                            }
                        
                        address.TrashStatus = "ELIGIBLE";
                        if (!String.IsNullOrEmpty(request.Comments))
                            {
                            address.Comment = request.Comments;
                            }
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(address);
                        RecyclingResidentAddressDictionary[dictionaryKey] = address;
                        }

                    isAdded = true;
                    }
                else
                    {
                    WRMLogger.LogBuilder.AppendLine("No address found for Recycling Request at " + dictionaryKey);
                    }

                }
            else
                {
                WRMLogger.LogBuilder.AppendLine("Bad Recycling Request at " + dictionaryKey);
                }
            return isAdded;
            }
        public bool updateAddressFromRecycleRequest(RecyclingRequest request)
            {
            bool isUpdated = false;
            string status = request.Status.Trim();
            status = translateAddressRecyclingStatus(status);
            if (status.Equals("NOT RECYCLING"))
                {
                return isUpdated;
                }
            string streetName = request.StreetName;
            int streetNumber = request.StreetNumber ?? 0;
            string zipCode = request.ZipCode;
            string unitNumber = null;
            if (request.UnitNumber != null)
                unitNumber = request.UnitNumber;

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
            Address foundAddress = new Address();


           if (RecyclingResidentAddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                {
                
                if (String.IsNullOrEmpty(foundAddress.RecyclingStatus) ||
                    ((request.LastUpdatedDate ?? Program.posixEpoche) > (foundAddress.UpdateDate ?? Program.posixEpoche)) ||
                (((request.LastUpdatedDate ?? Program.posixEpoche) == (foundAddress.UpdateDate ?? Program.posixEpoche))
                && ((request.CreationDate ?? Program.posixEpoche) > (foundAddress.CreateDate ?? Program.posixEpoche))))
  //              if (((address.UpdateDate ?? Program.posixEpoche) > (foundAddress.UpdateDate ?? Program.posixEpoche)) ||
  //                  (((address.UpdateDate ?? DateTime.Now) == (foundAddress.UpdateDate ?? Program.posixEpoche))
  //                      && ((address.CreateDate ?? DateTime.Now) > (foundAddress.CreateDate ?? Program.posixEpoche))))
//                    if ((request.LastUpdatedDate ?? Program.posixEpoche) > (foundAddress.UpdateDate ?? Program.posixEpoche))
                    {
                    RecyclingResidentAddressDictionary[dictionaryKey].CreateDate = request.CreationDate;
                    RecyclingResidentAddressDictionary[dictionaryKey].CreateUser = request.CreatedBy;
                    RecyclingResidentAddressDictionary[dictionaryKey].UpdateDate = request.LastUpdatedDate;
                    RecyclingResidentAddressDictionary[dictionaryKey].UpdateUser = request.LastUpdatedBy;
                    if (!status.Equals("NOT RECYCLING"))
                        {
                        
                        RecyclingResidentAddressDictionary[dictionaryKey].RecyclingStatus = status;
                        RecyclingResidentAddressDictionary[dictionaryKey].RecyclingStatusDate = request.StatusDate;
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
            status = translateAddressRecyclingStatus(status);

             if (status.Equals("NOT RECYCLING"))
                {
                return isAdded;
                }

            string streetName = request.StreetName;
            int streetNumber = request.StreetNumber ?? 0;
            string zipCode = request.ZipCode;
            string unitNumber = null;
            if (request.UnitNumber != null)
                unitNumber = request.UnitNumber;

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
            Resident resident = buildResidentFromRequest(request);
            Resident foundResident = new Resident();
            if (!ResidentAddressPopulation.ResidentDictionary.TryGetValue(dictionaryKey, out foundResident))

                {
                int foundAddressId;
                if (AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out foundAddressId))
                    {
                    resident.AddressID = foundAddressId;
                    
                    // WRMLogger.LogBuilder.AppendLine("Resident added at " + dictionaryKey);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(resident);
                    ResidentAddressPopulation.ResidentDictionary.Add(dictionaryKey, resident);
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
            status = translateAddressRecyclingStatus(status);

            if (status.Equals("NOT RECYCLING"))
                {
                return isUpdated;
                }

            string streetName = request.StreetName;
            int streetNumber = request.StreetNumber ?? 0;
            string zipCode = request.ZipCode;
            string unitNumber = null;
            if (request.UnitNumber != null)
                unitNumber = request.UnitNumber;

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
            Resident resident = buildResidentFromRequest(request);
            Resident foundResident = new Resident();
            if (ResidentAddressPopulation.ResidentDictionary.TryGetValue(dictionaryKey, out foundResident))
                {
                if (
                    ((resident.UpdateDate ?? Program.posixEpoche) > (foundResident.UpdateDate ?? Program.posixEpoche)) ||
                     (((resident.UpdateDate ?? Program.posixEpoche) == (foundResident.UpdateDate ?? Program.posixEpoche))
                        && ((resident.CreateDate ?? Program.posixEpoche) > (foundResident.CreateDate ?? Program.posixEpoche))))
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
        private string translateAddressRecyclingStatus(string status)
            {
            switch (status)
                {
                case "APPROVED":
                    status = "RECYCLING";
                    break;
                case "REQUESTED":
                    status = "REQUESTED";
                    break;
                case "DISAPPROVED":
                    status = "NOT RECYCLING";
                    break;
                case "WITHDRAWN":
                    status = "NOT RECYCLING";
                    break;
                //       throw new WRMWithdrawnStatusException("Ignore Recycling request with status of WITHDRAWN");
                default:
                    throw new Exception(" Invalid Recycling status :" + status);
                }
            return status;
            }

        }

    }
