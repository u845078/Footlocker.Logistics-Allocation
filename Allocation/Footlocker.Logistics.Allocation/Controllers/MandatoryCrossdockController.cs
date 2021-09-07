using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class MandatoryCrossdockController : AppController
    {
        #region Initializations

        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        
        #endregion

        private List<MandatoryCrossdock> GetMandatoryCrossdocksForUser()
        {
            List<string> divs = (from a in currentUser.GetUserDivisions(AppName) select a.DivCode).ToList();
            List<string> temp = new List<String>();

            foreach (string div in divs)
            {
                temp.AddRange((from a in WebSecurityService.ListUserDepartments(UserName, "Allocation", div) select div + '-' + a.DeptNumber).ToList());
            }

            //List<MandatoryCrossdock> model = db.MandatoryCrossdocks.Include("ItemMaster").Where(u => temp.Contains(u.ItemMaster.MerchantSku.Substring(0, 5))).ToList();

            var query = (from mc in db.MandatoryCrossdocks
                         join im in db.ItemMasters
                           on mc.ItemID equals im.ID
                         select new { MandatoryCrossDock = mc, Division = im.Div, Department = im.Dept }).ToList();

            List<MandatoryCrossdock> model = query.Where(q => temp.Contains(q.Division + "-" + q.Department))
                                                   .Select(q => q.MandatoryCrossDock)
                                                   .ToList();
            return model;
        }

        //
        // GET: /MandatoryCrossdock/
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Admin,Support")]
        public ActionResult Index()
        {
            List<MandatoryCrossdock> model = GetMandatoryCrossdocksForUser();
            return View(model);
        }

        [GridAction]
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Director of Allocation,Admin,Support")]
        public ActionResult _StoreDefaults(int InstanceID, Int64 ItemID)
        {
            List<MandatoryCrossdockDefault> model = (from a in db.MandatoryCrossdockDefaults where ((a.InstanceID == InstanceID) && (a.ItemID == ItemID)) select a).ToList();
            return View(new GridModel(model));
        }


        /// <summary>
        /// Add a default store for the sku
        /// </summary>
        public JsonResult AddStore(int instanceid, Int64 itemid, string store, int percent)
        {
            store = store.PadLeft(5, '0');
            try
            {
                ItemMaster i = (from a in db.ItemMasters where a.ID == itemid select a).First();

                if ((from a in db.vValidStores where ((a.Division == i.Div) && (a.Store == store)) select a).Count() == 0)
                {
                    return Json("Invalid Store");
                }

                MandatoryCrossdockDefault model = new MandatoryCrossdockDefault();
                model.InstanceID = (from a in db.InstanceDivisions where a.Division == i.Div select a.InstanceID).First();
                model.ItemID = i.ID;
                model.Store = store;
                model.Division = i.Div;
                model.PercentAsInt = percent;

                db.MandatoryCrossdockDefaults.Add(model);
                db.SaveChanges(UserName);

                return Json("Success");
            }
            catch (Exception ex)
            {
                return Json("Error");
            }
            //return GetGridJson(planID, page);
        }


        /// <summary>
        /// Remove a default store for the sku
        /// </summary>
        public JsonResult DeleteStore(int instanceid, Int64 itemid, string store)
        {
            try
            {
                MandatoryCrossdockDefault model = (from a in db.MandatoryCrossdockDefaults where ((a.InstanceID == instanceid) && (a.ItemID == itemid) && (a.Store == store)) select a).First();
                db.MandatoryCrossdockDefaults.Remove(model);
                db.SaveChanges(UserName);

                return Json("Success");
            }
            catch (Exception ex)
            {
                return Json("Error");
            }
            //return GetGridJson(planID, page);
        }


        /// <summary>
        /// Add a store to the range plan (planID)
        /// </summary>
        public JsonResult GetMessage(int instanceid, Int64 itemid)
        {
            try
            {
                if ((from a in db.MandatoryCrossdockDefaults where ((a.InstanceID == instanceid) && (a.ItemID == itemid)) select a.Percent).Sum() != Convert.ToDecimal(1))
                {
                    return Json("The percent must add up to 100 for everything to crossdock correctly");
                }


                return Json("Success");
            }
            catch (Exception ex)
            {
                return Json("The percent must add up to 100 for everything to crossdock correctly");
            }
            //return GetGridJson(planID, page);
        }


        public ActionResult Edit(int instanceID, Int64 itemID)
        {

            EditMandatoryCrossdock model = new EditMandatoryCrossdock();
            try
            {
                model.MandatoryCrossdock = (from a in db.MandatoryCrossdocks where ((a.InstanceID == instanceID) && (a.ItemID == itemID)) select a).First();
                try
                {
                    if ((from a in db.MandatoryCrossdockDefaults where ((a.InstanceID == instanceID) && (a.ItemID == itemID)) select a.Percent).Sum() != Convert.ToDecimal(1))
                    {
                        model.Message = "The percent must add up to 100 for everything to crossdock correctly";
                    }
                }
                catch { }

                model.Sku = model.MandatoryCrossdock.ItemMaster.MerchantSku;
            }
            catch (Exception ex)
            {
                model.Message = "Error:  " + ex.Message;
                if (model.MandatoryCrossdock == null)
                {
                    model.MandatoryCrossdock = new MandatoryCrossdock();
                }
            }
            return View(model);
        }

        public ActionResult Create()
        {
            return View(new EditMandatoryCrossdock());
        }

        [HttpPost]
        public ActionResult Create(EditMandatoryCrossdock model)
        {
            ItemMaster i = (from a in db.ItemMasters where a.MerchantSku == model.Sku select a).FirstOrDefault();

            if (i == null)
            {
                model.Message = "Invalid item";
                return View(model);
            }

            try
            {
                InstanceDivision inst = (from a in db.InstanceDivisions where a.Division == i.Div select a).FirstOrDefault();
                model.MandatoryCrossdock = (from a in db.MandatoryCrossdocks where ((a.InstanceID == inst.InstanceID) && (a.ItemID == i.ID)) select a).FirstOrDefault();
                if (model.MandatoryCrossdock == null)
                {
                    model.MandatoryCrossdock = new MandatoryCrossdock();
                    model.MandatoryCrossdock.InstanceID = inst.InstanceID;
                    model.MandatoryCrossdock.ItemID = i.ID;
                    model.MandatoryCrossdock.CreatedBy = UserName;
                    model.MandatoryCrossdock.CreateDate = DateTime.Now;

                    db.MandatoryCrossdocks.Add(model.MandatoryCrossdock);
                    db.SaveChanges(UserName);
                }
            }
            catch (Exception ex)
            {
                model.Message = "Error:  " + ex.Message;
                return View(model);                
            }

            return RedirectToAction("Edit", new { instanceID = model.MandatoryCrossdock.InstanceID, itemID = model.MandatoryCrossdock.ItemID });
        }


        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveStoreDefaults([Bind(Prefix = "updated")]IEnumerable<MandatoryCrossdockDefault> updated)
        {
            int instanceid=0;
            Int64 itemid=0;
            foreach (MandatoryCrossdockDefault mcd in updated)
            {
                db.Entry(mcd).State = System.Data.EntityState.Modified;
                instanceid = mcd.InstanceID;
                itemid = mcd.ItemID;
            }
            db.SaveChanges(UserName);

            List<MandatoryCrossdockDefault> model = (from a in db.MandatoryCrossdockDefaults where ((a.InstanceID == instanceid) && (a.ItemID == itemid)) select a).ToList();

            return View(new GridModel(model));

        }

        public ActionResult Delete(int instanceID, Int64 itemID)
        {
            EditMandatoryCrossdock model = new EditMandatoryCrossdock();
            model.MandatoryCrossdock = (from a in db.MandatoryCrossdocks where ((a.InstanceID == instanceID) && (a.ItemID == itemID)) select a).First();
            foreach (MandatoryCrossdockDefault m in (from a in db.MandatoryCrossdockDefaults where ((a.InstanceID == instanceID) && (a.ItemID == itemID)) select a).ToList())
            {
                db.MandatoryCrossdockDefaults.Remove(m);
            }
            MandatoryCrossdock mc = (from a in db.MandatoryCrossdocks where ((a.InstanceID == instanceID) && (a.ItemID == itemID)) select a).First();
            db.MandatoryCrossdocks.Remove(mc);

            db.SaveChanges(UserName);

            return RedirectToAction("Index");
        }
        //[HttpPost]
        //public ActionResult Edit(EditMandatoryCrossdock model)
        //{
        //    //TODO save the edits (crossdock defaults)
        //    return View(model);
        //}


    }
}
