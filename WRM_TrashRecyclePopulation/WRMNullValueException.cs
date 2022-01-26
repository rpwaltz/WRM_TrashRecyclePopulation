using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRM_TrashRecyclePopulation
    {
    public class WRMNullValueException : Exception
        {
        public WRMNullValueException()
            {
            }
        public WRMNullValueException(string message)
            : base(message)
            {
            }

        public WRMNullValueException(string message, Exception inner)
            : base(message, inner)
            {
            }
        }
    }
