using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Footlocker.Logistics.Allocation.Models
{
    public class ControlDate
    {
        [Key]
        public int InstanceID { get; set; }

        public DateTime RunDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime PickDate { get; set; }
    }
}
