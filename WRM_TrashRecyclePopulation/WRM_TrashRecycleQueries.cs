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
        WRM_TrashRecycle context { get; set; }
        List<KgisResidentAddressView> kgisCityResidentAddressList = null;
        public WRM_TrashRecycleQueries(WRM_TrashRecycle trashRecycleContext)
        {
            context = trashRecycleContext;

        }
        public IEnumerable<KgisResidentAddressView> retrieveKgisCityResidentAddress()
            {
            if (this.kgisCityResidentAddressList == null)
                {
                this.kgisCityResidentAddressList = context.KgisResidentAddressView.ToList();
                }
 
            //           List<String> AddressUseTypes = new List<String>(){ "DWELLING, MULTI-FAMILY", "DWELLING, SINGLE-FAMILY", "DWELLING, TWO-FAMILY", "DWELLING, TOWNHOUSE", "GROUP HOME", "MOBILE HOME", "DWELLING, APT UNIT" };

            //           kgisCityResidentAddressList = context.KgisResidentAddressView.Where(kcra => AddressUseTypes.Contains(kcra.AddressUseType)).ToList();
            
            IEnumerable <KgisResidentAddressView> orderedKgisCityResidentAddressList = this.kgisCityResidentAddressList.OrderBy(kgisCityResidentAddressList => kgisCityResidentAddressList.StreetName).ThenBy(kgisCityResidentAddressList => kgisCityResidentAddressList.AddressNum).ThenBy(kgisCityResidentAddressList => kgisCityResidentAddressList.Unit);

            return orderedKgisCityResidentAddressList;
            }

 
        public Boolean verifyKGISAddress(Address address)
        {
            Boolean verified = false;
            String kgisAddressWhereClause = "";
            try
                {
                kgisAddressWhereClause = formatKGISAddressWhereClause(address);
                }
            catch (Exception ex)
                {
                return verified;
                }
            String Recycling_KGIS_MAP_VWQuery = String.Format("select *  from Kgis_Resident_Address_View  where  {0}", kgisAddressWhereClause);
            WRMLogger.LogBuilder.AppendLine(Recycling_KGIS_MAP_VWQuery);
            List<KgisResidentAddressView> myTableResults = context.KgisResidentAddressView.FromSqlRaw(Recycling_KGIS_MAP_VWQuery).ToList();
            if (myTableResults.Count > 0)
                {
                verified = true;
                }

            return verified;
        }
        public Boolean verifyKGISApartmentUnitTotal(Address address)
            {
            Boolean verified = false;
            String kgisAddressWhereClause = "";
            try
                {
                kgisAddressWhereClause = formatKGISAddressWhereClause(address);
                }
            catch (Exception ex)
                {
                return verified;
                }
            String Recycling_KGIS_MAP_VWQuery = String.Format("select  count(*)  from Kgis_Resident_Address_View where  {0}  group by UNIT_TYPE", kgisAddressWhereClause);
            WRMLogger.LogBuilder.AppendLine(Recycling_KGIS_MAP_VWQuery);
            
            List<KgisResidentAddressView> myTableResults = context.KgisResidentAddressView.FromSqlRaw(Recycling_KGIS_MAP_VWQuery).ToList();
            if (myTableResults.Count < 4)
                {
                verified = true;
                }
            return verified;
            }
        public Boolean verifyAndCorrectKGISApartmentUnitNumber(ref Address address, ref string[] validUnits)
            {
 
            Boolean verified = false;
            String kgisAddressWhereClause = "";
            try
                {
                kgisAddressWhereClause = formatKGISAddressWhereClause(address);
                }
            catch (Exception ex)
                {
                return verified;
                }
            String KgisCityResidentAddressQuery = String.Format("select count(*)  from Kgis_Resident_Address_View where  {0}", kgisAddressWhereClause);

            DbConnection connection = context.Database.GetDbConnection();
            if (connection.State.Equals(ConnectionState.Closed)) { connection.Open(); }
            using (connection)
                {
                using (DbCommand command = connection.CreateCommand())
                    {
                    
                    command.CommandText = KgisCityResidentAddressQuery;

                    DbDataReader dbDataReader = command.ExecuteReader();

                    if (dbDataReader.HasRows)
                        {
                        int count = dbDataReader.GetInt32(1);
                        if (count > 0)
                            {
                            verified = true;
                            }
                        }
                    else
                        {
                        WRMLogger.LogBuilder.AppendLine("No rows found.");
                        }
                    dbDataReader.Close();
                    }
                }
            if (connection.State.Equals(ConnectionState.Open)) { connection.Close(); }

            return verified;
            }

        private string formatKGISAddressWhereClause (Address address)
            {

            if ((address.ZipCode is null || address.StreetName is null ))
                {

                throw new Exception("Address formated Incorrectly");
                }
            int? addressNumber = address.StreetNumber;
            String streetName = address.StreetName.ToUpper();

            int zipCode = int.Parse(address.ZipCode);

            string addressNumberQuery = "ADDRESS_NUM = " + address.StreetNumber;
            string streetNameQuery = "STREET_NAME = '" + address.StreetName + "'";
            string zipCodeQuery = "ZIP_CODE = " + address.ZipCode;


            string  addressWhereClause = String.Format(" {0}  AND {1} and {2}", addressNumberQuery, streetNameQuery, zipCodeQuery);
            return addressWhereClause;
            }
        public Boolean determineAddressFailure(dynamic request)
            {
            Boolean verified = false;
            WRMLogger.LogBuilder.AppendLine("determineAddressFailure");

            String Recycling_KGIS_MAP_VWQuery = String.Format("select *  from Kgis_Resident_Address_View where  ADDRESS_NUM = {0} AND STREET_NAME = '{1}'", request.StreetNumber ?? 0, request.StreetName);
            WRMLogger.LogBuilder.Append("");
            List<KgisResidentAddressView> myTableResults =  context.KgisResidentAddressView.FromSqlRaw(Recycling_KGIS_MAP_VWQuery).ToList();
            
            if (myTableResults.Count > 0)
                {
                WRMLogger.LogBuilder.AppendLine("found a failure");
                WRMLogger.LogBuilder.Append("RESULTS ");
                foreach (KgisResidentAddressView allKgisAddress in myTableResults) 
                    {
                    WRMLogger.LogBuilder.Append("VERIFIED ");
                    if (allKgisAddress.Jurisdiction != 1)
                        {
                        WRMLogger.LogBuilder.Append("OUTSIDE KNOXVILLE:");
                        }
                    if (allKgisAddress.AddressStatus != 2)
                        {
                        WRMLogger.LogBuilder.Append("INVALID STATUS FOR:");
                        }
                    if (allKgisAddress.Jurisdiction == 1 && allKgisAddress.AddressStatus == 2)
                        {
                        verified = true;
                        WRMLogger.LogBuilder.Append("CONFUSED STATUS FOR:");
                        }
                    }
                }
            return verified;
            }

        }
}
