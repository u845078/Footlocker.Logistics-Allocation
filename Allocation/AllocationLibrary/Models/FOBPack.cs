using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class FOBPack
    {
        public int ID { get; set; }
        public int FOBID { get; set; }
        public int Quantity { get; set; }
        public decimal Cost { get; set; }

        [ForeignKey("FOBID")]
        public FOB FOB { get; set; }

        public ICollection<FOBPackOverride> Overrides { get; set; }
    }
}
