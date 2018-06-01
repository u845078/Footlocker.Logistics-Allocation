// -----------------------------------------------------------------------
// <copyright file="EcommInventory.cs" company="">
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
    /// TODO: Update summary.
    /// </summary>
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
        public Int64 ItemID { get; set; }
        [Key]
        [Column(Order = 3)]
        public string Size { get; set; }
        public int Qty { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
