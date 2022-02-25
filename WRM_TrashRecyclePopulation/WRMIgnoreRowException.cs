using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRM_TrashRecyclePopulation
    {

    public class WRMIgnoreRowException : Exception
        {
        public WRMIgnoreRowException()
            {
            }
        public WRMIgnoreRowException(string message)
            : base(message)
            {
            }

        public WRMIgnoreRowException(string message, Exception inner)
            : base(message, inner)
            {
            }
        }
    }
