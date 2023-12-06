using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class Price
    {
        [Key]
        [Column(Order=0)]
        public int InstanceID { get; set; }
        [Key]
        [Column(Order = 1)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 2)]
        public string Store { get; set; }
        [Key]
        [Column(Order = 3)]
        public string Stock { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime BusinessDate { get; set; }
        public decimal ItemCost { get; set; }
    }
}
