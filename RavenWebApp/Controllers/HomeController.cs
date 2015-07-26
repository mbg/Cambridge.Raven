using Cambridge.Raven;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RavenWebApp.Controllers
{
    public class HomeController : RavenController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

    }
}
