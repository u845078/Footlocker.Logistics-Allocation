namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// DCs associated with a given route
    /// </summary>
    public class RouteDistributionCenter
    {
        [Key]
        [Column(Order = 0)]
        public int RouteID { get; set; }

        [Key]
        [Column(Order = 1)]
        public int DCID { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreateDate { get; set; }

    }

}
