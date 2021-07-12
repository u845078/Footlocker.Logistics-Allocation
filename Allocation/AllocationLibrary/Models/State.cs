using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("StateCodes")]
    public class State
    {
        [Key, Column("CountryCode", Order = 0)]
        public string CountryCode { get; set; }

        [Key, Column("StateCode", Order = 1)]
        [Display(Name = "State/Province")]
        public string Code { get; set; }

        [Column("StateName")]
        public string Name { get; set; }

        [ForeignKey("CountryCode")]
        public virtual Country Country { get; set; }

        [NotMapped]
        [Display(Name = "State/Province")]
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
