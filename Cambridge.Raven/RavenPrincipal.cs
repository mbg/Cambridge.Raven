using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Cambridge.Raven
{
    public class RavenPrincipal : IPrincipal
    {
        #region Instance members
        /// <summary>
        /// 
        /// </summary>
        private RavenIdentity identity;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public IIdentity Identity
        {
            get { return this.identity; }
        }
        #endregion

        #region IsInRole
        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public Boolean IsInRole(string role)
        {
            return false;
        }
        #endregion

        public RavenPrincipal(RavenIdentity identity)
        {
            this.identity = identity;
        }
    }
}
