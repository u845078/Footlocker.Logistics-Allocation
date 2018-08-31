using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreBTSDetailModel
    {
        public List<StoreLookup> details { get; set; }
        public List<Division> divisions { get; set; }
        public List<StoreBTS> CopyFrom { get; set; }
        public string division { get; set; }
        public StoreBTS header { get; set; }

        public List<StoreLookup> UnassignedStores { get; set; }

    }
}