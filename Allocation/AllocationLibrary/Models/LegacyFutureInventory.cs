// -----------------------------------------------------------------------
// <copyright file="LegacyInventory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Future Inventory from input file from legacy (mainframe) system.
    /// </summary>
    [Table("LegacyFutureInventory")]
    public class LegacyFutureInventory
    {
        [Key]
        [Column(Order = 0)]
        public int InstanceID { get; set; }
        [Key]
        [Column(Order = 1)]
        public string InventoryType { get; set; }
        [Key]
        [Column(Order = 2)]
        public string InventoryID { get; set; }
        [Key]
        [Column(Order = 3)]
        public string ProductNodeType { get; set; }
        [Key]
        [Column(Order = 4)]
        public string ProductNodeID { get; set; }
        [Key]
        [Column(Order = 5)]
        public string LocNodeType { get; set; }
        [Key]
        [Column(Order = 6)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 7)]
        public string Store { get; set; }
        public string AvailableDate { get; set; }
        public string MultiPrecedence { get; set; }
        public string LifeCycle { get; set; }

        public int StockQty { get; set; }
        public string Sku { get; set; }
        public string Size { get; set; }

        [NotMapped]
        public string PO
        { 
            get
            {
                if (InventoryType == "PO")
                    return InventoryID.Split('-')[0];
                else
                    return string.Empty;
            } 
        }
    }
}
