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
        static private Dictionary<string,Resident> residentDictionary = new Dictionary<string,Resident>();
        static private IEnumerable<Kgisaddress> kgisCityResidentAddressList = WRM_TrashRecycleQueries.retrieveKgisCityResidentAddressList();


        public static Dictionary<string, Resident> ResidentDictionary { get => residentDictionary; set => residentDictionary = value; }

        abstract public void buildAndSaveTrashRecycleEntitiesFromRequest(dynamic request, IEnumerator<Kgisaddress> foundKgisResidentAddressEnumerator);

        abstract public void buildAndSaveTrashRecycleEntitiesFromRequestWithUnits(dynamic request, IEnumerator<Kgisaddress> foundKgisResidentAddressEnumerator);

        static private Regex reducePhoneNumber = new Regex("[^\\d]");
        static private Regex formatPhoneNumber = new Regex(@"(\d{3})(\d{3})(\d{4})");
        public Resident buildRequestResident(dynamic request, int addressId)
            {
            Resident resident = new Resident();
            resident.Email = request.Email;
            resident.FirstName = request.FirstName;
            resident.LastName = request.LastName;
            if (!String.IsNullOrWhiteSpace(resident.Phone) )
                { 
                string phoneNumber = reducePhoneNumber.Replace(request.PhoneNumber, string.Empty).Trim();
                resident.Phone = formatPhoneNumber.Replace(phoneNumber, "($1) $2-$3");
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

  //          string residentDictionaryIdentifier = IdentifierProvider.provideIdentifierFromResident(resident.FirstName, resident.LastName, resident.Phone, resident.Email);

            string residentDictionaryKey = resident.AddressId.ToString();

            Resident foundResident;
            if (residentDictionary.TryGetValue(residentDictionaryKey, out foundResident))
                {
                if ( ((resident.UpdateDate ?? Program.posixEpoche) > (foundResident.UpdateDate ?? Program.posixEpoche))  )
                    {
                    foundResident.FirstName = resident.FirstName;
                    foundResident.LastName = resident.LastName;
                    foundResident.Email = resident.Email;
                    foundResident.Note = resident.Note;
                    foundResident.Phone = resident.Phone;
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(foundResident);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    }
                else
                    {
                    string logMessage = string.Format("Unable to determine if Resident changed from Name {0} {1} created {2} updated {3} to Name {4} {5} created {6} updated {7}\n", 
                        foundResident.FirstName, foundResident.LastName, foundResident.CreateDate.ToString(), foundResident.UpdateDate.ToString(),
                        resident.FirstName, resident.LastName, resident.CreateDate.ToString(), resident.UpdateDate.ToString());
                    WRMLogger.Logger.logMessageAndDeltaTime(logMessage, ref Program.beforeNow, ref Program.justNow, ref Program.loopMillisecondsPast);
                    }
                resident = foundResident;
                }
            else
                {
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(resident);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
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

            IEnumerable<Kgisaddress> foundKgisResidentAddress =
                from req in kgisCityResidentAddressList
                where Decimal.ToInt32(req.AddressNum ?? 0) == request.StreetNumber && req.StreetName.Equals(request.StreetName) && req.Jurisdiction == 1 && req.AddressStatus == 2
                select req;

           IEnumerator<Kgisaddress> foundKgisResidentAddressEnumerator = foundKgisResidentAddress.GetEnumerator();


            int countFoundAddresses = foundKgisResidentAddress.Count();

            switch (countFoundAddresses)
                {
                case 0:
                    if (WRM_TrashRecycleQueries.determineAddressFailure(request))
                        {
                        WRMLogger.LogBuilder.AppendLine(" FOR ADDRESS [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "]");

                        }
                    else
                        {
                        WRMLogger.LogBuilder.AppendLine(" ADDRESS DOES NOT EXIST FOR [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] ["  + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "]");
                        }

                    break;
                case 1:
                    try
                        {
                        buildAndSaveTrashRecycleEntitiesFromRequest(request, foundKgisResidentAddressEnumerator);


                        }
                    catch (Exception ex)
                        {
                        WRMLogger.LogBuilder.AppendLine(" ADDRESS FAILED [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "]");
                        WRMLogger.LogBuilder.AppendFormat("Exception:{0} : {1} : {2} : {3}{4}", ex.HResult, ex.Message, ex.TargetSite, ex.HelpLink, Environment.NewLine);
                        WRMLogger.LogBuilder.AppendLine(ex.StackTrace);

                        Exception inner = ex.InnerException;
                        if (inner != null)
                            {

                            WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                            }
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
                        WRMLogger.LogBuilder.AppendLine(ex.Message +  "ADDRESS WITH UNIT IS NOT A VALID TYPE [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "]");
                        }
                    break;
                default:
                    WRMLogger.LogBuilder.AppendLine("MORE THAN FOUR UNITS [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "] has more than 4  units!");
                    break;
                }

            }

        }
    }
