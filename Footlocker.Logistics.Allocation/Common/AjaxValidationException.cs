using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Common
{
    public class AjaxValidationException : Exception
    {
        #region Fields

        private JsonResultData _resultData = null;

        #endregion

        #region Initializations

        public AjaxValidationException(string errorURL) 
            : this() 
        {
            ErrorURL = errorURL;
        }

        public AjaxValidationException() : base() { }

        #endregion

        #region Non-Public Properties

        protected virtual ActionResultCode ErrorCode { get { return ActionResultCode.ValidationError; } }

        protected virtual string DisplayMessage { get { return "A validation error has occurred. Please correct the issue before proceeding."; } }

        #endregion

        #region Public Properties

        public string ErrorURL { get; set; }

        #endregion

        #region Public Methods

        public virtual JsonResultData ToResult()
        {
            return new JsonResultData(ErrorCode, DisplayMessage, ErrorURL);
        }

        #endregion
    }
}
