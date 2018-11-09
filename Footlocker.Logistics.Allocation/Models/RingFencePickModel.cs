using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFencePickModel
    {
        public List<Division> Divisions { get; set; }

        [RegularExpression(@"^\d{2}$", ErrorMessage = "Division must be in the format ##")]
        public string Division { get; set; }

        [RegularExpression(@"^\d{5}$", ErrorMessage = "Store number must be in the format #####")]
        public string Store { get; set; }

        public RingFence RingFence { get; set; }
        public List<RingFenceDetail> Details { get; set; }
        public string Message;
    }
}