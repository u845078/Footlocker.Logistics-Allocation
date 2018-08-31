using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreSeasonalityDetailModel
    {
        public List<ValidStoreLookup> details { get; set; }
        public List<Division> divisions { get; set; }
        public string division { get; set; }

        public List<ValidStoreLookup> UnassignedStores { get; set; }

    }
}