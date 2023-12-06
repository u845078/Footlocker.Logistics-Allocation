using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    /// <summary>
    /// Stores that cannot use cross dock.
    /// Typically, a Turkey store in Europe, because they have to go through customs.
    /// </summary>
    public class CrossDockExclusion
    {
        [Key]
        [Column(Order=0)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Store { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual StoreLookup StoreLookup { get; set; }
    }
}
