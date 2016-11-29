// -----------------------------------------------------------------------
// <copyright file="RingFenceType.cs" company="">
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
    public class RingFenceType
    {
        [Key]
        public Int32 ID { get; set; }
        public string Description { get; set; }
    }
}
