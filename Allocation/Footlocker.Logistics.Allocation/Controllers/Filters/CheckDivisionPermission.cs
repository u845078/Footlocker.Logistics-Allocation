using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using Footlocker.Common;

namespace System.Web.Mvc
{
    /// <summary>
    /// Checks custom user permission setting in WebSecurity for a role within a division.
    /// Division must be either passed as property value or set in "CurrentDivision" session variable.
    /// RoleName is optional. If is not set, it will just check for Division permission.
    /// </summary>
    public class CheckDivisionPermissionAttribute : ActionFilterAttribute
    {
        public string DivCode { get; set; }
        public string Roles { get; set; }

        /// TODO:  Set "DefaultApplication" to your application name
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool ok = false;

            if (string.IsNullOrEmpty(DivCode) && System.Web.HttpContext.Current.Session["CurrentDivision"] != null)
            {
                //use the "CurrentDivision" session variable
                DivCode = System.Web.HttpContext.Current.Session["CurrentDivision"].ToString();
            }

            if (!string.IsNullOrEmpty(DivCode))
            {
                string username = System.Web.HttpContext.Current.User.Identity.Name.ToLower().Replace("corp\\", "");

                if (string.IsNullOrEmpty(Roles))
                {
                    //no roles - check division permission only
                    ok = WebSecurityService.UserHasDivision(username, "DefaultApplication", DivCode);
                }
                else
                {
                    //check division/role permission
                    string[] roles = Roles.Split(new char[] { ',' });
                    ok = WebSecurityService.UserHasDivisionRole(username, "DefaultApplication", DivCode, roles);
                }
            }

            if (!ok)
            {
                filterContext.HttpContext.Response.Redirect("~/Error/DivisionPermissionDenied?divcode=" + DivCode + "&roles=" + Roles);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}