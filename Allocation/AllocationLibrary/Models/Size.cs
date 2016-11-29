// -----------------------------------------------------------------------
// <copyright file="Size.cs" company="">
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
    [Table("Sizes")]
    public class SizeObj
    {
        [Key]
        [Column(Order = 0)]
        public int InstanceID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Sku { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Size { get; set; }
    }
}
