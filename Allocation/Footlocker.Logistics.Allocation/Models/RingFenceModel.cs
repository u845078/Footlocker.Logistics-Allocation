using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Validation;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceModel
    {
        public RingFenceModel() { }

        //public RingFenceModel(RingFence rf)
        //{
        //    this.RingFence = rf;
        //}

        public RingFence RingFence { get; set; }

        public List<Division> Divisions { get; set; }
        public List<RingFenceDetail> WarehouseAvailable { get; set; }
        public List<RingFenceDetail> FutureAvailable { get; set; }
    }
}