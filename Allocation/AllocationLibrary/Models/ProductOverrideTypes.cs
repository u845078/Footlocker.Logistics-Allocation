using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ProductOverrideTypes")]
    public class ProductOverrideTypes
    {
        [Key]
        public string productOverrideTypeCode { get; set; }

        public string productOverrideTypeDesc { get; set; }

        [NotMapped]
        public int sortValue
        {
            get
            {
                switch (productOverrideTypeCode)
                {
                    case "DEPT":
                        return 0;                        
                    case "CAT":
                        return 1;
                    case "LC_BRANDID":
                        return 2;
                    case "SKU":
                        return 3;
                    default:
                        return 10;
                }
            }
        }

        public virtual ICollection<ProductHierarchyOverrides> productHierarchyOverrides { get; set; }
    }
}
