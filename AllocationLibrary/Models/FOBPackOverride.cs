using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class FOBPackOverride
    {
        public int ID { get; set; }
        public int FOBPackID { get; set; }
        public int FOBDeptID { get; set; }
        public decimal Cost { get; set; }

        [ForeignKey("FOBPackID")]
        public FOBPack FOBPack { get; set; }

        [ForeignKey("FOBDeptID")]
        public FOBDept FOBDept { get; set; }
    }
}
