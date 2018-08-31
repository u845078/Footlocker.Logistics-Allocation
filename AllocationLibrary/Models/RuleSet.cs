using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RuleSet
    {
        [Key]
        public Int64 RuleSetID { get; set; }

        public string Type { get; set; }
        public Int64? PlanID { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }

        public string Division { get; set; }

        public ICollection<RuleSelectedStore> Stores { get; set; }
    }
}