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

        private SolidWaste solidWasteContext;
        private WRM_TrashRecycle wrmTrashRecycleContext;

        private static Dictionary<string,Address> addressDictionary = new Dictionary<string,Address>();

        public SolidWaste SolidWasteContext { get => solidWasteContext; set => solidWasteContext = value; }
        public WRM_TrashRecycle WrmTrashRecycleContext { get => wrmTrashRecycleContext; set => wrmTrashRecycleContext = value; }
        public static Dictionary<string, Address> AddressDictionary { get => addressDictionary; set => addressDictionary = value; }

        public AddressPopulation(SolidWaste solidWasteContext, WRM_TrashRecycle wrmTrashRecycleContext)
            {
            this.solidWasteContext = solidWasteContext;
            this.wrmTrashRecycleContext = wrmTrashRecycleContext;

            ServiceTrashDayImporter serviceTrashDayImporter = ServiceTrashDayImporter.getServiceTrashDayImporter();
            this.serviceTrashDayDictionary = serviceTrashDayImporter.addressDictionary;
            }


        static public string translateAddressTypeFromKGISAddressUse(KgisResidentAddressView kgisCityResidentAddress)
            {
            string addressType = "";
            switch (kgisCityResidentAddress.AddressUseType)
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
                    WRMLogger.LogBuilder.Append(kgisCityResidentAddress.AddressUseType + " ");
                    break;
                }
            if (String.IsNullOrEmpty(addressType))
                {
                throw new NotSupportedException("Cannot recognize Property Type " + kgisCityResidentAddress.AddressUseType);
                }
            return addressType;
            }

        public Address buildRequestAddress (dynamic request, KgisResidentAddressView kgisCityResidentAddress)
            {
            Address address = new Address();
            address.AddressType = translateAddressTypeFromKGISAddressUse(kgisCityResidentAddress);
            address.GisparcelId = kgisCityResidentAddress.Parcelid;

            address.StreetName = IdentifierProvider.normalizeStreetName(kgisCityResidentAddress.StreetName);
            address.StreetNumber = Convert.ToInt32(kgisCityResidentAddress.AddressNum);
            address.ZipCode = kgisCityResidentAddress.ZipCode.ToString();
            address.GisaddressUseType = kgisCityResidentAddress.AddressUseType;

            address.Gislatitude = kgisCityResidentAddress.Latitude;
            address.Gislongitude = kgisCityResidentAddress.Longitude;
            address.GispointX = kgisCityResidentAddress.PointX;
            address.GispointY = kgisCityResidentAddress.PointY;

            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);

            if (serviceTrashDayDictionary.ContainsKey(dictionaryKey))
                {

                Address serviceDayAddress = serviceTrashDayDictionary[dictionaryKey];
                if (serviceDayAddress.RecyclingPickup ?? false)
                    {
                    address.RecycleDayOfWeek = serviceDayAddress.RecycleDayOfWeek;
                    address.RecycleFrequency = serviceDayAddress.RecycleFrequency;
                    }

                if (address.TrashPickup ?? false)
                    {
                    address.TrashPickup = serviceDayAddress.TrashPickup;
                    address.TrashDayOfWeek = serviceDayAddress.TrashDayOfWeek;
                    }

                }
            else
                {
                // log that the address does not have a service day
                }

            address.Comment = request.Comments;
            address.CreateDate = request.CreationDate;
            address.CreateUser = request.CreatedBy;
            address.UpdateDate = request.LastUpdatedDate;
            address.UpdateUser = request.LastUpdatedBy;
            return address;

            }
        public Address saveAddress(Address address)
            {
            Address foundAddress;
            string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(address.StreetName, address.StreetNumber, address.UnitNumber, address.ZipCode);
            WRMLogger.LogBuilder.AppendLine("addressdictionaryKey " + dictionaryKey);
            if (AddressDictionary.TryGetValue(dictionaryKey, out foundAddress))
                {
                address = foundAddress;
                WRMLogger.LogBuilder.AppendLine(address.StreetName + address.StreetNumber + address.UnitNumber + address.ZipCode  + "=" + address.AddressId);
                }
            else
                { 
                WrmTrashRecycleContext.Add(address);
                // logBuilder.AppendLine("Add " + request.StreetNumber + "  " + request.StreetName);
                WrmTrashRecycleContext.SaveChanges();
                WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                AddressDictionary.Add(dictionaryKey,address);
                }

            return address;
            }
        }
    }
