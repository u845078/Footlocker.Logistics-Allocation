using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class VendorGroupDetail
    {
        public int GroupID { get; set; }
        
        [Key]
        [RegularExpression(@"^\d{5}$",ErrorMessage="Vendor number must be in the format #####")]
        [Required]
        public string VendorNumber { get; set; }
        
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }

        [NotMapped]
        public string VendorName { get; set; }

        [NotMapped]
        public string ErrorMessage { get; set; }
    }
}
