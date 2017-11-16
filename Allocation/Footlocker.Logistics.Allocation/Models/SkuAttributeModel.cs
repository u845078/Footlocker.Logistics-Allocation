using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SkuAttributeModel
    {
        public int HeaderID { get; set; }
        public List<Division> Divisions { get; set; }
        public List<Department> Departments { get; set; }
        public List<Categories> Categories { get; set; }
        public List<BrandIDs> Brands { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Category { get; set; }
        public string BrandID { get; set; }
        public DateTime? UpdateDate { get; set; }
        public List<SkuAttributeDetail> Attributes { get; set; }
        public string Message { get; set; }
        [Display(Name="Weight for Active Skus")]
        public Int32 WeightActive { get; set; }
    }
}