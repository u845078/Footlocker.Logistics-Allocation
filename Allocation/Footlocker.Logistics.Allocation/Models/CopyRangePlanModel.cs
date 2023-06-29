using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SizeElement
    {
        public string Size { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CopyRangePlanModel
    {
        [Display(Name = "From SKU")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "Sku must be in the format ##-##-#####-##")]
        public string FromSku {get;set;}

        [Display(Name = "From Description")]
        public string FromDescription { get; set; }

        [Display(Name = "To SKU")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "Sku must be in the format ##-##-#####-##")]
        public string ToSku { get; set; }
        
        [Display(Name = "To Description")]
        public string ToDescription { get; set; }

        public string Message { get; set; }
        public string PlanType { get; set; }

        [Display(Name = "Copy Order Planning Request from source SKU?")]
        public bool CopyOPRequest { get; set; }

        public RangePlan FromRangePlan { get; set; }

        public bool HaveSizes { get; set; }
        public IEnumerable<SizeElement> FromSizes { get; set; }
        public IEnumerable<SizeElement> ToSizes { get; set; }
    }
}