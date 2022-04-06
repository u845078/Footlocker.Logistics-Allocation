using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vInventoryReductionsByType")]
    public class InventoryReductionsByType
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

        [Column("RingFenceQty")]
        public int RingFenceQuantity { get; set; }

        [Column("RDQQty")]
        public int RDQQuantity { get; set; }

        [Column("OrderQty")]
        public int OrderQuantity { get; set; }
    }
}
