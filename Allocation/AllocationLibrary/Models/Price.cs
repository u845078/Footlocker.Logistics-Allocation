// -----------------------------------------------------------------------
// <copyright file="Price.cs" company="">
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
    public class Price
    {
        [Key]
        [Column(Order=0)]
        public Int32 InstanceID { get; set; }
        [Key]
        [Column(Order = 1)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 2)]
        public string Store { get; set; }
        [Key]
        [Column(Order = 3)]
        public string Stock { get; set; }
        public Decimal RetailPrice { get; set; }
        public Decimal SalePrice { get; set; }
        public DateTime BusinessDate { get; set; }
        public Decimal ItemCost { get; set; }

    }
}
