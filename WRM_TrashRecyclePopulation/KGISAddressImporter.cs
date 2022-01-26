using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation
    {
    public class KGISAddressImporter
        {
        //static public KgisCityResidentAddress KGISAddressCache = null;
        static object lockMe = new object();
        static Dictionary<string, KGISAddress> kgisAddressCache = null;

        public static Dictionary<string, KGISAddress> getKGISAddressCache()
            {
            if (kgisAddressCache == null)
                {
                lock (lockMe)
                    {
                    kgisAddressCache = new Dictionary<string, KGISAddress>();
                    WRM_TrashRecycle context = WRM_EntityFrameworkContextCache.WrmTrashRecycleContext;
                    int countAddresses = 0;
                    foreach (KGISAddress kgisAddress in context.KGISAddress.ToList())
                        {
                        countAddresses++;
                        string kgisIdentifier = IdentifierProvider.provideIdentifierFromAddress(kgisAddress.STREET_NAME, Convert.ToInt32(kgisAddress.ADDRESS_NUM ?? 0), "", kgisAddress.ZIP_CODE.ToString());
                        KGISAddress foundKGISAddress = new KGISAddress();
                        if (!KGISAddressImporter.kgisAddressCache.TryGetValue(kgisIdentifier, out foundKGISAddress))
                            {
                            KGISAddressImporter.kgisAddressCache.Add(kgisIdentifier, kgisAddress);
                            }
                        }
                    WRMLogger.LogBuilder.AppendLine("Number of Address from KGIS " + countAddresses.ToString());
                    WRMLogger.Logger.log();
                    }
                }
            return KGISAddressImporter.kgisAddressCache;
            }

        }
    }
