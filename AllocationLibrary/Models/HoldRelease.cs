using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class HoldRelease
    {
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Division must be in the format ##")]
        public string Division { get; set; }
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Store number must be in the format #####")]
        public string Store { get; set; }
        public int Qty { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
