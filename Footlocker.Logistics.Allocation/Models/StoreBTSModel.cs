using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreBTSModel
    {
        public string CurrentDivision { get; set; }
        public int CurrentYear { get; set; }
        public List<int> Years { get; set; }
        public List<Division> Divisions { get; set; }
        public List<StoreBTS> List { get; set; }
        public StoreBTSControl Control { get; set; }
    }
}