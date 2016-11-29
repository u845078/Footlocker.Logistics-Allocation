using System;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("StoreExtensions")]
    public class StoreExtension
    {
        [Key, Column(Order = 0)]
        public string Division { get; set; }

        [Key, Column(Order = 1)]
        public string Store { get; set; }

        public int? ConceptTypeID { get; set; }
        public int? CustomerTypeID { get; set; }
        public int? PriorityTypeID { get; set; }
        public int? StrategyTypeID { get; set; }

        public Boolean ExcludeStore { get; set; }
        public DateTime? FirstReceipt { get; set; }

        [ForeignKey("ConceptTypeID")]
        public ConceptType ConceptType { get; set; }

        [ForeignKey("CustomerTypeID")]
        public CustomerType CustomerType { get; set; }

        [ForeignKey("PriorityTypeID")]
        public PriorityType PriorityType { get; set; }

        [ForeignKey("StrategyTypeID")]
        public StrategyType StrategyType { get; set; }

        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
