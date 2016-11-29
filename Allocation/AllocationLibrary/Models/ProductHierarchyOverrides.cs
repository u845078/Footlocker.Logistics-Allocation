using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ProductHierarchyOverrides")]
    public class ProductHierarchyOverrides
    {
        [Key]
        public long productHierarchyOverrideID { get; set; }
        public string productOverrideTypeCode { get; set; }

        [NotMapped]
        public ProductOverrideTypes productOverrideType { get; set; }
        public DateTime effectiveFromDt { get; set; }
        public DateTime? effectiveToDt { get; set; }
        public string overrideDivision { get; set; }
        public string overrideDepartment { get; set; }
        public string overrideCategory { get; set; }
        public string overrideBrandID { get; set; }
        public string overrideSKU { get; set; }
        public long? overrideItemID { get; set; }

        public string newDivision { get; set; }
        public string newDepartment { get; set; }
        public string newCategory { get; set; }
        public string newBrandID { get; set; }
        public string displayOverrideValue { get; set; }
        public string displayNewValue { get; set; }
        public DateTime lastModifiedDate { get; set; }
        public string lastModifiedUser { get; set; }

        [NotMapped]
        public string lastModifiedUserName { get; set; }
    }
}
