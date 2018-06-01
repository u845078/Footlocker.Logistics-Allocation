namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Store lead time from a given distribution center
    /// used to create route details
    /// </summary>
    public class StoreLeadTime
    {
        [Key]
        [Column(Order=0)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Store { get; set; }

        [Key]
        [Column(Order = 2)]
        public int DCID { get; set; }

        public int LeadTime { get; set; }

        public int Rank { get; set; }

        public Boolean Active { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreateDate { get; set; }

        [NotMapped]
        public string Warehouse { get; set; }
    }
}
