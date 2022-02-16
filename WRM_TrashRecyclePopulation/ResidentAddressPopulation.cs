using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation
    {
    abstract class ResidentAddressPopulation : AddressPopulation
        {
        private static Dictionary<string, Resident> residentDictionary = new Dictionary<string, Resident>();

        public static Dictionary<string, Resident> ResidentDictionary { get => residentDictionary; set => residentDictionary = value; }

        //       abstract public void buildAndSaveTrashRecycleEntitiesFromRequest(dynamic request, IEnumerator<KGISAddress> foundKgisResidentAddressEnumerator, int numberOfUnits);

        private static Dictionary<string, int> residentIdentiferDictionary = new Dictionary<string, int>();
        public static Dictionary<string, int> ResidentIdentiferDictionary { get => residentIdentiferDictionary; set => residentIdentiferDictionary = value; }

        static public Regex reducePhoneNumber = new Regex("[^\\d]");

        public Resident buildResidentFromRequest(dynamic request)
            {
            Resident resident = new Resident();
            resident.Email = request.Email;
            resident.FirstName = request.FirstName;
            resident.LastName = request.LastName;

            if (request.GetType().GetProperty("PhoneNumber") != null)
                {
                if (!String.IsNullOrWhiteSpace(resident.Phone))
                    {
                    string phoneNumber = reducePhoneNumber.Replace(request.PhoneNumber, string.Empty).Trim();
                    resident.Phone = phoneNumber;
                    }
                }

            if ((request.GetType().GetProperty("SendEmailNewsletter") != null))
                {
                if (String.IsNullOrEmpty(request.SendEmailNewsletter) || request.SendEmailNewsletter.Equals("N"))
                    {
                    resident.SendEmailNewsletter = false;
                    }
                else
                    {
                    resident.SendEmailNewsletter = true;
                    }
                }
            else
                {
                resident.SendEmailNewsletter = false;
                }
            resident.CreateDate = request.CreationDate;
            resident.CreateUser = request.CreatedBy;
            resident.UpdateDate = request.LastUpdatedDate;
            resident.UpdateUser = request.LastUpdatedBy;
            return resident;
            }


        public void addOrUpdateResidentFromRequestToWRM_TrashRecycle(dynamic request)
            {

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
                throw new WRMNullValueException("Address is not found for " + dictionaryKey);
                }

            if (ResidentAddressPopulation.ResidentDictionary.TryGetValue(dictionaryKey, out foundResident))
                {
                List<Resident> existingResidentList = WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Resident.Where(a => a.AddressID == addressId).ToList();
                if (existingResidentList.Count == 0)
                    {
                    throw new WRMNullValueException("Resident exists in ResidentDictionary but is not found in Database " + foundResident.LastName + " " +  dictionaryKey);
                    }
                Resident existingResident = existingResidentList.First();
                // compare dates, if request date is later than resident dates, update
                if ((request.LastUpdatedDate ?? Program.posixEpoche) > (existingResident.UpdateDate ?? Program.posixEpoche))
                    {
                    Resident resident = buildResidentFromRequest(request);
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
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    }
                }
            else
                {
                Resident resident = buildResidentFromRequest(request);
                resident.AddressID = addressId;
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(resident);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                Resident newResident = WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Resident.Where(a => a.AddressID == addressId).ToList().First();
                
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                ResidentAddressPopulation.ResidentDictionary[dictionaryKey] = newResident;
                ResidentAddressPopulation.ResidentIdentiferDictionary[dictionaryKey] = newResident.ResidentID;
                }

            }
        
        }
    }
