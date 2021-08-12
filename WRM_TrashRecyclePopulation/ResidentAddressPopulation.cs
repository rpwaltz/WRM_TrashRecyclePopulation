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
        private static Dictionary<string,Resident> residentDictionary = new Dictionary<string,Resident>();
        private WRM_TrashRecycleQueries wrm_TrashRecycleQueries;
        static private IEnumerable<KgisResidentAddressView> kgisCityResidentAddressList;




        public ResidentAddressPopulation(SolidWaste solidWasteContext, WRM_TrashRecycle wrmTrashRecycleContext) : base(solidWasteContext, wrmTrashRecycleContext)
            {
            Wrm_TrashRecycleQueries = new WRM_TrashRecycleQueries(wrmTrashRecycleContext);
            if (KgisCityResidentAddressList == null) 
                { 
                KgisCityResidentAddressList = Wrm_TrashRecycleQueries.retrieveKgisCityResidentAddress();
                }
            }


        public static Dictionary<string, Resident> ResidentDictionary { get => residentDictionary; set => residentDictionary = value; }
        public WRM_TrashRecycleQueries Wrm_TrashRecycleQueries { get => wrm_TrashRecycleQueries; set => wrm_TrashRecycleQueries = value; }
        static public IEnumerable<KgisResidentAddressView> KgisCityResidentAddressList { get => kgisCityResidentAddressList; set => kgisCityResidentAddressList = value; }

        abstract public void buildAndSaveTrashRecycleEntitiesFromRequest(dynamic request, IEnumerator<KgisResidentAddressView> foundKgisResidentAddressEnumerator);

        abstract public void buildAndSaveTrashRecycleEntitiesFromRequestWithUnits(dynamic request, IEnumerator<KgisResidentAddressView> foundKgisResidentAddressEnumerator);


        public Resident buildRequestResident(dynamic request, int addressId)
            {
            Resident resident = new Resident();
            resident.Email = request.Email;
            resident.FirstName = request.FirstName;
            resident.LastName = request.LastName;
            if (!String.IsNullOrWhiteSpace(resident.Phone) ){ 
                string phoneNumber = Regex.Replace(request.PhoneNumber, "[^\\d]", string.Empty).Trim();
                resident.Phone = Regex.Replace(phoneNumber, @"(\d{3})(\d{3})(\d{4})", " ($1) $2-$3");
            }
            resident.AddressId = addressId;
            resident.CreateDate = request.CreationDate;
            resident.CreateUser = request.CreatedBy;
            resident.UpdateDate = request.LastUpdatedDate;
            resident.UpdateUser = request.LastUpdatedBy;
            return resident;
            }
        public Resident saveResident(Resident resident)
            {

            string residentDictionaryIdentifier = IdentifierProvider.provideIdentifierFromResident(resident.FirstName, resident.LastName, resident.Phone, resident.Email);

            string residentDictionaryKey = residentDictionaryIdentifier + "-" + resident.AddressId;
            WRMLogger.LogBuilder.AppendLine("residentDictionaryKey " + residentDictionaryKey);
            Resident foundResident;
            if (residentDictionary.TryGetValue(residentDictionaryKey, out foundResident))
                {
                resident = foundResident;
                }
            else
                {
                WrmTrashRecycleContext.Add(resident);
                // logBuilder.AppendLine("Add " + request.StreetNumber + "  " + request.StreetName);
                WrmTrashRecycleContext.SaveChanges();
                WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                residentDictionary.Add(residentDictionaryKey, resident);
                }
            return resident;
            }

        public void populateResidentAddressFromRequest(dynamic request)
            {
            if (String.IsNullOrEmpty(request.StreetName) )
                {
                return;
                }
            request.StreetName =  request.StreetName.ToUpper().Trim();

            //  IEnumerable<KgisResidentAddressView> foundKgisResidentAddress = kgisCityResidentAddressList.Where(req => req.StreetName.ToUpper().Equals(request.StreetName.ToUpper()));
            IEnumerable<KgisResidentAddressView> foundKgisResidentAddress =
                from req in kgisCityResidentAddressList
                where Decimal.ToInt32(req.AddressNum ?? 0) == request.StreetNumber && req.StreetName.Equals(request.StreetName) && req.Jurisdiction == 1 && req.AddressStatus == 2
                select req;

            IEnumerator<KgisResidentAddressView> foundKgisResidentAddressEnumerator = foundKgisResidentAddress.GetEnumerator();


            int countFoundAddresses = foundKgisResidentAddress.Count();

            switch (countFoundAddresses)
                {
                case 0:
                    if (Wrm_TrashRecycleQueries.determineAddressFailure(request))
                        {
                        WRMLogger.LogBuilder.AppendLine(" FOR ADDRESS [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "] does not exist! \n");

                        }
                    else
                        {
                        WRMLogger.LogBuilder.AppendLine("ADDRESS DOES NOT EXIST [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] ["  + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "] does not exist! \n");
                        }

                    break;
                case 1:
                    try
                        {
                        buildAndSaveTrashRecycleEntitiesFromRequest(request, foundKgisResidentAddressEnumerator);


                        }
                    catch (Exception ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ADDRESS IS NOT A VALID TYPE [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "] does not exist! ");
                        }

                    // request.FirstName; request.LastName, request.PhoneNumber; request.Email;
                    break;
                case 2:
                case 3:
                case 4:
                    try
                        {
                        buildAndSaveTrashRecycleEntitiesFromRequestWithUnits(request, foundKgisResidentAddressEnumerator);
 
                        }
                    catch (Exception ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ADDRESS IS NOT A VALID TYPE[" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "] does not exist! ");
                        }
                    break;
                default:
                    WRMLogger.LogBuilder.AppendLine("MORE THAN FOUR UNITS [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "] has more than 4  units!");
                    break;
                }

            }

        }
    }
