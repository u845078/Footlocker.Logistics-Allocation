using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DistributionCenter
    {
        public int ID { get; set; }
        [Required]
        [Display(Name="DC")]
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int InstanceID { get; set; }
        public string MFCode { get; set; }
        public string Type { get; set; }

        public string displayValue
        {
            get
            {
                return MFCode + " - " + Name;
            }
        }
    }
}
