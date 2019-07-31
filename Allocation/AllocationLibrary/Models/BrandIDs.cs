using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            BrandIDs b = obj as BrandIDs;

            if (b == null)
            {
                return false;
            }
            else
            {
                if (string.IsNullOrEmpty(this.departmentCode))
                {
                    return b.divisionCode == this.divisionCode &&
                           b.brandIDCode == this.brandIDCode &&
                           b.brandIDName == this.brandIDName;
                }
                else
                {
                    return b.divisionCode == this.divisionCode &&
                           b.departmentCode == this.departmentCode &&
                           b.brandIDCode == this.brandIDCode &&
                           b.brandIDName == this.brandIDName;
                }

            }
        }

        public override int GetHashCode()
        {
            string departmentCode = string.IsNullOrEmpty(this.departmentCode) ? "" : this.departmentCode;
            return (this.divisionCode + departmentCode + this.brandIDCode + this.brandIDName).GetHashCode();
        }

        #endregion


    }
}
