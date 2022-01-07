using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste.Models;

using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;

/*
 * 
 * 
 * 
 * 
 * and vaa.address_use_type in (
''DWELLING, MULTI-FAMILY'',
''DWELLING, SINGLE-FAMILY'',
''DWELLING, TWO-FAMILY'',
''DWELLING, TOWNHOUSE'',
''GROUP HOME'',
''MOBILE HOME'',
''DWELLING, APT UNIT'')
'
 * 
 * 
 * 
 * 
 * 
 */
namespace WRM_TrashRecyclePopulation
{
    class WRM_TrashRecycleQueries
    {
        static private List<RecyclingRequest> solidWasteRecyclingRequestList = null;
        static private List<BackdoorServiceRequest> solidWasteBackdoorRequestList = null;

        static private IEnumerable<BackdoorServiceRequest> orderedSolidWasteBackdoorRequestList = null;
        static private IEnumerable<RecyclingRequest> orderedRecyclingRequestList = null;

        static private IEnumerable<Kgisaddress> kgisCityResidentAddressList = KGISCityResidentCache.getKGISCityResidentCache();
        public static IEnumerable<Kgisaddress> retrieveKgisCityResidentAddressList()
            {
            return KGISCityResidentCache.getKGISCityResidentCache();
            }

        public static IEnumerable<BackdoorServiceRequest> retrieveBackdoorRequestList()
            {

            if (solidWasteBackdoorRequestList == null)
                {
                solidWasteBackdoorRequestList = WRM_EntityFrameworkContextCache.SolidWasteContext.BackdoorServiceRequest.ToList();

                List<String> requestStatuses = new List<String>() { "PAY FOR SERVICE", "MEDICAL NEED/OVER 75", "REQUESTED" };

                orderedSolidWasteBackdoorRequestList = solidWasteBackdoorRequestList.Where(swrrl => requestStatuses.Contains(swrrl.Status)).OrderBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.StreetName).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.StreetNumber).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.UnitNumber).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.BackdoorId);
                }
            return orderedSolidWasteBackdoorRequestList;
            }

        public static IEnumerable<RecyclingRequest> retrieveRecyclingRequestList()
            {

            if (solidWasteRecyclingRequestList == null)
                {
                solidWasteRecyclingRequestList = WRM_EntityFrameworkContextCache.SolidWasteContext.RecyclingRequest.ToList();

                List<String> requestStatuses = new List<String>() { "DISAPPROVED", "WITHDRAWN", "APPROVED", "REQUESTED" };

                orderedRecyclingRequestList = solidWasteRecyclingRequestList.Where(swrrl => requestStatuses.Contains(swrrl.Status)).OrderBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.StreetName).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.StreetNumber).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.UnitNumber).ThenBy(solidWasteBackdoorRequestList => solidWasteBackdoorRequestList.Id);
                }
            return orderedRecyclingRequestList;
            }
        public static List<Kgisaddress> findKGISAddressList(Address address)
            {
            decimal? addressNumber = address.StreetNumber;
            String streetName = address.StreetName.ToUpper().Trim();

            int zipCode = int.Parse(address.ZipCode);

            IEnumerable<Kgisaddress> findKgisResidentAddress =
                from kgisAddress in kgisCityResidentAddressList
                where Decimal.ToInt32(kgisAddress.AddressNum ?? 0) == addressNumber
                   && kgisAddress.StreetName.Equals(streetName)
                   && kgisAddress.ZipCode == zipCode
                   && kgisAddress.Jurisdiction == 1
                   && kgisAddress.AddressStatus == 2
                select kgisAddress;
            return findKgisResidentAddress.ToList();

            }
        public static Boolean verifyKGISAddress(Address address)
        {

            Boolean verified = false;

            List<Kgisaddress> myTableResults = findKGISAddressList(address);
            if (myTableResults.Count > 0)
                {
                verified = true;
                }

            return verified;
        }
        public static Boolean verifyKGISApartmentUnitTotal(Address address)
            {

            Boolean verified = false;
            
            List<Kgisaddress> myTableResults = findKGISAddressList(address);
            if (myTableResults.Count < 5)
                {
                verified = true;
                }
            return verified;
            }

        public static Boolean determineAddressFailure(dynamic request)
            {

            Boolean verified = false;

            WRMLogger.LogBuilder.Append("determineAddressFailure ");
            int addressNumber = request.StreetNumber ?? 0;
            String streetName = request.StreetName.ToUpper();


            IEnumerable<Kgisaddress> findKgisResidentAddress =
                from kgisAddress in kgisCityResidentAddressList
                where Decimal.ToInt32(kgisAddress.AddressNum ?? 0) == addressNumber
                   && kgisAddress.StreetName.Equals(streetName)
                select kgisAddress;
            List<Kgisaddress> myTableResults = findKgisResidentAddress.ToList();

            
            if (myTableResults.Count > 0)
                {
 
                foreach (Kgisaddress allKgisAddress in myTableResults) 
                    {
                    if (allKgisAddress.Jurisdiction != 1)
                        {
                        WRMLogger.LogBuilder.Append("OUTSIDE KNOXVILLE: ");
                        }
                    if (allKgisAddress.AddressStatus != 2)
                        {
                        WRMLogger.LogBuilder.Append("INVALID STATUS FOR: ");
                        }
                    if (allKgisAddress.Jurisdiction == 1 && allKgisAddress.AddressStatus == 2)
                        {
                        verified = true;
                        WRMLogger.LogBuilder.Append("CONFUSED STATUS FOR: ");
                        }
                    }
                }
            return verified;
            }
        }
}
