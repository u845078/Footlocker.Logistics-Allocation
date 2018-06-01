using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("POStatusCodes")]
    public class POStatus
    {
        [Key]
        [Column("POStatusCode")]        
        public string Code { get; set; }

        [Column("POStatusDesc")]
        public string Description { get; set; }

        public List<PurchaseOrder> PurchaseOrder { get; set; }
    }
}
