using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class CrossDockExclusionModel
    {
        public List<Division> Divisions { get; set; }
        public CrossDockExclusion Exclusion { get; set; }
        public string ErrorMessage { get; set; }
    }
}