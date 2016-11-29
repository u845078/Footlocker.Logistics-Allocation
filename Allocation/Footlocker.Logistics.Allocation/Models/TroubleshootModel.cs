using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Logistics.Allocation.Models;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class TroubleshootModel
    {
        [Required]
        public string Sku { get; set; }

        public string Division
        {
            get 
            {
                if (string.IsNullOrEmpty(Sku))
                    return "";
                else
                    return Sku.Split('-')[0];
            }
        }
        public string Size { get; set; }
        public List<DistributionCenter> AllDCs { get; set; }
        public Int32 Warehouse { get; set; }
        public string Store { get; set; }

        public List<RangePlan> RangePlans { get; set; }
        //public List<RangePlanDetail> RangePlanDetails { get; set; }
        public List<Hold> Holds { get; set; }
        public List<RingFence> RingFences { get; set; }
        //public List<RingFenceDetail> RingFenceDetails {get;set;}
        public List<ExpeditePO> POOverrides { get; set; }

        public List<SizeObj> Sizes { get; set; }
        public ItemMaster ItemMaster { get; set; }
        public Boolean ValidItem { get; set; }
        public AllocationDriver AllocationDriver { get; set; }
        public ControlDate ControlDate { get; set; }

        public List<RDQ> RDQs { get; set; }
    }
}