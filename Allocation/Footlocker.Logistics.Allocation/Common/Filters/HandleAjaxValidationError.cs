using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Net;

namespace Footlocker.Logistics.Allocation.Common
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class HandleAjaxValidationErrorAttribute : ActionFilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            // Ensure the request is via AJAX, and the Exception is a validation exception
            if (filterContext.ExceptionHandled
                || !filterContext.HttpContext.Request.IsAjaxRequest()
                || !typeof(AjaxValidationException).IsAssignableFrom(filterContext.Exception.GetType())) { return; }

            // Set the Action Result to our JSON
            filterContext.Result = ValidationError(filterContext.Exception.Message, filterContext);

            // Let the system know that the exception has been handled                
            filterContext.ExceptionHandled = true;
        }

        protected JsonResult ValidationError(string message, ExceptionContext filterContext)
        {
            // Set the response status code to 400 - Bad Request                
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            // Needed for IIS7.0                
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

            // Return JSON to caller detailing the validation error
            return new JsonResult
            {
                Data = (filterContext.Exception as AjaxValidationException).ToResult(),
                ContentEncoding = System.Text.Encoding.UTF8,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}