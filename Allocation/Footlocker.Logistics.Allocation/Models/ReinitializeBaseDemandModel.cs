using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class ReinitializeBaseDemandModel
    {
        public List<ReinitializeBaseDemand> ReinitBaseDemandList { get; set; }

        public ReinitializeBaseDemand ReinitializeBaseDemand { get; set; }
        public bool ShowAllSKUsInd {  get; set; }
    }
}