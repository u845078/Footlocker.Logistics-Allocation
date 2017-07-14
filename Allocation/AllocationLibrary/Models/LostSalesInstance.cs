using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class LostSalesInstance
    {
        public String ProductId { get; set; }
        public String LocationId { get; set; }
        public double[] DailySales { get; set; }
        public double WeeklySales { get; set; }

        public LostSalesInstance()
        {
            DailySales = new double[14];
            //initialize each element to 0 to avoid null values if not all elements are assigned to a lost sale value
            for (int i = 0; i < DailySales.Count(); i++)
            {
                DailySales[i] = 0;
            }
        }
    }
}