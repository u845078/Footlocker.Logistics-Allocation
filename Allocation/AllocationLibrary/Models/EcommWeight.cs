using System;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EcommWeight
    {
        public string Division { get; set; }
        public string Store { get; set; }
        public string FOB { get; set; }
        public decimal Weight { get; set; }
    }
}
