// -----------------------------------------------------------------------
// <copyright file="RuleSelectedStore.cs" company="">
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
    public class RuleSelectedStore
    {
        [Key]
        [Column(Order=0)]
        public Int64 RuleSetID { get; set; }
        [Key]
        [Column(Order = 1)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 2)]
        public string Store { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }

        public StoreLookup StoreLookup { get; set; }
    }
}
