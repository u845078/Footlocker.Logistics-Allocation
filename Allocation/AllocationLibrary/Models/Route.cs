using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class Route
    {
        public int ID { get; set; }
        [Required]
        [Display(Name="Route")]
        public string Name { get; set; }
        public string Perspective { get; set; }
        public string Pass { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int InstanceID { get; set; }
        public string Code { get; set; }

        [NotMapped]
        public string DisplayString
        {
            get {
                return Name + " " + Perspective + " " + Pass;
            }
            set { }
        }
    }
}
