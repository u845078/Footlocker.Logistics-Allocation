// -----------------------------------------------------------------------
// <copyright file="InventorySummary.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class InventorySummary
    {
        public long ItemID { get; set; }
        public string Sku { get; set; }
        public string Size { get; set; }
        public int Qty { get; set; }
    }
}
