using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("Brands")]
    public class BrandIDs
    {
        [Key, Column("InstanceID", Order = 0)]
        public int instanceID { get; set; }

        [Key, Column("Div", Order = 1)]
        public string divisionCode { get; set; }

        [Key, Column("Dept", Order = 2)]
        public string departmentCode { get; set; }

        [Key, Column("Brand", Order = 3)]
        public string brandIDCode { get; set; }

        [Column("Description")]
        public string brandIDName { get; set; }

        [NotMapped]
        public string brandIDDisplay
        {
            get
            {
                return brandIDCode + " - " + brandIDName;
            }
        }

        #region override comparisons
        public override bool Equals(object obj)
        {
            if (!(obj is BrandIDs b))            
                return false;            
            else
            {
                if (string.IsNullOrEmpty(departmentCode))
                {
                    return b.divisionCode == divisionCode &&
                           b.brandIDCode == brandIDCode &&
                           b.brandIDName == brandIDName;
                }
                else
                {
                    return b.divisionCode == divisionCode &&
                           b.departmentCode == departmentCode &&
                           b.brandIDCode == brandIDCode &&
                           b.brandIDName == brandIDName;
                }

            }
        }

        public override int GetHashCode()
        {
            string departmentCode = string.IsNullOrEmpty(this.departmentCode) ? "" : this.departmentCode;
            return (divisionCode + departmentCode + brandIDCode + brandIDName).GetHashCode();
        }
        #endregion
    }
}
