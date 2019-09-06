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

        public ActionResult Index()
        {
            List<EcomCustomerFulfillmentXref> model;

            model = (from ecfx in db.EcomCustomerFulfillmentXrefs
                     select ecfx).ToList();

            return View(model);
        }

        public ActionResult Create()
        {
            List<DistributionCenter> fulfillmentCenterList = (from fc in db.DistributionCenters
                                                              where fc.IsFulfillmentCenter == true
                                                              select fc).ToList();

            EcomCustomerFulfillmentXrefModel model = new EcomCustomerFulfillmentXrefModel
            {
                FulfillmentCenters = new List<SelectListItem>()
            };

            foreach (var fcl in fulfillmentCenterList)
            {
                model.FulfillmentCenters.Add(new SelectListItem { Text = fcl.displayValue, Value = fcl.ID.ToString() });
            }

            model.ECOMStores = GetECOMStores(model.FulfillmentCenters[0].Value);
         
            return View(model);            
        }

        public ActionResult Edit()
        {
            return View();
        }


        #region SelectListItem routines
        public List<SelectListItem> GetECOMStores(string fulfillmentCenterID)
        {
            List<SelectListItem> ecomStores = new List<SelectListItem>();
            int fcID;

            if (!string.IsNullOrEmpty(fulfillmentCenterID))
            {
                fcID = Convert.ToInt32(fulfillmentCenterID);

                var queryList = (from a in db.EcommWarehouses
                                 where a.StorageDCID == fcID
                                 orderby a.Store
                                 select a).ToList();

                foreach (var rec in queryList)
                {
                    ecomStores.Add(new SelectListItem
                    {
                        Text = String.Format("{0}-{1} - {2}", rec.Division, rec.Store, rec.Name),
                        Value = String.Format("{0}-{1}", rec.Division, rec.Store)
                    });
                }
            }

            return ecomStores;
        }
        #endregion
    }
}
