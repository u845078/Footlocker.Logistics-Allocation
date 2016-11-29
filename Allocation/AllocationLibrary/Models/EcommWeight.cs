// -----------------------------------------------------------------------
// <copyright file="EcommWeight.cs" company="">
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
    public class EcommWeight
    {
        public string Division { get; set; }
        public string Store { get; set; }
        public string FOB { get; set; }
        public decimal Weight { get; set; }
    }
}
