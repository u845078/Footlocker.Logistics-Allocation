using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EditStoreAttributeModel
    {
        public List<Division> Divisions { get; set; }
        public List<StoreAttribute> StoreAttributes { get; set; }
        public List<FamilyOfBusiness> FOBs { get; set; }

        public StoreAttribute newStoreAttribute { get; set; }
    }
}