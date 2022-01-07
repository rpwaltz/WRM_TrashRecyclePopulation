using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation
    {
    public class KGISCityResidentCache
        {
        static private IEnumerable<Kgisaddress> orderedKgisCityResidentAddressList = null;
        //static public KgisCityResidentAddress KGISCityResidentCache = null;

        public static IEnumerable<Kgisaddress> getKGISCityResidentCache()
            {
            if (orderedKgisCityResidentAddressList == null)
                {
                KgisCityResidentAddress KGISCityResidentCache = new KgisCityResidentAddress();
                orderedKgisCityResidentAddressList = KGISCityResidentCache.retrieveKgisCityResidentAddressList();
                }
            return orderedKgisCityResidentAddressList;
            }

        private class KgisCityResidentAddress
            {
            static private List<Kgisaddress> kgisCityResidentAddressList = null;
            public KgisCityResidentAddress()
                {
                }
            public IEnumerable<Kgisaddress> retrieveKgisCityResidentAddressList()
                {

                if (kgisCityResidentAddressList == null)
                    {
                    kgisCityResidentAddressList = new List<Kgisaddress>();
                    WRM_TrashRecycle context = WRM_EntityFrameworkContextCache.WrmTrashRecycleContext;

                    foreach (Kgisaddress kgisAddress in context.Kgisaddress.ToList())
                        {
                        kgisCityResidentAddressList.Add(Clone(kgisAddress));
                        }

                    orderedKgisCityResidentAddressList = kgisCityResidentAddressList.OrderBy(kgisCityResidentAddressList => kgisCityResidentAddressList.StreetName).ThenBy(kgisCityResidentAddressList => kgisCityResidentAddressList.AddressNum).ThenBy(kgisCityResidentAddressList => kgisCityResidentAddressList.Unit);
                    }
                return orderedKgisCityResidentAddressList;
                }
            public static T Clone<T>(T source)
                {
                var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(source);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(serialized);
                }
            }
        }
    }
