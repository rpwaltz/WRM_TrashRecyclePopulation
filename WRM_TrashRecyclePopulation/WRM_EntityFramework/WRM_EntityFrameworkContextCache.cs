using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste;

namespace WRM_TrashRecyclePopulation
    {
    class WRM_EntityFrameworkContextCache
        {
        public static WRM_EntityFramework.WRM_TrashRecycle.WRM_TrashRecycle wrmTrashRecycleContext = null;

        public static void executeSql(string sql)
            {
            WRM_EntityFrameworkContextCache.wrmTrashRecycleContext.Database.ExecuteSqlRaw(sql);
            }
        public static WRM_TrashRecycle WrmTrashRecycleContext
            {
            get
                {
                if (wrmTrashRecycleContext == null)
                    {
                    var connectionString = ConfigurationManager.ConnectionStrings["WRM_TrashRecycleDatabase"].ConnectionString;
                    
                    var contextOptions = new DbContextOptionsBuilder<WRM_TrashRecycle>().UseSqlServer(connectionString).Options;
                    wrmTrashRecycleContext = new WRM_EntityFramework.WRM_TrashRecycle.WRM_TrashRecycle(contextOptions);
                    }
                return wrmTrashRecycleContext;
                }
            set => wrmTrashRecycleContext = value;
            }

        private static WRM_EntityFramework.SolidWaste.SolidWaste solidWasteContext = null;

        public static WRM_EntityFramework.SolidWaste.SolidWaste SolidWasteContext
            {
            get
                {
                if (solidWasteContext == null)
                    {
                    var connectionString = ConfigurationManager.ConnectionStrings["SolidWasteDatabase"].ConnectionString;
                    DbContextOptions<SolidWaste> contextOptions = new DbContextOptionsBuilder<SolidWaste>().UseSqlServer(connectionString).Options;

                    solidWasteContext = new WRM_EntityFramework.SolidWaste.SolidWaste(contextOptions);
                    }
                return solidWasteContext;
                }
            set => solidWasteContext = value;
            }


        }
    }