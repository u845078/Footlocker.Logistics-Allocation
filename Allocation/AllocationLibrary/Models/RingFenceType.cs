using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceType
    {
        [Key]
        public int ID { get; set; }
        public string Description { get; set; }
    }
}
