using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class AuditRDQModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Message { get; set; }

        public List<AuditRDQ> list { get; set; }

    }
}