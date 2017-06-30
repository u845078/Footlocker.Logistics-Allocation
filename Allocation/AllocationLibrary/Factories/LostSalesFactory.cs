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
        public LostSalesFactory()
        {
        }

        public LostSalesRequest Create(DataRow dr)
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
        }
           
    }
}
