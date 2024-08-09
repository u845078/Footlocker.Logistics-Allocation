using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
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
            List<string> temp = new List<string>();

            foreach (string div in currentUser.GetUserDivList(AppName))
            {
                temp.AddRange(currentUser.GetUserDivDept(AppName));
            }

            var query = (from mc in db.MandatoryCrossdocks
                         join im in db.ItemMasters
                           on mc.ItemID equals im.ID
                         select new { MandatoryCrossDock = mc, Division = im.Div, Department = im.Dept }).ToList();

            List<MandatoryCrossdock> model = query.Where(q => temp.Contains(q.Division + "-" + q.Department))
                                                   .Select(q => q.MandatoryCrossDock)
                                                   .ToList();

            List<string> uniqueNames = (from a in model
                                        where !string.IsNullOrEmpty(a.CreatedBy)
                                        select a.CreatedBy).Distinct().ToList();

            Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

            foreach (var item in fullNamePairs)
            {
                model.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
            }
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
        public ActionResult _StoreDefaults(int InstanceID, long ItemID)
        {
            List<MandatoryCrossdockDefault> model = db.MandatoryCrossdockDefaults.Where(mcd => mcd.InstanceID == InstanceID && mcd.ItemID == ItemID).ToList();
            return View(new GridModel(model));
        }

        /// <summary>
        /// Add a default store for the sku
        /// </summary>
        public JsonResult AddStore(int instanceid, long itemid, string store, int percent)
        {
            store = store.PadLeft(5, '0');
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            ConfigService configService = new ConfigService();

            try
            {
                ItemMaster i = itemDAO.GetItem(itemid);                

                if (db.vValidStores.Where(vs => vs.Division == i.Div && vs.Store == store).Count() == 0)                
                    return Json("Invalid Store");

                MandatoryCrossdockDefault model = new MandatoryCrossdockDefault()
                {
                    InstanceID = configService.GetInstance(i.Div),
                    ItemID = i.ID,
                    Store = store,
                    Division = i.Div,
                    PercentAsInt = percent
                };

                db.MandatoryCrossdockDefaults.Add(model);
                db.SaveChanges(currentUser.NetworkID);

                return Json("Success");
            }
            catch 
            {
                return Json("Error");
            }
        }

        /// <summary>
        /// Remove a default store for the sku
        /// </summary>
        public JsonResult DeleteStore(int instanceid, long itemid, string store)
        {
            try
            {
                MandatoryCrossdockDefault model = db.MandatoryCrossdockDefaults.Where(mcd => mcd.InstanceID == instanceid && mcd.ItemID == itemid && mcd.Store == store).First();
                db.MandatoryCrossdockDefaults.Remove(model);
                db.SaveChanges(currentUser.NetworkID);

                return Json("Success");
            }
            catch
            {
                return Json("Error");
            }
        }

        /// <summary>
        /// Add a store to the range plan (planID)
        /// </summary>
        public JsonResult GetMessage(int instanceid, long itemid)
        {
            try
            {
                if ((from a in db.MandatoryCrossdockDefaults 
                     where a.InstanceID == instanceid && a.ItemID == itemid
                     select a.Percent).Sum() != Convert.ToDecimal(1))
                {
                    return Json("The percent must add up to 100 for everything to crossdock correctly");
                }

                return Json("Success");
            }
            catch 
            {
                return Json("The percent must add up to 100 for everything to crossdock correctly");
            }
        }

        public ActionResult Edit(int instanceID, long itemID)
        {
            EditMandatoryCrossdock model = new EditMandatoryCrossdock();
            try
            {
                model.MandatoryCrossdock = db.MandatoryCrossdocks.Where(mc => mc.InstanceID == instanceID && mc.ItemID == itemID).First();

                try
                {
                    if ((from a in db.MandatoryCrossdockDefaults 
                         where a.InstanceID == instanceID && a.ItemID == itemID 
                         select a.Percent).Sum() != Convert.ToDecimal(1))
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
        [ValidateAntiForgeryToken]
        public ActionResult Create(EditMandatoryCrossdock model)
        {
            ConfigService configService = new ConfigService();

            ItemMaster i = db.ItemMasters.Where(im => im.MerchantSku == model.Sku).FirstOrDefault();

            if (i == null)
            {
                model.Message = "Invalid item";
                return View(model);
            }

            try
            {
                int instanceID = configService.GetInstance(i.Div);
                
                model.MandatoryCrossdock = db.MandatoryCrossdocks.Where(mc => mc.InstanceID == instanceID && mc.ItemID == i.ID).FirstOrDefault();
                if (model.MandatoryCrossdock == null)
                {
                    model.MandatoryCrossdock = new MandatoryCrossdock()
                    {
                        InstanceID = instanceID,
                        ItemID = i.ID,
                        CreatedBy = currentUser.NetworkID,
                        CreateDate = DateTime.Now
                    };

                    db.MandatoryCrossdocks.Add(model.MandatoryCrossdock);
                    db.SaveChanges(currentUser.NetworkID);
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
            long itemid =0;
            foreach (MandatoryCrossdockDefault mcd in updated)
            {
                db.Entry(mcd).State = System.Data.EntityState.Modified;
                instanceid = mcd.InstanceID;
                itemid = mcd.ItemID;
            }

            db.SaveChanges(currentUser.NetworkID);

            List<MandatoryCrossdockDefault> model = db.MandatoryCrossdockDefaults.Where(mcd => mcd.InstanceID == instanceid && mcd.ItemID == itemid).ToList();

            return View(new GridModel(model));
        }

        public ActionResult Delete(int instanceID, long itemID)
        {
            EditMandatoryCrossdock model = new EditMandatoryCrossdock();
            model.MandatoryCrossdock = (from a in db.MandatoryCrossdocks where a.InstanceID == instanceID && a.ItemID == itemID select a).First();
            foreach (MandatoryCrossdockDefault m in (from a in db.MandatoryCrossdockDefaults where a.InstanceID == instanceID && a.ItemID == itemID select a).ToList())
            {
                db.MandatoryCrossdockDefaults.Remove(m);
            }
            MandatoryCrossdock mc = (from a in db.MandatoryCrossdocks where ((a.InstanceID == instanceID) && (a.ItemID == itemID)) select a).First();
            db.MandatoryCrossdocks.Remove(mc);

            db.SaveChanges(currentUser.NetworkID);

            return RedirectToAction("Index");
        }
    }
}
