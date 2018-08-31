using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Common
{
    public class DeleteValidationException : AjaxValidationException
    {
        #region Initializations

        public DeleteValidationException(string errorURL, string modelTypeName)
            : this(errorURL)
        {
            ModelTypeName = modelTypeName;
        }

        public DeleteValidationException(string errorURL) : base(errorURL) 
        {
            ModelTypeName = "item";
        }

        #endregion

        #region Public Properties

        public string ModelTypeName { get; set; }

        #endregion

        #region Non-Public Properties

        protected override ActionResultCode ErrorCode
        {
            get
            {
                return ActionResultCode.DeleteConstraintError;
            }
        }

        protected override string DisplayMessage
        {
            get
            {
                return String.Format("Unable to delete this {0}, as it is still being referenced at this time. \n\nPlease remove all references to this {0} prior to deleting.", ModelTypeName);
            }
        }

        #endregion
    }
}
