using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Head Merchandiser,Merchandiser,Div Logistics,Logistics,Director of Allocation,Admin,Support")]
    public class DTSController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        [CheckPermission(Roles = "Head Merchandiser,Merchandiser,Div Logistics,Logistics,Director of Allocation,Admin,Support")]
        public ActionResult Index(string message)
        {
            List<DirectToStoreSku> model;
            List<DirectToStoreSku> allDTS = db.DirectToStoreSkus.ToList();

            model = (from a in allDTS 
                     join b in currentUser.GetUserDepartments(AppName) 
                     on new { a.Division, a.Department } equals new { Division = b.DivCode, Department = b.DeptNumber } 
                     select a).ToList();

            if (model.Count > 0)
            {
                List<string> uniqueNames = (from l in model
                                            select l.CreatedBy).Distinct().ToList();
                Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

                foreach (var item in uniqueNames)
                {
                    fullNamePairs.Add(item, getFullUserNameFromDatabase(item.Replace('\\', '/')));
                }

                foreach (var item in fullNamePairs)
                {
                    model.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                }
            }

            ViewData["message"] = message;
            return View(model);
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Director of Allocation,Admin,Support")]
        public ActionResult Constraints()
        {
            List<DTSConstraintModel> model;

            List<DTSConstraintModel> allDTS = db.DTSConstraintModels.ToList();

            model = (from a in allDTS 
                     join b in currentUser.GetUserDepartments(AppName) 
                     on new { a.Division, a.Department } equals new { Division = b.DivCode, Department = b.DeptNumber } 
                     select a).ToList();

            return View(model);
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Director of Allocation,Admin,Support")]
        public ActionResult OneSizeConstraints()
        {
            return View();
        }

        [GridAction]
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Director of Allocation,Admin,Support")]
        public ActionResult _OneSizeConstraints()
        {
            List<DirectToStoreConstraint> model;

            if (Session["OneSizeConstraintsList"] == null)
            {
                DirectToStoreDAO dao = new DirectToStoreDAO(appConfig.EuropeDivisions);
                List<DirectToStoreConstraint> allDTS = dao.GetDTSConstraintsOneSize();
                model = (from a in allDTS join b in currentUser.GetUserDepartments(AppName) on new { a.Division, a.Department } equals new { Division = b.DivCode, Department = b.DeptNumber } orderby a.Sku select a).ToList();
                Session["OneSizeConstraintsList"] = model;
            }
            else
            {
                model = (List<DirectToStoreConstraint>)Session["OneSizeConstraintsList"];
            }

            return View(new GridModel(model));
        }

        [GridAction]
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Director of Allocation,Admin,Support")]
        public ActionResult _SaveOneSizeDetail([Bind(Prefix = "updated")]IEnumerable<DirectToStoreConstraint> updated)
        {
            Session["OneSizeConstraintsList"] = null;
            if (updated != null)
            {
                foreach (DirectToStoreConstraint update in updated)
                {
                    var query = (from a in db.DirectToStoreConstraints where ((a.Sku == update.Sku) && (a.Size == update.Size)) select a);
                    if (query.Count() > 0)
                    {
                        db.Entry(update).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        // Create constraint in context
                        db.DirectToStoreConstraints.Add(update);
                    }

                    // Set timestamp
                    update.CreateDate = DateTime.Now;
                    update.CreatedBy = currentUser.NetworkID;

                }
                // Persist constraint changes
                db.SaveChanges();
            }

            return View(new GridModel(new List<DirectToStoreConstraint>()));
        }


        [GridAction]
        public ActionResult _Index()
        {
            List<DirectToStoreSku> model;
            List<DirectToStoreSku> allDTS = (from a in db.DirectToStoreSkus.Include("ItemMaster")
                                             select a).ToList();
            model = (from a in allDTS 
                     join b in currentUser.GetUserDepartments(AppName) 
                     on new { a.Division, a.Department } equals new { Division = b.DivCode, Department = b.DeptNumber } 
                     select a).ToList();
            GridModel gridList = new GridModel(model);
            
            return View(gridList);
        }

        [GridAction]
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Director of Allocation,Admin,Support,")]
        public ActionResult _Constraints()
        {
            List<DirectToStoreSku> model;
            List<DirectToStoreSku> allDTS = (from a in db.DirectToStoreSkus select a).ToList();
            model = (from a in allDTS 
                     join b in currentUser.GetUserDepartments(AppName) 
                     on new { a.Division, a.Department } equals new { Division = b.DivCode, Department = b.DeptNumber } 
                     select a).ToList();
            return View(new GridModel(model));
        }

        [CheckPermission(Roles = "Head Merchandiser,Merchandiser,Div Logistics,Logistics,Director of Allocation,Admin,Support,")]
        public ActionResult Create()
        {
            DirectToStoreSku model = new DirectToStoreSku()
            {
                StartDate = DateTime.Now,
                VendorPackQty = 1
            };
            return View(model);
        }

        [HttpPost]
        [CheckPermission(Roles = "Head Merchandiser,Merchandiser,Div Logistics,Logistics,Director of Allocation,Admin,Support,")]
        public ActionResult Create(DirectToStoreSku model)
        {
            model.CreateDate = DateTime.Now;
            model.CreatedBy = currentUser.NetworkID;
            if (currentUser.HasDivDept(AppName, model.Sku.Substring(0, 2), model.Sku.Substring(3, 2)))
            {
                ItemMaster item = db.ItemMasters.Where(im => im.MerchantSku == model.Sku && im.Deleted == 0).FirstOrDefault();

                if (item != null)
                {
                    if (db.VendorGroupDetails.Where(vgd => vgd.VendorNumber == item.Vendor).Count() > 0)
                    {
                        model.ItemID = item.ID;
                        model.Vendor = item.Vendor;
                        db.DirectToStoreSkus.Add(model);

                        //update range - set start date for each store equal to the delivery group
                        List<DeliveryGroup> groups = (from a in db.DeliveryGroups 
                                                      join b in db.RangePlans 
                                                      on a.PlanID equals b.Id 
                                                      where b.Sku == item.MerchantSku 
                                                      select a).ToList();
                        RangePlanDetail detail;
                        foreach (DeliveryGroup dg in groups)
                        {
                            List<RuleSelectedStore> stores = db.RuleSelectedStores.Where(rss => rss.RuleSetID == dg.RuleSetID).ToList();
                            foreach (RuleSelectedStore store in stores)
                            {
                                detail = (from a in db.RangePlanDetails 
                                          where ((a.ID == dg.PlanID) && 
                                                 (a.Division == store.Division) && 
                                                 (a.Store == store.Store)) 
                                          select a).FirstOrDefault();
                                if (detail != null)
                                {
                                    if (dg.StartDate != null)
                                    {
                                        detail.StartDate = ((DateTime)dg.StartDate);
                                        db.Entry(detail).State = System.Data.EntityState.Modified;
                                    }
                                    if (dg.EndDate != null)
                                    {
                                        detail.EndDate = ((DateTime)dg.EndDate);
                                    }
                                }
                            }
                        }

                        db.SaveChanges(UserName);
                        UpdateRangeActiveARStatus();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewData["message"] = string.Format("You must add Vendor {0} to a Vendor Group before creating Direct To Store Skus for them.", item.Vendor);
                        return View(model);                        
                    }
                }
                else
                {
                    ViewData["message"] = "Sku not found.";
                    return View(model);
                }
            }
            else
            {
                ViewData["message"] = "You are not authorized to create DTS skus for this division.";
                return View(model);
            }
        }

        private void UpdateRangeActiveARStatus()
        {
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            itemDAO.UpdateActiveARStatus();
            Session["SkuSetup"] = null; 
        }

        [CheckPermission(Roles = "Head Merchandiser,Merchandiser,Div Logistics,Logistics,Director of Allocation,Admin,Support,")]
        public ActionResult Edit(string Sku)
        {
            DirectToStoreSku model = db.DirectToStoreSkus.Where(dts => dts.Sku == Sku).FirstOrDefault();
            DirectToStoreDAO dao = new DirectToStoreDAO(appConfig.EuropeDivisions);
            model.Vendors = dao.GetVendors(Sku);
            return View(model);
        }

        [HttpPost]
        [CheckPermission(Roles = "Head Merchandiser,Merchandiser,Div Logistics,Logistics,Director of Allocation,Admin,Support,")]
        public ActionResult Edit(DirectToStoreSku model)
        {
            DirectToStoreDAO vendorDao = new DirectToStoreDAO(appConfig.EuropeDivisions);
            model.Vendors = vendorDao.GetVendors(model.Sku);

            if (currentUser.HasDivDept(AppName, model.Sku.Substring(0, 2), model.Sku.Substring(3, 2)))
            {
                ItemMaster item = db.ItemMasters.Where(im => im.MerchantSku == model.Sku).FirstOrDefault();

                if (item != null)
                {
                    model.CreateDate = DateTime.Now;
                    model.CreatedBy = currentUser.NetworkID;
                    model.ItemID = item.ID;
                    
                    DirectToStoreDAO dao = new DirectToStoreDAO(appConfig.EuropeDivisions);
                    if (dao.IsVendorValidForSku(model.Vendor, model.Sku))
                    {
                        if (db.VendorGroupDetails.Where(vgd => vgd.VendorNumber == model.Vendor).Count() > 0)
                        {
                            db.Entry(model).State = System.Data.EntityState.Modified;
                            db.SaveChanges();
                            UpdateRangeActiveARStatus();
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewData["message"] = string.Format("You must add Vendor {0} to a Vendor Group before creating Direct To Store Skus for them.", item.Vendor);
                            return View(model);
                        } 
                    }
                    else
                    {
                        ViewData["message"] = "Vendor not valid for this sku.";
                        return View(model);
                    }
                }
                else
                {
                    ViewData["message"] = "Sku not found.";
                    return View(model);
                }
            }
            else
            {
                ViewData["message"] = "You are not authorized to edit DTS skus for this division.";
                return View(model);
            }
        }


        [CheckPermission(Roles = "Head Merchandiser,Merchandiser,Div Logistics,Logistics,Director of Allocation,Admin,Support,")]
        public ActionResult Delete(string Sku)
        {
            DirectToStoreSku model = (from a in db.DirectToStoreSkus 
                                      where a.Sku == Sku 
                                      select a).First();

            if (currentUser.HasDivision(AppName, model.Sku.Substring(0, 2)))
            {
                db.DirectToStoreSkus.Remove(model);
                db.SaveChanges();
                UpdateRangeActiveARStatus();
                string message = "Delete successful.";
                return RedirectToAction("Index", new { message = message });
            }
            else
            {               
                string message = "You are not authorized to delete DTS skus for this division.";
                return RedirectToAction("Index", new { message = message });
            }
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Logistics,Director of Allocation,Admin,Support,")]
        public ActionResult Details(string Sku)
        {
            //TODO add a record for each size of this sku
            SizeDAO dao = new SizeDAO();
            List<string> sizes = dao.GetSizes(Sku);

            if (sizes.Count == 0)
            {
                ViewData["message"] = "This sku is not ranged yet.";
            }
            ViewData["Sku"] = Sku;
            return View();
        }

        [GridAction]
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Logistics,Director of Allocation,Admin,Support,")]
        public ActionResult _Details(string Sku)
        {
            List<DirectToStoreConstraint> model = GetConstraintsForSku(Sku);
            return View(new GridModel(model));
        }

        private List<DirectToStoreConstraint> GetConstraintsForSku(string Sku)
        {
            List<DirectToStoreConstraint> model = (from a in db.DirectToStoreConstraints where (a.Sku == Sku) select a).ToList();
            //TODO add a record for each size of this sku
            SizeDAO dao = new SizeDAO();
            List<string> sizes = dao.GetSizes(Sku);
            //var notSetList = (from a in sizes where 
            var notSetList = sizes.Where(p => !model.Any(p2 => p2.Size == p));
            DirectToStoreConstraint item;
            DateTime controlDate = (from a in db.InstanceDivisions join b in db.ControlDates on a.InstanceID equals b.InstanceID where a.Division == Sku.Substring(0, 2) select b.RunDate).First();

            foreach (string size in notSetList)
            {
                item = new DirectToStoreConstraint();
                item.StartDate = controlDate.AddDays(1);
                item.Size = size;
                item.Sku = Sku;
                item.MaxQty = 99999;
                model.Add(item);
            }

            model = model.OrderBy(p => p.Size).ToList();
            return model;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveDetailBatchInsert([Bind(Prefix = "updated")]IEnumerable<DirectToStoreConstraint> updated, string Sku)
        {
            if (updated != null)
            {
                foreach (DirectToStoreConstraint update in updated)
                {
                    var query = (from a in db.DirectToStoreConstraints where ((a.Sku == update.Sku) && (a.Size == update.Size)) select a);
                    if (query.Count() > 0)
                    {
                        db.Entry(update).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        // Create constraint in context
                        db.DirectToStoreConstraints.Add(update);
                    }

                    // Set timestamp
                    update.CreateDate = DateTime.Now;
                    update.CreatedBy = User.Identity.Name;

                }
                // Persist constraint changes
                db.SaveChanges();
            }

            List<DirectToStoreConstraint> list = GetConstraintsForSku(Sku);
            return View(new GridModel(list));
        }


        [HttpPost]
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Director of Allocation,Admin,Support,")]
        public ActionResult SaveDetail(FormCollection fc)
        {
            DirectToStoreConstraint model= new DirectToStoreConstraint();
            string sku, size;
            sku = fc["item.sku"];
            size = fc["item.Size"];
            var query = (from a in db.DirectToStoreConstraints where ((a.Sku == sku) && (a.Size == size)) select a);
            if (query.Count() > 0)
            {
                model = query.First();
            }
            else
            {
                // Set sku and size, only done on creation
                model.Sku = sku;
                model.Size = size;

                // Create constraint in context
                db.DirectToStoreConstraints.Add(model);
            }
            
            // Update qty and dates
            model.MaxQty = Convert.ToInt32(fc["item.MaxQty"]);
            model.StartDate = Convert.ToDateTime(fc["StartDate" + size]);
            if (fc["EndDate"+size] != "")
            {
                model.EndDate = Convert.ToDateTime(fc["EndDate" + size]);
            }

            // Set timestamp
            model.CreateDate = DateTime.Now;
            model.CreatedBy = User.Identity.Name;

            // Persist constraint changes
            db.SaveChanges();

            return RedirectToAction("Details", new { Sku = model.Sku });
        }

        [HttpPost]
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Director of Allocation,Admin,Support,")]
        public ActionResult SaveOneSizeDetail(FormCollection fc)
        {

            DirectToStoreConstraint model = new DirectToStoreConstraint();
            string sku, size;
            sku = fc["item.sku"];
            size = fc["item.Size"];
            var query = (from a in db.DirectToStoreConstraints where ((a.Sku == sku) && (a.Size == size)) select a);
            if (query.Count() > 0)
            {
                model = query.First();
            }
            else
            {
                model.Sku = sku;
                model.Size = size;
                model.CreateDate = DateTime.Now;
                model.CreatedBy = User.Identity.Name;
                db.DirectToStoreConstraints.Add(model);
            }
            model.MaxQty = Convert.ToInt32(fc["item.MaxQty"]);
            model.StartDate = Convert.ToDateTime(fc["StartDate" + sku]);
            if (fc["EndDate" + sku] != "")
            {
                model.EndDate = Convert.ToDateTime(fc["EndDate" + sku]);
            }
            db.SaveChanges();
            return RedirectToAction("OneSizeConstraints");
        }

    }
}
