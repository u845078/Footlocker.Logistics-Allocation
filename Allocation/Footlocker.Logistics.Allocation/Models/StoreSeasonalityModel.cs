using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreSeasonalityModel
    {
        public string CurrentDivision { get; set; }
        public List<Division> Divisions { get; set; }
        public List<StoreSeasonality> List { get; set; }

        public List<ValidStoreLookup> UnassignedStores { get; set; }
    }
}