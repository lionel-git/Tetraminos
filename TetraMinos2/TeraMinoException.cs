using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraMinos2
{
    public class TetraMinoException : Exception
    {
        public TetraMinoException()
        {
        }

        public TetraMinoException(string message)
            : base(message)
        {
        }

        public TetraMinoException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
