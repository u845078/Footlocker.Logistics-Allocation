// -----------------------------------------------------------------------
// <copyright file="RingFenceSizeSummary.cs" company="">
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

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RingFenceSizeSummary
    {
        public string Size { get; set; }
        [Display(Name="Total Units")]
        public int TotalQty { get; set; }
        [Display(Name = "Future Units")]
        public int FutureQty { get; set; }
        [Display(Name = "Caselot Units")]
        public int CaselotQty { get; set; }
        [Display(Name = "Bin Units")]
        public int BinQty { get; set; }

    }
}
