using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [NotMapped]
    public class SizeAllocationTotal : SizeAllocation
    {
        [NotMapped]
        public Boolean ModifiedStore { get; set; }
        [NotMapped]
        public Boolean ModifiedDates { get; set; }
    }
}