using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class CreateStoreAttributeModel
    {
        public List<Division> Divisions { get; set; }
        public StoreAttribute StoreAttribute { get; set; }
        public List<FamilyOfBusiness> FOBs { get; set; }
    }
}