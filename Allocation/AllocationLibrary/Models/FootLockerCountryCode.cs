using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("FootLockerCountryCodes")]
    public class FootLockerCountryCode
    {
        [Column("FLCountryCode")]
        [Key]
        public string CountryCode { get; set; }
        public string ISOCurrencyCode { get; set; }
    }
}
