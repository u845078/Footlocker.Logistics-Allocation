using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EditMandatoryCrossdock
    {
        public MandatoryCrossdock MandatoryCrossdock;

        public List<MandatoryCrossdockDefault> Defaults;

        [Display(Name = "Sku (##-##-#####-##)")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        public string Sku { get; set; }

        public string Message { get; set; }
    }
}