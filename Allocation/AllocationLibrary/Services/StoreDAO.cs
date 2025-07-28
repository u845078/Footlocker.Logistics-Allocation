using System;
using System.Collections.Generic;
using System.Data;
using Footlocker.Logistics.Allocation.Models;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Services
{
    public class StoreDAO
    {
        readonly AllocationLibraryContext db = new AllocationLibraryContext();

        public ValidStoreLookup GetValidStore(string division, string storeNumber)
        {
            return db.ValidStores.Where(vs => vs.Division == division && vs.Store == storeNumber).FirstOrDefault();
        }
    }
}
