using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class LostSalesRequestModel
    {
        [Required(ErrorMessage = "SKU is a required field")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        public String Sku { get; set; }

        // create error message property since we are staying on the same page after each call.
        public String ErrorMessage { get; set; }
    }

}
