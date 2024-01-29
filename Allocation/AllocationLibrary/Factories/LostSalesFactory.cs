using Footlocker.Logistics.Allocation.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class LostSalesFactory
    {
        public LostSalesInstance Create(List<DataRow> dr)
        {
            LostSalesInstance newObject = new LostSalesInstance()
            {
                ProductId = Convert.ToString(dr[0]["PRODUCT_ID"]),
                LocationId = Convert.ToString(dr[0]["LOCATION_ID"])
            };

            //get daily lost sales from each row except the last row of the list
            for (int i = 0; i < dr.Count; i++) 
            {
                newObject.DailySales[Convert.ToInt16(dr[i]["OFFSET"])] = Convert.ToDouble(dr[i]["RC_LOST_UNITS"]);
            }

            return newObject;
        }    
    }
}
