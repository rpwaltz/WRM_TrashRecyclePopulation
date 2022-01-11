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
        static private Dictionary<string, Resident> residentDictionary = new Dictionary<string, Resident>();
        static private IEnumerable<KGISAddress> kgisCityResidentAddressList = WRM_TrashRecycleQueries.retrieveKgisCityResidentAddressList();


        public static Dictionary<string, Resident> ResidentDictionary { get => residentDictionary; set => residentDictionary = value; }

        abstract public void buildAndSaveTrashRecycleEntitiesFromRequest(dynamic request, IEnumerator<KGISAddress> foundKgisResidentAddressEnumerator, int numberOfUnits);


        static private Regex reducePhoneNumber = new Regex("[^\\d]");

        public Resident buildRequestResident(dynamic request)
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
        public void populateResidentAddressFromRequest(dynamic request)
            {
            if (String.IsNullOrEmpty(request.StreetName))
                {
                return;
                }
            request.StreetName = request.StreetName.ToUpper().Trim();

            IEnumerable<KGISAddress> foundKgisResidentAddress =
                from req in kgisCityResidentAddressList
                where Decimal.ToInt32(req.ADDRESS_NUM ?? 0) == request.StreetNumber && req.STREET_NAME.Equals(request.StreetName) && req.JURISDICTION == 1 && req.ADDRESS_STATUS == 2
                select req;

            IEnumerator<KGISAddress> foundKgisResidentAddressEnumerator = foundKgisResidentAddress.GetEnumerator();


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
                        WRMLogger.LogBuilder.AppendLine(" ADDRESS DOES NOT EXIST FOR [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "]");
                        }

                    break;
                case 1:
                    try
                        {
                        countFoundAddresses = 0;
                        buildAndSaveTrashRecycleEntitiesFromRequest(request, foundKgisResidentAddressEnumerator, countFoundAddresses);


                        }
                    catch (Exception ex)
                        {
                        WRMLogger.LogBuilder.AppendLine(" ADDRESS FAILED [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "]");
                        WRMLogger.LogBuilder.AppendFormat("Exception:{0} : {1} : {2} : {3}{4}", ex.HResult, ex.Message, ex.TargetSite, ex.HelpLink, Environment.NewLine);
                        WRMLogger.LogBuilder.AppendLine(ex.StackTrace);

                        Exception inner = ex.InnerException;
                        if (inner != null)
                            {
                            WRMLogger.LogBuilder.AppendLine("Inner Exception");
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
                        buildAndSaveTrashRecycleEntitiesFromRequest(request, foundKgisResidentAddressEnumerator, countFoundAddresses);


                        }
                    catch (Exception ex)
                        {
                        WRMLogger.LogBuilder.AppendLine(ex.Message + "ADDRESS WITH UNIT IS NOT A VALID TYPE [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "]");
                        WRMLogger.LogBuilder.AppendLine(" ADDRESS FAILED [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "]");
                        WRMLogger.LogBuilder.AppendFormat("Exception:{0} : {1} : {2} : {3}{4}", ex.HResult, ex.Message, ex.TargetSite, ex.HelpLink, Environment.NewLine);
                        WRMLogger.LogBuilder.AppendLine(ex.StackTrace);

                        Exception inner = ex.InnerException;
                        if (inner != null)
                            {
                            WRMLogger.LogBuilder.AppendLine("Inner Exception");
                            WRMLogger.LogBuilder.AppendLine(inner.StackTrace);
                            }
                        }
                    break;

                default:
                    WRMLogger.LogBuilder.AppendLine("MORE THAN FOUR UNITS [" + request.StreetNumber + "] [" + request.StreetNamePrefix + "] [" + request.StreetName + "] [" + request.StreetNameSuffix + "] [" + request.StreetSuffixDirection + "] [" + request.UnitNumber + "] has more than 4  units!");
                    break;
                }

            }

        }
    }
