using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cambridge.Raven
{
    public class RavenResponseException : RavenException
    {
        public RavenResponseException(String message)
            : base("Invalid response received: " + message)
        {
        }
    }
}
