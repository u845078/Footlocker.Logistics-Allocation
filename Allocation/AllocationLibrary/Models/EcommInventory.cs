using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("EcommInventory")]
    public class EcommInventory
    {
        [Key]
        [Column(Order = 0)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 1)]
        public string Store { get; set; }
        [Key]
        [Column(Order = 2)]
        public long ItemID { get; set; }
        [Key]
        [Column(Order = 3)]
        public string Size { get; set; }
        public int Qty { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
