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
            if (!String.IsNullOrWhiteSpace(resident.Phone))
                {
                string phoneNumber = reducePhoneNumber.Replace(request.PhoneNumber, string.Empty).Trim();
                resident.Phone = phoneNumber;
                }
            resident.CreateDate = request.CreationDate;
            resident.CreateUser = request.CreatedBy;
            resident.UpdateDate = request.LastUpdatedDate;
            resident.UpdateUser = request.LastUpdatedBy;
            return resident;
            }

        public Address buildAndAddResidentAddressFromRequest(dynamic request)
            {
            Address address = new Address();

            string streetName = request.StreetName;
            int streetNumber = request.StreetNumber ?? 0;
            string zipCode = request.ZipCode;
            string unitNumber = null;
            if (request.UnitNumber != null)
                unitNumber = request.UnitNumber;
            Address foundAddress = new Address();
            int foundAddressId = 0;
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
            if (AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out foundAddressId))
                {
                if (AddressPopulation.AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                    {
                    address = foundAddress;

                    }
                else
                    {
                    throw new WRMNullValueException("Address id = " + foundAddressId + " but no Address object found in AddressDictionary");
                    }
                }
            else
                {
                address.StreetName = streetName;
                address.StreetNumber = streetNumber;
                address.UnitNumber = unitNumber;
                address.ZipCode = zipCode;
                address.CreateDate = request.CreationDate;
                address.CreateUser = request.CreatedBy;
                address.UpdateDate = request.LastUpdatedDate;
                address.UpdateUser = request.LastUpdatedBy;
                addAddressToWRM_TrashRecycle(address);
                }

            return address;
            }
        public Resident addOrUpdateResidentFromRequestToWRM_TrashRecycle(dynamic request)
            {

            string streetName = request.StreetName;
            int streetNumber = request.StreetNumber ?? 0;
            string zipCode = request.ZipCode;
            string unitNumber = null;
            if (request.UnitNumber != null)
                unitNumber = request.UnitNumber;
            Resident resident = null;
            Resident foundResident = new Resident();
            int foundResidentId = 0;
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
            int addressId = AddressPopulation.AddressIdentiferDictionary[dictionaryKey];
            if (ResidentAddressPopulation.ResidentIdentiferDictionary.TryGetValue(dictionaryKey, out foundResidentId))
                {
                resident = foundResident;

                // compare dates, if request date is later than resident dates, update
                if ((request.LastUpdatedDate ?? Program.posixEpoche) > (resident.UpdateDate ?? Program.posixEpoche))
                    {

                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].Email = request.Email;
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].FirstName = request.FirstName;
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].LastName = request.LastName;
                    if (!String.IsNullOrWhiteSpace(ResidentAddressPopulation.ResidentDictionary[dictionaryKey].Phone))
                        {
                        string phoneNumber = reducePhoneNumber.Replace(request.PhoneNumber, string.Empty).Trim();
                        ResidentAddressPopulation.ResidentDictionary[dictionaryKey].Phone = phoneNumber;
                        }
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].CreateDate = request.CreationDate;
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].CreateUser = request.CreatedBy;
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].UpdateDate = request.LastUpdatedDate;
                    ResidentAddressPopulation.ResidentDictionary[dictionaryKey].UpdateUser = request.LastUpdatedBy;
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(ResidentAddressPopulation.ResidentDictionary[dictionaryKey]);
                    }
                }
            else
                {
                resident = buildResidentFromRequest(request);
                resident.AddressID = addressId;
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(resident);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                resident = WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Resident.Where(a => a.AddressID == addressId).ToList().First();
                ResidentAddressPopulation.ResidentDictionary[dictionaryKey] = resident;
                }
            return resident;
            }
        
        }
    }
