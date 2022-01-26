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

                int numberRequestsSaved = 0;
                IEnumerable<RecyclingRequest> orderedSolidWasteRecyclingRequestList = WRM_EntityFrameworkContextCache.SolidWasteContext.RecyclingRequest.OrderBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.StreetName).ThenBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.StreetNumber).ThenBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.UnitNumber).ThenBy(solidWasteRecyclingRequestList => solidWasteRecyclingRequestList.Id).ToList();




                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if (numberRequestsSaved % 100 == 0)
                        {
                        Program.logLine = "Processed Recycling Requests: " + numberRequestsSaved;
                        WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        }

                    try
                        {
                        populateResidentAddressFromRecycleRequest(recyclingRequest);
                        ++numberRequestsSaved;
                        }
                    catch (Exception ex) when (ex is WRMWithdrawnStatusException || ex is WRMNotSupportedException || ex is WRMNullValueException)
                        {
                        WRMLogger.LogBuilder.AppendLine(ex.Message);
                        }


                    }

                Program.logLine = "Finished Recycling Requests " + numberRequestsSaved;
                WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                WRMLogger.Logger.log();
                foreach (Resident resident in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Resident.ToList())
                    {
                    string dictionaryKey = ReverseAddressIdentiferDictionary[resident.AddressID ?? 0];
                    ResidentIdentiferDictionary[dictionaryKey] = resident.ResidentID;
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

        public void populateResidentAddressFromRecycleRequest(RecyclingRequest request)
            {
            string status = request.Status.Trim();
            status = translateAddressStatus(status);
            Address address = buildAndAddResidentAddressFromRequest(request);
            Resident resident = buildResidentFromRequest(request);
            resident.AddressID = address.AddressID;
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
            Address foundAddress = new Address();
            if (AddressPopulation.AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                {
                // compare dates, if request date is later than recycling dates, update
                if ((request.LastUpdatedDate ?? Program.posixEpoche) > (address.UpdateDate ?? Program.posixEpoche))
                    {
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    AddressPopulation.AddressDictionary[dictionaryKey].CreateDate = request.CreationDate;
                    AddressPopulation.AddressDictionary[dictionaryKey].CreateUser = request.CreatedBy;
                    AddressPopulation.AddressDictionary[dictionaryKey].UpdateDate = request.LastUpdatedDate;
                    AddressPopulation.AddressDictionary[dictionaryKey].UpdateUser = request.LastUpdatedBy;

                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatus = status;
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingPickup = status.Equals("APPROVED");
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatusDate = request.StatusDate;

                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[dictionaryKey]);
                    // update resident
                    Resident foundResident = new Resident();
                    if (ResidentDictionary.TryGetValue(dictionaryKey, out foundResident))
                        {
                        ResidentDictionary[dictionaryKey].FirstName = resident.FirstName;
                        ResidentDictionary[dictionaryKey].LastName = resident.LastName;
                        ResidentDictionary[dictionaryKey].Email = resident.Email;
                        if (!String.IsNullOrWhiteSpace(resident.Phone))
                            {
                            string phoneNumber = reducePhoneNumber.Replace(request.PhoneNumber, string.Empty).Trim();
                            resident.Phone = phoneNumber;
                            }
                        if (String.IsNullOrEmpty(request.SendEmailNewsletter) || request.SendEmailNewsletter.Equals("N"))
                            {
                            ResidentDictionary[dictionaryKey].SendEmailNewsletter = false;
                            }
                        else
                            {
                            ResidentDictionary[dictionaryKey].SendEmailNewsletter = true;
                            }
                        ResidentDictionary[dictionaryKey].CreateDate = request.CreationDate;
                        ResidentDictionary[dictionaryKey].CreateUser = request.CreatedBy;
                        ResidentDictionary[dictionaryKey].UpdateDate = request.LastUpdatedDate;
                        ResidentDictionary[dictionaryKey].UpdateUser = request.LastUpdatedBy;

                        ResidentDictionary[dictionaryKey].AddressID = address.AddressID;
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(ResidentDictionary[dictionaryKey]);
                        }
                    else
                        {
                        // add a new resident to an existing address
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(resident);
                        ResidentDictionary[dictionaryKey] = resident;
                        }
                    }
                else
                    {
                    if (String.IsNullOrEmpty(request.SendEmailNewsletter) || request.SendEmailNewsletter.Equals("N"))
                        {
                        resident.SendEmailNewsletter = false;
                        }
                    else
                        {
                        resident.SendEmailNewsletter = true;
                        }
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatus = status;
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingPickup = status.Equals("APPROVED");
                    AddressPopulation.AddressDictionary[dictionaryKey].RecyclingStatusDate = request.StatusDate;
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(AddressPopulation.AddressDictionary[dictionaryKey]);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(resident);
                    ResidentDictionary[dictionaryKey] = resident;
                    }
                }
            else
                {
                throw new WRMNullValueException("Address not found in Dictionary  [" + dictionaryKey + "]");
                }
            

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
