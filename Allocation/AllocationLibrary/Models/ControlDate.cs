// -----------------------------------------------------------------------
// <copyright file="ControlDate.cs" company="">
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
    public class ControlDate
    {
        [Key]
        public Int32 InstanceID { get; set; }

        public DateTime RunDate { get; set; }
    }
}
