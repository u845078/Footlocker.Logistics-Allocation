using System;

namespace Footlocker.Logistics.Allocation.Models
{
    public class CrossDockExclusionViewModel
    {
        public CrossDockExclusionViewModel() { }

        public string Division { get; set; }
        public string Store { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
    }
}