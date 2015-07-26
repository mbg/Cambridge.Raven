using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Cambridge.Raven
{
    public class RavenController : Controller
    {
        private RavenAuth auth;

        protected virtual new RavenPrincipal User
        {
            get
            {
                return HttpContext.User as RavenPrincipal;
            }
        }

        public RavenController()
        {
            this.auth = new RavenAuth();
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            this.auth.Authorize(filterContext);
        }
    }
}
