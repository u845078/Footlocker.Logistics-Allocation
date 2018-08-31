using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class VendorGroup
    {
        public int ID { get; set; }
        [NotMapped]
        public string Name 
        {
            get
            {
                return "VG:" + ID;
            }
            set { }
        }
        [Column("Name")]
        public string Comment {get;set;}
        public int Count { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
