using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("Vendors")]
    public class Vendors
    {
        [Key, Column("InstanceID", Order = 0)]
        public int InstanceID { get; set; }

        [Key, Column("Vendor", Order = 1)]
        public string VendorCode { get; set; }

        [Column("Description")]
        public string VendorName { get; set; }

        [NotMapped]
        public string VendorDisplay
        {
            get
            {
                return VendorCode + " - " + VendorName ;
            }
        }
    }
}
