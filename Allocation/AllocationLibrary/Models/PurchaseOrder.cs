using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("POs")]
    public class PurchaseOrder
    {
        [Key]
        [Column(Order=0)]
        public int InstanceID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 2)]
        public string PO { get; set; }

        [Key]
        [Column(Order = 3)]
        public string SKU { get; set; }

        public DateTime DeliveryDate { get; set; }

        [Column("POStatusCode")]
        [ForeignKey("POStatus")]
        public string StatusCode { get; set; }
        
        public virtual POStatus POStatus { get; set; }

        [Column("POStockStatusCode")]
        [ForeignKey("POStockStatus")]
        public string StockStatusCode { get; set; }
        
        public virtual POStockStatus POStockStatus { get; set; }

        [Column("BlanketPOInd")]
        public bool BlanketPOInd { get; set; }
    }
}
