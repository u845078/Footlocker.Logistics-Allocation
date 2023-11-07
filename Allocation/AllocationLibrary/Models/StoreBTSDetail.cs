using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreBTSDetail
    {
        public int GroupID { get; set; }

        [Key]
        [Required]
        [Column(Order=0)]
        public string Division { get; set; }

        [Key]
        [RegularExpression(@"^\d{5}$",ErrorMessage="Store number must be in the format #####")]
        [Required]
        [Column(Order = 1)]
        public string Store { get; set; }

        [Key]
        [Required]
        [Column(Order = 2)]
        public int Year { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }

        [NotMapped]
        public string errorMessage { get; set; }
    }
}
