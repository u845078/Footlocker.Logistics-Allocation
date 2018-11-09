using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class QuantumDataDownloadModel
    {
        [Required(ErrorMessage = "SKU is a required field")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        public String Sku { get; set; }
    }
}
