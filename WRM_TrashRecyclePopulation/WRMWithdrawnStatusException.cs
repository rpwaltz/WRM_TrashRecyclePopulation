using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRM_TrashRecyclePopulation
    {

    public class WRMWithdrawnStatusException : Exception
        {
        public WRMWithdrawnStatusException()
            {
            }
        public WRMWithdrawnStatusException(string message)
            : base(message)
            {
            }

        public WRMWithdrawnStatusException(string message, Exception inner)
            : base(message, inner)
            {
            }
        }
    }
