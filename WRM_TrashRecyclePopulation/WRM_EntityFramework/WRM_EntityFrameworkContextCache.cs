using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;

namespace WRM_TrashRecyclePopulation
    {
    class WRM_EntityFrameworkContextCache
        {
        private static WRM_EntityFramework.WRM_TrashRecycle.WRM_TrashRecycle wrmTrashRecycleContext = null;

        public static WRM_TrashRecycle WrmTrashRecycleContext
            {
            get
                {
                if (wrmTrashRecycleContext == null)
                    {
                    wrmTrashRecycleContext = new WRM_EntityFramework.WRM_TrashRecycle.WRM_TrashRecycle();
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
                    solidWasteContext = new WRM_EntityFramework.SolidWaste.SolidWaste();
                    }
                return solidWasteContext;
                }
            set => solidWasteContext = value;
            }


        }
    }