using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class ReinitializeBaseDemandModel
    {
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        [Required]
        public string SKU { get; set; }

        public string SKUDescription { get; set; }

        public ReinitializeBaseDemand ReinitializeBaseDemand { get; set; }
    }
}