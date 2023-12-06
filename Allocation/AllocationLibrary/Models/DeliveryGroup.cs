using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    /// <summary>
    /// Within a range, group of stores that will be on the same initial PO.
    /// </summary>
    public class DeliveryGroup
    {
        [Key]
        public long ID { get; set; }
        public long PlanID { get; set; }
        public long RuleSetID { get; set; }
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
