using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("RingFenceStatusCodes")]
    public class RingFenceStatusCodes
    {
        [Key]
        public string ringFenceStatusCode { get; set; }

        public string ringFenceStatusDesc { get; set; }

        public virtual List<RingFenceDetail> RingFenceDetails { get; set; }
    }
}
