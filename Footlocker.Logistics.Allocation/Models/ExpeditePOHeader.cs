using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Models;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class ExpeditePOHeader
    {
        public string Sku { get; set; }
        public string Department { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime OverrideDate { get; set; }

        public string OverrideDateString
        {
            get {
                return OverrideDate.ToString("yyyy-MM-dd");
            }
        }

        public Boolean MultiSku
        {
            get
            {
                if (Sku != null)
                {
                    return Sku.Contains("multi");
                }
                return false;
            }
        }
    }
}