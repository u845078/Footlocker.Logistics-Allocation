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
        /*public LostSalesRequest Create(DataRow dr)
        {
            LostSalesRequest newObject = new LostSalesRequest();
            newObject.ProductId = Convert.ToString(dr["PRODUCT_ID"]);
            newObject.LocationId = Convert.ToString(dr["LOCATION_ID"]);
            newObject.Start = Convert.ToDateTime(dr["Day 1 Date"]);
            for (int i = 1; i < 15; i++) //set daily lost sales from day 1-14
            {
                newObject.DailySales.Add(Convert.ToDouble(dr["Day " + i]));
            }
            for (int i = 1; i < 3; i++) //set weekly lost sales
            {
                newObject.WeeklySales.Add(Convert.ToDouble(dr["Week " + i + " Totals"]));
            }
           
            return newObject;
        }*/


        public LostSalesRequest Create(List<DataRow> dr)
        {
            LostSalesRequest newObject = new LostSalesRequest();
            newObject.ProductId = Convert.ToString(dr[0]["PRODUCT_ID"]);
            newObject.LocationId = Convert.ToString(dr[0]["LOCATION_ID"]);
            newObject.Start = Convert.ToDateTime(dr[0]["DAY_DT"]);
            for (int i = 0; i < dr.Count - 1; i++) //get daily lost sales from each row except the last row of the list
            {
                newObject.DailySales.Add(Convert.ToDouble(dr[i]["RC_LOST_UNITS"]));
            }
             newObject.WeeklySales = Convert.ToDouble(dr[dr.Count-1]["RC_LOST_UNITS"]); //last row of the list is the prior week lost sales
            return newObject;
        }    
    }
}
