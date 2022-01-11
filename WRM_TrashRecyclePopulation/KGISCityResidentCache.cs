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
        static private IEnumerable<KGISAddress> orderedKgisCityResidentAddressList = null;
        //static public KgisCityResidentAddress KGISCityResidentCache = null;

        public static IEnumerable<KGISAddress> getKGISCityResidentCache()
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
            static private List<KGISAddress> kgisCityResidentAddressList = null;
            public KgisCityResidentAddress()
                {
                }
            public IEnumerable<KGISAddress> retrieveKgisCityResidentAddressList()
                {

                if (kgisCityResidentAddressList == null)
                    {
                    kgisCityResidentAddressList = new List<KGISAddress>();
                    WRM_TrashRecycle context = WRM_EntityFrameworkContextCache.WrmTrashRecycleContext;

                    foreach (KGISAddress KGISAddress in context.KGISAddress.ToList())
                        {
                        kgisCityResidentAddressList.Add(Clone(KGISAddress));
                        }

                    orderedKgisCityResidentAddressList = kgisCityResidentAddressList.OrderBy(kgisCityResidentAddressList => kgisCityResidentAddressList.STREET_NAME).ThenBy(kgisCityResidentAddressList => kgisCityResidentAddressList.ADDRESS_NUM).ThenBy(kgisCityResidentAddressList => kgisCityResidentAddressList.UNIT);
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
