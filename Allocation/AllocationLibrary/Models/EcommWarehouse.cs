// -----------------------------------------------------------------------
// <copyright file="EcommWarehouse.cs" company="">
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
    public class EcommWarehouse
    {
        [Key]
        [Column(Order = 0)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 1)]
        public string Store { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
