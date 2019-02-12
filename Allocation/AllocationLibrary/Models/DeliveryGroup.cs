 //-----------------------------------------------------------------------
 //<copyright file = "DeliveryGroup.cs" company="">
 //TODO: Update copyright text.
 //</copyright>
 //-----------------------------------------------------------------------

namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Within a range, group of stores that will be on the same initial PO.
    /// </summary>
    public class DeliveryGroup
    {
        [Key]
        public Int64 ID { get; set; }
        public Int64 PlanID { get; set; }
        public Int64 RuleSetID { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int StoreCount { get; set; }
        public DateTime? MinStart { get; set; }
        public DateTime? MinEnd { get; set; }
        public int? MinEndDays { get; set; }
        public DateTime? ALRStartDate { get; set; }
        [NotMapped]
        public bool Selected{ get; set; }
    }
}
