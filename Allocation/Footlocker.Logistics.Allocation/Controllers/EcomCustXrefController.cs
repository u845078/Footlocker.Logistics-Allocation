using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Head Merchandiser,Merchandiser,Div Logistics,Director of Allocation,Admin,Support")]
    public class EcomCustXrefController : AppController
    {
        readonly AllocationLibraryContext db = new AllocationLibraryContext();

        //
        // GET: /EcomCustXref/

        //public ActionResult Index()
        //{
        //    List<EcomCustomerFulfillmentXref> model;

        //    model = (from ecfx in db.EcomCustomerFulfillmentXrefs
        //             select ecfx).ToList();

        //    return View(model);
        //}

        public ActionResult Create()
        {
            List<DistributionCenter> fulfillmentCenterList = (from fc in db.DistributionCenters
                                                              where fc.IsFulfillmentCenter == true
                                                              select fc).ToList();

            EcomCustomerFulfillmentXrefModel model = new EcomCustomerFulfillmentXrefModel();
            
            foreach (var fcl in fulfillmentCenterList)
            {
                model.FulfillmentCenters.Add(new SelectListItem { Text = fcl.displayValue, Value = fcl.ID.ToString() });
            }

            var instanceList = (from i in db.Instances
                                select i).ToList();

            foreach (var il in instanceList)
            {
                model.Instances.Add(new SelectListItem { Text = il.Name, Value = il.ID.ToString() });
            }
         
            return View(model);
        }

        public ActionResult Edit()
        {
            return View();
        }
    }
}
