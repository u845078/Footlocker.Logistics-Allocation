using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class ProductType
    {
        public string Division { get; set; }
        public string Dept { get; set; }
        public string StockNumber { get; set; }
        public string ProductTypeCode { get; set; }
        public string ProductTypeName { get; set; }
        public int ProductTypeID { get; set; }
    }
}