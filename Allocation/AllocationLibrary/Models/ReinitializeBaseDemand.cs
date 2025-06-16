using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ReinitializeBaseDemand")]
    public class ReinitializeBaseDemand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ReinitializeBaseDemandID { get; set; }

        [Required]
        public string Division { get; set; }
        
        [Required]
        public long ItemID { get; set; }

        [NotMapped]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        [Display(Name = "SKU")]
        public string SKU { get; set; }

        [NotMapped]
        public string SKUDescription { get; set; }

        public string Size { get; set; }

        public string Store { get; set; }

        public float BaseDemand {  get; set; }

        public bool ExtractedInd {  get; set; }

        [NotMapped]
        public string RecordStatus 
        {
            get 
            {
                if (ExtractedInd)
                    return "Extracted";
                else
                    return "Pending";
            }
        }

        [NotMapped]
        public string ErrorMessage { get; set; }

        public DateTime CreateDateTime { get; set; }
        public string CreateUser { get; set; }

        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedUser { get; set; }
    }
}
