using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class FOBDept
    {
        public int ID { get; set; }
        public int FOBID { get; set; }
        public string Department { get; set; }

        [ForeignKey("FOBID")]
        public FOB FOB { get; set; }
    }
}
