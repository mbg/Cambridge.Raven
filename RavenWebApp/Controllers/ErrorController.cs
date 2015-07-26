using Cambridge.Raven;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RavenWebApp.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/

        public ActionResult Raven(RavenStatus id)
        {
            return View(id);
        }

    }
}
