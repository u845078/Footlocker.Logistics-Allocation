using Footlocker.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceReleaseModel : RuleFilterModel
    {
        public List<Instance> Instances { get; set; }
        public List<Division> Divisions { get; set; }
        public List<Department> Departments { get; set; }
        public List<DistributionCenter> DistributionCenterList { get; set; }
        public List<RingFenceType> RingFenceTypeList { get; set; }

        public int Instance { get; set; }        
        public string Division { get; set; }
        public string Department { get; set; }
        public int DistributionCenter { get; set; }
        public string Store { get; set; }
        public string SKU { get; set; }
        public string PO { get; set; }

        [Display(Name = "Ring Fence Type")]
        public string RingFenceType { get; set; }
        public string ShowStoreSelector { get; set; }

        public bool HaveResults { get; set; }

        public RuleModel RuleModel { get; set; }

        public List<GroupedRingFence> RingFenceResults { get; set; }
    }
}