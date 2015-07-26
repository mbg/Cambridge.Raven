using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Configuration;
using System.Web;

namespace Cambridge.Raven
{
    public class RavenAuthorizeAttribute : AuthorizeAttribute
    {
        private RavenAuth auth;

        public RavenAuthorizeAttribute()
        {
            this.auth = new RavenAuth();
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            this.auth.Authorize(filterContext);
        }
    }
}
