using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ItemPackDetail")]
    public class ItemPackDetail
    {
        [Key]
        [Column("ID", Order = 0)]
        public long ID { get; set; }

        [Column("PackID", Order = 1)]
        public long PackID { get; set; }

        [Column(Order = 2)]
        public string Size { get; set; }

        [Column(Order = 3)]
        public int Quantity { get; set; }

        [ForeignKey("PackID")]
        //public ItemPack Pack { get; set; }
        internal ItemPack Pack { get; set; }

        [NotMapped]
        public int packAmount { get; set; }

        [NotMapped]
        public int ComputedQuantity {
            get
            {
                return Quantity * packAmount;
            }
        }
    }
}
