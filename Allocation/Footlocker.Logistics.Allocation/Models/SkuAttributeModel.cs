using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Footlocker.Common;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SkuAttributeModel
    {
        public int HeaderID { get; set; }

        public SelectList DivisionList { get; set; }

        public SelectList DepartmentList { get; set; }
        public SelectList CategoryList { get; set; }
        public SelectList BrandList { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Category { get; set; }
        public string BrandID { get; set; }
        public DateTime? UpdateDate { get; set; }
        public List<SkuAttributeDetail> Attributes { get; set; }
        public string Message { get; set; }
        [Display(Name="Weight for Active Skus (1 - 100)")]
        public int WeightActive { get; set; }
    }
}