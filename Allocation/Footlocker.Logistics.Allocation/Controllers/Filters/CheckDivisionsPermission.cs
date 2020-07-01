using Footlocker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Web.Mvc
{
    public class CheckDivisionsPermission : ActionFilterAttribute
    {
        public string DivCodes { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool ok = false;

            string username = System.Web.HttpContext.Current.User.Identity.Name.ToLower().Replace("corp\\", "");

            if (!string.IsNullOrEmpty(DivCodes))
            {
                string[] divisions = DivCodes.Split(',');

                foreach (string div in divisions)
                {
                    if (WebSecurityService.UserHasDivision(username, "Allocation", div))
                    {
                        ok = true;
                    }
                }
            }

            if (!ok && !string.IsNullOrEmpty(DivCodes))
            {
                string message = "You need access to one of the following divisions to access this page: " + DivCodes;
                filterContext.HttpContext.Response.Redirect("~/Error/GenericallyDenied?message=" + message);
            }
        }
    }
}