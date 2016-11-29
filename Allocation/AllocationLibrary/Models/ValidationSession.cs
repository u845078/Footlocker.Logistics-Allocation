using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ValidationSession")]
    public class ValidationSession
    {
        public int ID { get; set; }
        public DateTime RunDate { get; set; }
        public DateTime ControlDate { get; set; }
    }
}
