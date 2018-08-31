using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class TroubleshootStoreModel
    {
        [Required]
        public string Division { get; set; }
        [Required]
        public string Store { get; set; }

        public List<Division> Divisions { get; set; }

        public StoreLookup StoreLookup { get; set; }
        public Boolean isValid { get; set; }
        public StoreExtension StoreExtension { get; set; }
        public NetworkZone Zone { get; set; }
        public StoreSeasonality StoreSeasonality { get; set; }

        public string Message { get; set; }
        //public List<StoreBTS> BTSGroups { get; set; }
        //public List<StoreAttribute> LikeStores { get; set; }
        //public List<StoreLeadTime> StoreLeadTimes { get; set; }
        //public List<Hold> Holds { get; set; }
        //public List<RingFence> RingFences { get; set; }
        //public List<RDQ> RDQs { get; set; }
    }
}