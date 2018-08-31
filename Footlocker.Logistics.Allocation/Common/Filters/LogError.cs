using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Footlocker.Common.Utilities;

namespace Footlocker.Logistics.Allocation.Common
{
    public class LogErrorAttribute : ActionFilterAttribute, IExceptionFilter
    {
        #region Fields

        private LogService _logger = null;

        #endregion

        #region Non-Public Properties
        
        private LogService Logger
        {
            get
            {
                if (_logger == null)
                {
                    //_logger = new LogService(ConfigurationManager.AppSettings["FullyQualifiedLoggingPath"]);
                    _logger = new LogService("C:\\log\\Allocation.log.txt");
                }

                return _logger;
            }
        }

        #endregion

        #region Public Methods

        public void OnException(ExceptionContext filterContext)
        {
            // Not logging handled exceptions, as we are using exceptions for validation and currently dont want to log the validation errors
            if (filterContext.ExceptionHandled) { return; }

            // Log the exception (at this time only logs messages, digs out messages of inner exceptions)
            Logger.Log(filterContext.Exception);

            // Log the exception's stack trace
            // TODO: Incorporate stack trace into base LogService Log call for an exception?
            Logger.Log(filterContext.Exception.ToString());
        }

        #endregion
    }
}