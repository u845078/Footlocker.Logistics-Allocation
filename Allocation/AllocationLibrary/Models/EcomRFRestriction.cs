using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("EcomRingFenceRestrictions")]
    public class EcomRFRestriction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long EcomRFRestrictionID { get; set; }
        public long ItemID { get; set; }

        [NotMapped]
        [Required]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        [Display(Name = "SKU")]
        public string SKU { get; set; }

        [NotMapped]
        [Display(Name = "Description")]
        public string SKUDescription { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedUser { get; set; }
    }
}
