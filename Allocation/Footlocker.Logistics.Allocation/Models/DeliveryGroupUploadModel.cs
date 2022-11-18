using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DeliveryGroupUploadModel
    {
        public string SKU { get; set; }
        public string DeliveryGroupName { get; set; }

        public DeliveryGroup DeliveryGroup { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? MinEndDays { get; set; }

        public long RangePlanID { get; set; }

        public string ErrorMessage { get; set; }

        public string Division
        {
            get
            {
                if (!string.IsNullOrEmpty(SKU))
                    return SKU.Substring(0, 2);
                else
                    return string.Empty;
            }
        }

        public string Department
        {
            get
            {
                if (!string.IsNullOrEmpty(SKU))
                    return SKU.Substring(3, 2);
                else
                    return string.Empty;
            }
        }

        public void ApplyValues()
        {
            if (StartDate.HasValue)
                DeliveryGroup.StartDate = StartDate.Value;

            if (EndDate.HasValue)
                DeliveryGroup.EndDate = EndDate.Value;

            if (MinEndDays.HasValue)
                DeliveryGroup.MinEndDays = MinEndDays.Value;
        }
    }
}