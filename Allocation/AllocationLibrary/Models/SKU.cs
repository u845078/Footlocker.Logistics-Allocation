using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SKU
    {
        public string skuString;

        public string Division
        {
            get
            {
                if (!string.IsNullOrEmpty(skuString))
                    return skuString.Split('-')[0];
                else
                    return string.Empty;
            }
        }

        public string Department
        {
            get
            {
                if (!string.IsNullOrEmpty(skuString))
                    return skuString.Split('-')[1];
                else
                    return string.Empty;
            }
        }

        public SKU(string sku) 
        { 
            skuString = sku;
        }
    }
}
