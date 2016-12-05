using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    /// <summary>
    /// WSM is a calculated value internal to Quantum.
    /// the purpose of this object is to represent the results of a query Q gave us so users could
    /// interrogate this information (it's not on their UI).
    /// </summary>
    public class WSM
    {
        public String RunDate { get; set; }
        public String TargetProduct { get; set; }
        public String TargetProductId { get; set; }
        public String TargetLocation { get; set; }
        public String MatchProduct { get; set; }
        public String MatchProductId { get; set; }
        public String ProductWeight { get; set; }
        public String MatchLocation { get; set; }
        public String LocationWeight { get; set; }
        public String FinalMatchWeight { get; set; }
        public String FinalMatchDemand { get; set; }
        public String LastCapturedDemand { get; set; }
        public String StatusCode { get; set; }
    }
}
