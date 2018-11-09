using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceSizeModel
    {
        public RingFenceSizeModel() { }

        public RingFenceSizeModel(RingFence rf)
        {
            this.RingFence = rf;
        }

        public RingFence RingFence { get; set; }

        public string Size { get; set; }

        public List<Division> Divisions { get; set; }
        public List<RingFenceSizeSummary> Details { get; set; }
    }
}