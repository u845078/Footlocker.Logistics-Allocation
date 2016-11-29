using System;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("StrategyTypes")]
    public class StrategyType
    {
        [Key]
        [Column("ID", Order = 0)]
        public int ID { get; set; }

        [Column("Name", Order = 1)]
        public string Name { get; set; }
    }
}
