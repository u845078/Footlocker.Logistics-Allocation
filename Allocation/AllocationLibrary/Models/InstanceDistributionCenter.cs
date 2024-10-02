using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class InstanceDistributionCenter
    {
        [Key]
        [Column(Order=0)]
        public int InstanceID { get; set; }

        [Key]
        [Column(Order = 1)]
        [ForeignKey("DistributionCenter")]
        public int DCID { get; set; }

        public virtual DistributionCenter DistributionCenter { get; set; }
    }
}
