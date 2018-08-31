using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class LostSalesRequest
    {
        public List<LostSalesInstance> LostSales { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeeklySalesEndIndex { get; set; }

        public LostSalesRequest()
        {
            LostSales = new List<LostSalesInstance>();
        }
    }
}
