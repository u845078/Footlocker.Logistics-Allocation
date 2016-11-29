using System;
using System.Collections.Generic;

namespace Footlocker.Logistics.Allocation.Models
{
    public class FOB
    {
        public int ID { get; set; }
        public string Division { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal DefaultCost { get; set; }

        public ICollection<FOBDept> Departments { get; set; }
        public ICollection<FOBPack> Packs { get; set; }
    }
}
