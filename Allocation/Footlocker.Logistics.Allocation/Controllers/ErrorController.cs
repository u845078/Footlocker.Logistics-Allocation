using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult AccessDenied()
        {
            return View();
        }

        public ActionResult SessionExpired()
        {
            return View();
        }

        public ActionResult PermissionDenied()
        {
            return View();
        }

        public ActionResult GenericallyDenied(string message)
        {
            ViewBag.Message = message;
            return View();
        }
    }
}
