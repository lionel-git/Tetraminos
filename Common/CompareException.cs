using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class CompareException : Exception
    {
        public CompareException(object thisObject, object other) 
            : base($"Invalid compare of Type '{thisObject.GetType()}' against Type '{other.GetType()}'")
        {
        }
    }
}
