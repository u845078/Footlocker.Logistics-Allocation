using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Web.Mvc
{
    /// <summary>
    /// Checks if user is authorized to access application
    /// </summary>
    public class CheckAuthorizationAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Handle an unauthorized request.
        /// </summary>
        /// <param name="filterContext">The current authorization context.</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Error/AccessDenied");
        }
    }
}