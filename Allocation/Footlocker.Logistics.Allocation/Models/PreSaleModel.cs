using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class PreSaleModel
    {
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        public string SKU { get; set; }
        [StringLength(50, ErrorMessage = "Max length 50 characters")]
        public string SKUDescription { get; set; }
        public PreSaleSKU preSaleSKU { get; set; }
    }
}