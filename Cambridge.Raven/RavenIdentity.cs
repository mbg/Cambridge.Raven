using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Cambridge.Raven
{
    /// <summary>
    /// Represents a Raven user.
    /// </summary>
    [XmlRoot("RavenIdentity")]
    public class RavenIdentity : IIdentity
    {
        #region Instance members
        private String principal;
        #endregion

        public String AuthenticationType
        {
            get { return "pwd"; }
        }

        public Boolean IsAuthenticated
        {
            get { return true; }
        }

        [XmlElement("Principal")]
        public String Name
        {
            get { return this.principal; }
            set { this.principal = value;  }
        }

        public RavenIdentity()
        {
        }

        public RavenIdentity(String principal)
        {
            this.principal = principal;
        }
    }
}
