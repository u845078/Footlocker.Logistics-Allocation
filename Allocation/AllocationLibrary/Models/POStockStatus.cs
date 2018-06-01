using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("POStockStatusCodes")]
    public class POStockStatus
    {
        [Key]
        [Column("POStockStatusCode")]
        public string Code { get; set; }

        [Column("POStockStatusDesc")]
        public string Description { get; set; }

        public List<PurchaseOrder> PurchaseOrder { get; set; }
    }
}
