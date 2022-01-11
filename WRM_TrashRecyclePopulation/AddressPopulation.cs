using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation
    {
    class AddressPopulation
        {


        public Dictionary<string, Address> serviceTrashDayDictionary;


        private static Dictionary<string,Address> addressDictionary = new Dictionary<string,Address>();

        public static Dictionary<string, Address> AddressDictionary { get => addressDictionary; set => addressDictionary = value; }

        public AddressPopulation()
            {


            ServiceTrashDayImporter serviceTrashDayImporter = ServiceTrashDayImporter.getServiceTrashDayImporter();
            this.serviceTrashDayDictionary = serviceTrashDayImporter.addressDictionary;
            }


        static public string translateAddressTypeFromKGISAddressUse(KGISAddress kgisCityResidentAddress)
            {
            string addressType = "";
            switch (kgisCityResidentAddress.ADDRESS_USE_TYPE)
                {
                case "DWELLING, MULTI-FAMILY":
                case "ACCESSORY DWELLING UNIT":
                case "PRIMARY BUILDING ADDRESS":
                case "DWELLING, SINGLE-FAMILY":
                case "DWELLING, TWO-FAMILY":
                case "DWELLING, TOWNHOUSE":
                case "DWELLING, APT UNIT":
                    addressType = "RESIDENTIAL";
                    break;

                case "COMMUNITY CENTER":
                case "PUBLIC PARK":
                case "FIRE STATION":
                case "POLICE STATION":
                case "GREENWAY":
                case "RECREATIONAL FACILITY, PUBLIC":
                    addressType = "SPECIALTY";
                    break;

                case "RECREATIONAL FACILITY, PRIVATE":

                case "PARKING LOT/STRUCTURE":
                case "CEMETERY":
                case "BUSINESS":
                case "RESIDENTIAL CARE FACILITY":
                case "GOVERNMENT OFFICE/FACILITY":
                case "LODGE/MEETING HALL":
                case "PLACE OF WORSHIP":
                case "GROUP HOME":
                    addressType = "COMMERCIAL";
                    break;

                default:
                    WRMLogger.LogBuilder.Append(kgisCityResidentAddress.ADDRESS_USE_TYPE + " ");
                    break;
                }
            if (String.IsNullOrEmpty(addressType))
                {
                throw new NotSupportedException("Property Type may not be null");
                }
            return addressType;
            }

        public Address buildRequestAddress (dynamic request, KGISAddress kgisCityResidentAddress)
            {
            Address address = new Address();

            address.StreetName = IdentifierProvider.normalizeStreetName(kgisCityResidentAddress.STREET_NAME);
            if (String.IsNullOrEmpty(address.StreetName))
                {
                throw new NullReferenceException("Street Name may not be null.");
                }

            address.StreetNumber = Convert.ToInt32(kgisCityResidentAddress.ADDRESS_NUM);
            if (address.StreetNumber == 0)
                {
                throw new NullReferenceException("Street Number may not be null.");
                }
            address.ZipCode = kgisCityResidentAddress.ZIP_CODE.ToString();
            address.UnitNumber = null;
            if (request.UnitNumber != null)
                address.UnitNumber = request.UnitNumber;
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);

            if (serviceTrashDayDictionary.ContainsKey(dictionaryKey))
                {
                address = null;
                if (!serviceTrashDayDictionary.TryGetValue(dictionaryKey, out address))
                    {
                    throw new Exception("Address Dictionary contains key " + dictionaryKey + ", but unable to retreive address");
                    }
                }
            address.AddressType = translateAddressTypeFromKGISAddressUse(kgisCityResidentAddress);
            address.GISParcelID = kgisCityResidentAddress.PARCELID;
            address.GISAddressUseType = kgisCityResidentAddress.ADDRESS_USE_TYPE;

            address.GISLatitude = kgisCityResidentAddress.LATITUDE;
            address.GISLongitude = kgisCityResidentAddress.LONGITUDE;
            address.GISPointX = kgisCityResidentAddress.POINT_X;
            address.GISPointY = kgisCityResidentAddress.POINT_Y;
            address.Comment = request.Comments;
            address.CreateDate = request.CreationDate;
            address.CreateUser = request.CreatedBy;
            address.UpdateDate = request.LastUpdatedDate;
            address.UpdateUser = request.LastUpdatedBy;
            return address;

            }
        }
    }
