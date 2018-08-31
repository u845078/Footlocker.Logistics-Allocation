using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Common
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]        
    public class HandleAjaxErrorAttribute : ActionFilterAttribute, IExceptionFilter        
    {            
        public void OnException(ExceptionContext filterContext)            
        {
            if (filterContext.ExceptionHandled
                || !filterContext.HttpContext.Request.IsAjaxRequest()) { return; }
            
            // Set the Action Result to our JSON
            filterContext.Result = AjaxError(filterContext.Exception.Message, filterContext);                                     
            
            // Let the system know that the exception has been handled                
            filterContext.ExceptionHandled = true;            
        }                        
        
        protected JsonResult AjaxError(string displayMessage, ExceptionContext filterContext)            
        { 
            // NOTE: Overwriting exception message here by design, showing friendly error message
            // COMMENT OUT: to return actual exception messages to error dialogs on client
            displayMessage = "A system error has occurred.  Please contact your administrator. ";

            // Set the response status code to 500                
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;                    
            
            // Needed for IIS7.0                
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;                     
            
            // Return JSON to caller detailing the system error
            return new JsonResult
            {
                Data = new JsonResultData(ActionResultCode.SystemError, displayMessage),
                ContentEncoding = System.Text.Encoding.UTF8,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }        
    }    
}
