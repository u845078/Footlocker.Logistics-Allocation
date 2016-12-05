using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class QuantumSeasonalityData
    {
        public string locationFinalNodeID { get; set; }
        public DateTime weekBeginDate { get; set; }
        public float indexValue { get; set; }
    }
}
