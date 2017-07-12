using Footlocker.Logistics.Allocation.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class LostSalesFactory
    {
        public LostSalesInstance Create(List<DataRow> dr)
        {
            LostSalesInstance newObject = new LostSalesInstance();
            newObject.ProductId = Convert.ToString(dr[0]["PRODUCT_ID"]);
            newObject.LocationId = Convert.ToString(dr[0]["LOCATION_ID"]);
            for (int i = 0; i < dr.Count - 1; i++) //get daily lost sales from each row except the last row of the list
            {
                newObject.DailySales[Convert.ToInt16(dr[i]["OFFSET"])] = Convert.ToDouble(dr[i]["RC_LOST_UNITS"]);
            }
             newObject.WeeklySales = Convert.ToDouble(dr[dr.Count-1]["RC_LOST_UNITS"]); //last row of the list is the prior week lost sales
            return newObject;
        }    
    }
}
