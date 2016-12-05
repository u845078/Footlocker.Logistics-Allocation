using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class ProductHierarchyOverrideModel
    {
        public ProductHierarchyOverrides prodHierarchyOverride { get; set; }
        public List<SelectListItem> overrideTypes { get; set; }
        public List<SelectListItem> overrideDivisionList { get; set; }
        public List<SelectListItem> overrideDepartmentList { get; set; }
        public List<SelectListItem> overrideCategoryList { get; set; }
        public List<SelectListItem> overrideBrandIDList { get; set; }

        public string overrideTypeLabel { get; set; }

        [DisplayFormat(NullDisplayText = "** No value **")]
        public string overrideDivisionLabel { get; set; }

        [DisplayFormat(NullDisplayText = "** No value **")]
        public string overrideDepartmentLabel { get; set; }

        [DisplayFormat(NullDisplayText = "** No value **")]
        public string overrideCategoryLabel { get; set; }

        [DisplayFormat(NullDisplayText = "** No value **")]
        public string overrideBrandIDLabel { get; set; }

        public string overrideSKU { get; set; }
        public string overrideSKUDescription { get; set; }

        public List<SelectListItem> newDivisionList { get; set; }
        public List<SelectListItem> newDepartmentList { get; set; }
        public List<SelectListItem> newCategoryList { get; set; }
        public List<SelectListItem> newBrandIDList { get; set; }

        public ProductHierarchyOverrideModel()
        {
            prodHierarchyOverride = new ProductHierarchyOverrides();
        }

    }
}