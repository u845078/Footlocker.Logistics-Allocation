using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class MassEditRingFenceModel
    {

        public string Div { get; set; }
        public string Store { get; set; }
        public string Sku { get; set; }
        public string FOB { get; set; }
        public string Comment { get; set; }
        public DateTime? EndDate { get; set; }

    }
}