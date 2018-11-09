// -----------------------------------------------------------------------
// <copyright file="MaxLeadTime.cs" company="">
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
    /// 
    [Table("vMaxLeadTimes")]
    public class MaxLeadTime
    {
        [Key]
        [Column(Order=0)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 1)]
        public string Store { get; set; }
        public Int32 LeadTime { get; set; }

    }
}
