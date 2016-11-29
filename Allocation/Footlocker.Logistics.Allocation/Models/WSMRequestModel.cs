using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class WSMRequestModel
    {
        public WSMRequestModel()
        {
        }

        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        public string Sku { get; set; }

        public int Instance { get; set; }
        public List<Instance> Instances { get; set; } 
    }
}