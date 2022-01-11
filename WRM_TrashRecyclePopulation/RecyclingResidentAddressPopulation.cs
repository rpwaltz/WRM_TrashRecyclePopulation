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
                IEnumerable<KGISAddress> kgisCityResidentAddressList = WRM_TrashRecycleQueries.retrieveKgisCityResidentAddressList();


                IEnumerable<RecyclingRequest> orderedSolidWasteRecyclingRequestList = WRM_TrashRecycleQueries.retrieveRecyclingRequestList();

                foreach (RecyclingRequest recyclingRequest in orderedSolidWasteRecyclingRequestList)
                    {
                    if (maxToProcess % 100 == 0)
                        {
                        Program.logLine = "Processed Recycling Requests: " + maxToProcess;
                        WRMLogger.Logger.logMessageAndDeltaTime(Program.logLine, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                        WRMLogger.Logger.log();
//                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
//                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
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
                WRMLogger.LogBuilder.AppendLine(ex.StackTrace);
                WRMLogger.Logger.log();
                }
            return false;
            }

        override public void buildAndSaveTrashRecycleEntitiesFromRequest(dynamic recyclingRequest, IEnumerator<KGISAddress> foundKgisResidentAddressEnumerator, int numberOfUnits)
            {

            if (foundKgisResidentAddressEnumerator.Current == null)
                {
                foundKgisResidentAddressEnumerator.MoveNext();

                }

            KGISAddress kgisCityResidentAddress = foundKgisResidentAddressEnumerator.Current;
            string streetName = IdentifierProvider.normalizeStreetName(kgisCityResidentAddress.STREET_NAME);
            int streetNumber = Convert.ToInt32(kgisCityResidentAddress.ADDRESS_NUM);
            string zipCode = kgisCityResidentAddress.ZIP_CODE.ToString();
            string unitNumber = null;
            if (recyclingRequest.UnitNumber != null)
                unitNumber = recyclingRequest.UnitNumber;
            Address foundAddress = new Address();
            bool updateAddress = false;
            Address address = new Address();
            ;
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);

            if (AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                if (((foundAddress.UpdateDate ?? Program.posixEpoche) > (recyclingRequest.LastUpdatedDate ?? Program.posixEpoche)))
                    //update address status with recycling request
                    updateAddress = true;
                else
                    {
                    address = buildRecyclingAddress(recyclingRequest, kgisCityResidentAddress);
                    }


            Resident resident = buildRequestResident(recyclingRequest);
            if (updateAddress)
                {
                string status = (string)recyclingRequest.Status.Trim();
                foundAddress.RecyclingStatus = translateAddressStatus(status);

                if (foundAddress.Resident == null)
                    {
                    address.RecyclingStatus = translateAddressStatus(status);
                    if (resident != null)
                        {
                        resident.AddressID = foundAddress.AddressID;
                        foundAddress.Resident.Add(resident);
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(address);
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                        }
                    else
                        {
                        WRMLogger.LogBuilder.AppendLine("Unable to determine Resident");
                        WRMLogger.Logger.log();
                        }
                    }
                else
                    {
                    Resident foundResident = foundAddress.Resident.First();
                    foundResident.FirstName = resident.FirstName;
                    foundResident.LastName = resident.LastName;
                    foundResident.Email = resident.Email;
                    foundResident.Note = resident.Note;
                    foundResident.Phone = resident.Phone;
                    foundResident.UpdateDate = resident.UpdateDate;
                    foundResident.CreateDate = resident.CreateDate;
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(foundResident);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    }
                }
            else
                {
                address.NumberUnits = numberOfUnits.ToString();

                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(address);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                if (resident != null)
                    {
                    resident.AddressID = address.AddressID;
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(resident);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    }
                else
                    {
                    WRMLogger.LogBuilder.AppendLine("Unable to determine Resident");
                    WRMLogger.Logger.log();
                    }
                //               resident.AddressID = address.AddressID;
                //               WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(resident);
                //               WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                //               WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                }
            if (!updateAddress)
                {
                AddressDictionary.Add(dictionaryKey, address);
                }
            }


        private Address buildRecyclingAddress(dynamic recyclingRequest, KGISAddress kgisCityResidentAddress)
            {
            Address address = buildRequestAddress(recyclingRequest, kgisCityResidentAddress);

            address.RecyclingPickup = recyclingRequest.Status.Equals("APPROVED");
            string status = (string)recyclingRequest.Status.Trim();
            address.RecyclingStatus = translateAddressStatus(status);

            address.RecyclingStatusDate = recyclingRequest.StatusDate;
            address.RecyclingRequestedDate = recyclingRequest.CreationDate;
            return address;
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
                    status = "REJECTED";
                    break;
                default:
                    throw new Exception(" Invalid Recycling status :" + status);
                }
            return status;
            }



        }


    }
