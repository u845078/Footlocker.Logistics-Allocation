// -----------------------------------------------------------------------
// <copyright file="MandatoryCrossdockDefault.cs" company="">
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
    public class MandatoryCrossdockDefault
    {
        [Key]
        [Column(Order = 0)]
        public int InstanceID { get; set; }

        [Key]
        [Column(Order = 1)]
        public Int64 ItemID { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 3)]
        public string Store { get; set; }

        public decimal Percent { get; set; }

        [NotMapped]
        public int PercentAsInt 
        {
            get {
                return Convert.ToInt32(Percent * 100);
            }

            set
            {
                Percent = Convert.ToDecimal(value) / Convert.ToDecimal(100.0);
            }
        }
    }
}
