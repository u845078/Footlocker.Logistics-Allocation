using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;

namespace Footlocker.Logistics.Allocation.Controllers
{
    /// <summary>
    /// Checks if current session expired.
    /// </summary>
    public class CheckSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // check if session is supported
            if (filterContext.HttpContext.Session != null)
            {
                // check if a new session id was generated
                if (filterContext.HttpContext.Session.IsNewSession)
                {
                    // If it says it is a new session, but an existing cookie exists, then it must
                    // have timed out
                    string sessionCookie = filterContext.HttpContext.Request.Headers["Cookie"];
                    if ((null != sessionCookie) && (sessionCookie.IndexOf("ASP.NET_SessionId") >= 0))
                    {
                        filterContext.HttpContext.Response.Redirect("~/Error/SessionExpired");
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}