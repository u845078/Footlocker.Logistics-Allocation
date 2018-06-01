using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("WarehouseAllocationTypes")]
    public class WarehouseAllocationType
    {
        [Key]
        [Column("WarehouseAllocationType")]
        public int Code { get; set; }

        [Column("WarehouseAllocationTypeDesc")]
        public string Description { get; set; }
    }
}
