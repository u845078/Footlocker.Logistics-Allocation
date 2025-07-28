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
        
        public string Division { get; set; }

        [NotMapped]
        public string SKU { get; set; }

        public long ItemID { get; set; }

        [RegularExpression(@"^\d{3}$", ErrorMessage = "Size must be in the format ###")]
        [Required]
        public string Size { get; set; }

        [RegularExpression(@"^\d{5}$", ErrorMessage = "Store must be in the format #####")]
        [Required]
        public string Store { get; set; }

        public decimal BaseDemand {  get; set; }

        [NotMapped]
        public string BaseDemandString { get; set; }

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
        public DateTime? ExtractDate 
        { 
            get
            {
                if (!ExtractedInd)
                    return null;
                else
                    return LastModifiedDate;
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
