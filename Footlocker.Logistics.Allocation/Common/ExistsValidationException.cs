using System;

namespace Footlocker.Logistics.Allocation.Common
{
    public class ExistsValidationException : AjaxValidationException
    {
        #region Initializations

        public ExistsValidationException(string nonExistingResourceName, string targetResource)
            : base()
        {
            NonExistingResourceName = nonExistingResourceName;
            TargetResource = targetResource;
        }

        #endregion

        #region Public Properties

        public string NonExistingResourceName { get; set; }

        public string TargetResource { get; set; }

        #endregion

        #region Non-Public Properties

        protected override string DisplayMessage
        {
            get
            {
                return String.Format("At least one {0} must exist to create a {1}.", NonExistingResourceName, TargetResource);
            }
        }

        #endregion
    }
}
