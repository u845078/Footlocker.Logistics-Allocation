using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Footlocker.Logistics.Allocation.Models
{
    [Table("CountryCodes")]
    public class Country
    {
        [Key]
        [Column("CountryCode")]
        [Display(Name = "Country")]
        public string Code { get; set; }
        
        [Column("CountryName")]
        public string Name { get; set; }

        [NotMapped]
        [Display(Name = "Country")]
        public string DisplayName
        {  
            get
            {
                return string.Format("{0} - {1}", Code, Name);
            }
        }
        public DateTime LastModifiedDate { get; set; }

        public string LastModifiedUser { get; set; }
    }
}
