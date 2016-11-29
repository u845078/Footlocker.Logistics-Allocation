using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("StoreBTS")]
    public class StoreBTS
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public int Year { get; set; }
        [NotMapped]
        public string DisplayName
        {
            get {
                return Year + " " + Name;
            }
        }
        public int Count { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string Division { get; set; }
    }
}
