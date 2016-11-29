using System;

namespace Footlocker.Logistics.Allocation.Common
{
    public class JsonResultData
    {
        #region Initializations

        public JsonResultData(ActionResultCode resultCode, string message, string resultURL)
            : this(resultCode, message)
        {
            ResultURL = resultURL;
        }

        public JsonResultData(ActionResultCode resultCode, string message)
            : this(resultCode)
        {
            Message = message;
        }

        public JsonResultData(ActionResultCode resultCode)
            : this()
        {
            ResultCode = resultCode;
        }

        public JsonResultData() { }

        #endregion

        #region Public Properties

        public ActionResultCode ResultCode { get; set; }

        public string Message { get; set; }

        public string ResultURL { get; set; }

        public object Data { get; set; }

        #endregion
    }
}
