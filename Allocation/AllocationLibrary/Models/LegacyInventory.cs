using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    /// <summary>
    /// Inventory from input file from legacy (mainframe) system.
    /// </summary>
    [Table("LegacyInventory")]
    public class LegacyInventory
    {
        [Key]
        [Column(Order = 0)]
        public int InstanceID { get; set; }
        [Key]
        [Column(Order = 1)]
        public string ProductTypeCode { get; set; }
        [Key]
        [Column(Order = 2)]
        public string Sku { get; set; }
        [Key]
        [Column(Order = 3)]
        public string Size { get; set; }
        [Key]
        [Column(Order = 4)]
        public string LocationTypeCode { get; set; }
        [Key]
        [Column(Order = 5)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 6)]
        public string Store { get; set; }
        public string DayDT { get; set; }
        public string OnHandQuantity { get; set; }
        public string NonSellableQty { get; set; }
        public string Qty1 { get; set; }

        [NotMapped]
        [Display(Name="OnHandQuantity")]
        public int OnHandQuantityInt 
        {
            get
            {
                return Convert.ToInt32(OnHandQuantity);
            }
            set { }
        }
    }
}
