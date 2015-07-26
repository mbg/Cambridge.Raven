using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cambridge.Raven
{
    public class RavenException : Exception
    {
        public RavenException()
        {
        }

        public RavenException(String message)
            : base(message)
        {
        }
    }
}
