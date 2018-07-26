using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using Footlocker.Common;

namespace System.Web.Mvc
{
    /// <summary>
    /// Checks custom user permission setting in WebSecurity
    /// </summary>
    public class CheckPermissionAttribute : ActionFilterAttribute
    {
        public string Roles { get; set; }

        
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool ok = false;

            if ("TRUE" == System.Configuration.ConfigurationManager.AppSettings["lockdown"])
            {
                string username = System.Web.HttpContext.Current.User.Identity.Name.ToLower().Replace("corp\\", "");
                string[] roles = Roles.Split(new char[] { ',' });
                ok = WebSecurityService.UserHasRole(username, "Allocation", "IT");

                if (!ok)
                {
                    //user doesn't have any of the specified roles enabled in websecurity
                    filterContext.HttpContext.Response.Redirect("~/Error/PermissionDenied?roles=" + Roles);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Roles))
                {
                    string username = System.Web.HttpContext.Current.User.Identity.Name.ToLower().Replace("corp\\", "");
                    string[] roles = Roles.Split(new char[] { ',' });
                    ok = WebSecurityService.UserHasRole(username, "Allocation", roles);
                }

                if (!ok)
                {
                    //user doesn't have any of the specified roles enabled in websecurity
                    filterContext.HttpContext.Response.Redirect("~/Error/PermissionDenied?roles=" + Roles);
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}