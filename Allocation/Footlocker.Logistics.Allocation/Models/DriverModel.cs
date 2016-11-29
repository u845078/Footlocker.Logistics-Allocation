using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DriverModel
    {
        public string CurrentDivision { get; set; }
        public List<Division> Divisions { get; set; }
        public List<Department> Departments { get; set; }
        public List<AllocationDriver> Drivers { get; set; }
        public AllocationDriver NewDriver { get; set; }
    }
}