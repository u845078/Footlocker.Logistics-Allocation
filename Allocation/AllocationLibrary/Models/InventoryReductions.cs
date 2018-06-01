using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vInventoryReductions")]
    public class InventoryReductions
    {
        [Key]
        [Column(Order = 0)]
        public string Sku { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Size { get; set; }

        [Key]
        [Column(Order = 2)]
        public string MFCode { get; set; }

        [Key]
        [Column(Order = 3)]
        public string PO { get; set; }

        public int Qty { get; set; }
        public int InstanceID { get; set; }
    }
}
