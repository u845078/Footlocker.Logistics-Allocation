using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ItemPack")]
    public class ItemPack
    {
        [Key]
        [Column(Order = 0)]
        public long ID { get; set; }

        [Column(Order = 1)]
        public long ItemID { get; set; }

        [Column(Order = 2)]
        public string Name { get; set; }

        [Column(Order = 3)]
        public int TotalQty { get; set; }

        //[Association("FK_ItemPack_ItemPackDetail", "ID", "PackID")]
        public ICollection<ItemPackDetail> Details { get; set; }
    }
}
