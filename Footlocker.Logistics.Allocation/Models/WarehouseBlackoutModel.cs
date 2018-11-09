using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class WarehouseBlackoutModel
    {
        public WarehouseBlackout WarehouseBlackout { get; set; }
        public List<DistributionCenter> Warehouses { get; set; }
    }
}