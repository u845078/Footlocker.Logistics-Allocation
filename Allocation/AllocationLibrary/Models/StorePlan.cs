using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StorePlan
    {
        [Key]
        [Column(Order = 0)]
        public string InstanceID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string PlanName { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 3)]
        public string Store { get; set; }

        public string Description { get; set; }

    }
}
