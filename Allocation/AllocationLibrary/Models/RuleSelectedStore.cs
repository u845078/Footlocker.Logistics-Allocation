using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RuleSelectedStore
    {
        [Key]
        [Column(Order=0)]
        public long RuleSetID { get; set; }
        [Key]
        [Column(Order = 1)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 2)]
        public string Store { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }

        public StoreLookup StoreLookup { get; set; }
    }
}
