// -----------------------------------------------------------------------
// <copyright file="EcommRingFence.cs" company="">
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
    public class EcommRingFence
    {
        public string Sku { get; set; }
        public string Size { get; set; }
        public string PO { get; set; }
        public Int32 Qty { get; set; }
        public string EndDate { get; set; }
        public string Comments { get; set; }
    }
}
