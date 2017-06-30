using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class LostSalesRequest
    {
        public String ProductId { get; set; }
        public String LocationId { get; set; }
        public DateTime Start { get; set; }
        public List<double> DailySales { get; set; }
        public List<double> WeeklySales { get; set; }

        public LostSalesRequest()
        {
            DailySales = new List<double>();
            WeeklySales = new List<double>();
        }
    }
}