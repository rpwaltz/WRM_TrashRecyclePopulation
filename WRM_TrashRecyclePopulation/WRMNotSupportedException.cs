using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRM_TrashRecyclePopulation
    {
    public class WRMNotSupportedException : Exception
        {
        public WRMNotSupportedException()
            {
            }
        public WRMNotSupportedException(string message)
            : base(message)
            {
            }

        public WRMNotSupportedException(string message, Exception inner)
            : base(message, inner)
            {
            }
        }
    }
