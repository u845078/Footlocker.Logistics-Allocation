using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class PriorityType
    {
        [Key]
        [Column("ID", Order = 0)]
        public int ID { get; set; }

        [Column("Name", Order = 1)]
        public string Name { get; set; }

        public string MFCode { get; set; }
    }
}
