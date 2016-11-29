using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreBTSControl
    {
        [Key]
        public string Division { get; set; }
        public int TY { get; set; }
        public int LY { get; set; }

    }
}
