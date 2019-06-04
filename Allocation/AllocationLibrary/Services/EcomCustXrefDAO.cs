using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Footlocker.Logistics.Allocation.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Footlocker.Logistics.Allocation.Services
{
    public class EcomCustXrefDAO
    {
        readonly Database _database;
        readonly AllocationLibraryContext db = new AllocationLibraryContext();

        public EcomCustXrefDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        //public List<EcomCustomerFulfillmentXref> List()
        //{
        //    List<EcomCustomerFulfillmentXref> results;

        //    results = (from ecfx in db.EcomCustomerFulfillmentXrefs
        //               select ecfx).ToList();
        //    return results;
        //}

        public void Add(EcomCustomerFulfillmentXref newRec)
        {

        }

        public void Edit(EcomCustomerFulfillmentXref editRec)
        {

        }
    }
}
