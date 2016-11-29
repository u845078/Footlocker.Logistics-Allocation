using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Space Planning,Director of Allocation,Admin,Support,Advanced Merchandiser Processes")]
    public class StoreAttributeController : AppController
    {
        //
        // GET: /StoreAttribute/
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index()
        {
            List<StoreLookup> model = new List<StoreLookup>();

            model = (from a in db.StoreAttributes join b in db.StoreLookups on new { a.Division, a.Store } equals new { b.Division, b.Store } select b).Distinct().ToList();
            model = (from a in model join b in this.Divisions() on a.Division equals b.DivCode select a).ToList();
                     
            return View(model);
        }

        public ActionResult Edit(string div, string store, string message)
        {
            EditStoreAttributeModel model = new EditStoreAttributeModel();
            model.StoreAttributes = (from a in db.StoreAttributes where ((a.Division == div) && (a.Store == store)) select a).ToList();

            model.newStoreAttribute = new StoreAttribute();
            model.newStoreAttribute.Division = div;
            model.newStoreAttribute.Store = store;
            model.newStoreAttribute.LikeDivision = div;
            model.newStoreAttribute.Weight = 100;
            model.newStoreAttribute.StartDate = DateTime.Now;
            model.Divisions = this.Divisions();
            ViewData["message"] = message;

            FamilyOfBusinessDAO dao = new FamilyOfBusinessDAO();
            model.FOBs = dao.GetFOBs(div);
            foreach (StoreAttribute sa in model.StoreAttributes)
            {
                if (sa.Level == "FOB")
                {
                    sa.ValueDescription = (from a in model.FOBs where a.Code == sa.Value select a.Description).First();
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(EditStoreAttributeModel model)
        {
            FamilyOfBusinessDAO dao = new FamilyOfBusinessDAO();
            model.newStoreAttribute.Store = model.newStoreAttribute.Store.PadLeft(5, '0');
            model.newStoreAttribute.LikeStore = model.newStoreAttribute.LikeStore.PadLeft(5, '0');

            if (model.newStoreAttribute.Store == model.newStoreAttribute.LikeStore)
            {
                ViewData["message"] = "Like store number must be a different store.";
                ResetStoreAttributeModel(model, dao);
                return View(model);
            }
            if (model.newStoreAttribute.Level == "FOB")
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^\d{3}$");
                if (!(regex.IsMatch(model.newStoreAttribute.Value)))
                {
                    ViewData["message"] = "Invalid format, expected ###";
                    ResetStoreAttributeModel(model, dao);
                    return View(model);
                }
            }
            else if ((model.newStoreAttribute.Level == "Dept"))
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^\d{2}$");
                if (!(regex.IsMatch(model.newStoreAttribute.Value)))
                {
                    ViewData["message"] = "Invalid format, expected ##";
                    ResetStoreAttributeModel(model, dao);
                    return View(model);

                }
            }
            model.FOBs = dao.GetFOBs(model.newStoreAttribute.Division);

            model.newStoreAttribute.CreateDate = DateTime.Now;
            model.newStoreAttribute.CreatedBy = User.Identity.Name;

            string message = VerifyStoreAttribute(model.newStoreAttribute);
            if (message != "")
            {
                return RedirectToAction("Edit", new { div = model.newStoreAttribute.Division, store = model.newStoreAttribute.Store, message = message });
            }

            db.StoreAttributes.Add(model.newStoreAttribute);
            db.SaveChanges();
            string div = model.newStoreAttribute.Division;
            string store = model.newStoreAttribute.Store;
            model.StoreAttributes = (from a in db.StoreAttributes where ((a.Division == div) && (a.Store == store)) select a).ToList();

            foreach (StoreAttribute sa in model.StoreAttributes)
            {
                if (sa.Level == "FOB")
                {
                    sa.ValueDescription = (from a in model.FOBs where a.Code == sa.Value select a.Description).First();
                }
            }

            model.newStoreAttribute = new StoreAttribute();
            model.newStoreAttribute.Division = div;
            model.newStoreAttribute.Store = store;
            model.newStoreAttribute.StartDate = DateTime.Now;
            model.newStoreAttribute.Weight = 100;
            model.Divisions = this.Divisions();

            return View(model);
        }

        private void ResetStoreAttributeModel(EditStoreAttributeModel model, FamilyOfBusinessDAO dao)
        {
            model.Divisions = this.Divisions();
            model.FOBs = dao.GetFOBs(model.newStoreAttribute.Division);
            model.StoreAttributes = (from a in db.StoreAttributes where ((a.Division == model.newStoreAttribute.Division) && (a.Store == model.newStoreAttribute.Store)) select a).ToList();
        }

        public ActionResult Create()
        {
            CreateStoreAttributeModel model = new CreateStoreAttributeModel();
            model.Divisions = this.Divisions();
            model.StoreAttribute = new StoreAttribute();
            model.StoreAttribute.StartDate = DateTime.Now;
            model.StoreAttribute.Weight = 100;
            FamilyOfBusinessDAO dao = new FamilyOfBusinessDAO();
            model.FOBs = dao.GetFOBs("");

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CreateStoreAttributeModel model)
        {
            try
            {
                if (model.StoreAttribute.Level == "FOB")
                {
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^\d{3}$");
                    if (!(regex.IsMatch(model.StoreAttribute.Value)))
                    {
                        ViewData["message"] = "Invalid format, expected ###";
                        model.Divisions = this.Divisions();
                        return View(model);

                    }
                }
                else if ((model.StoreAttribute.Level == "Dept"))
                {
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^\d{2}$");
                    if (!(regex.IsMatch(model.StoreAttribute.Value)))
                    {
                        ViewData["message"] = "Invalid format, expected ##";
                        model.Divisions = this.Divisions();
                        return View(model);

                    }
                }
                model.StoreAttribute.Store = model.StoreAttribute.Store.PadLeft(5, '0');
                model.StoreAttribute.LikeStore = model.StoreAttribute.LikeStore.PadLeft(5, '0');

                model.StoreAttribute.CreateDate = DateTime.Now;
                model.StoreAttribute.CreatedBy = User.Identity.Name;

                string message = VerifyStoreAttribute(model.StoreAttribute);
                if (message != "")
                {
                    return RedirectToAction("Edit", new { div = model.StoreAttribute.Division, store = model.StoreAttribute.Store, message = message });
                }
                model.StoreAttribute.LikeDivision = model.StoreAttribute.Division;
                db.StoreAttributes.Add(model.StoreAttribute);
                db.SaveChanges();
                return RedirectToAction("Edit", new { div = model.StoreAttribute.Division, store = model.StoreAttribute.Store });
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                model.Divisions = this.Divisions();
                FamilyOfBusinessDAO dao = new FamilyOfBusinessDAO();
                model.FOBs = dao.GetFOBs("");
                return View(model);
            }
        }
        public ActionResult Delete(Int32 ID)
        {
            StoreAttribute sa = (from a in db.StoreAttributes where a.ID == ID select a).First();
            string div = sa.Division;
            string store = sa.Store;

            try
            {
                db.StoreAttributes.Remove(sa);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
            }

            EditStoreAttributeModel model = new EditStoreAttributeModel();
            model.StoreAttributes = (from a in db.StoreAttributes where ((a.Division == div) && (a.Store == store)) select a).ToList();
            model.newStoreAttribute = new StoreAttribute();
            model.newStoreAttribute.Division = div;
            model.newStoreAttribute.Store = store;
            model.newStoreAttribute.LikeDivision = div;
            FamilyOfBusinessDAO dao = new FamilyOfBusinessDAO();
            model.FOBs = dao.GetFOBs(div);

            return View("Edit", model);
        }

        public ActionResult EditStoreAttribute(Int32 ID)
        {
            CreateStoreAttributeModel model = new CreateStoreAttributeModel();
            model.Divisions = this.Divisions();
            model.StoreAttribute = (from a in db.StoreAttributes where a.ID == ID select a).First();
            FamilyOfBusinessDAO dao = new FamilyOfBusinessDAO();
            model.FOBs = dao.GetFOBs(model.StoreAttribute.Division);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditStoreAttribute(CreateStoreAttributeModel model)
        {

            model.StoreAttribute.CreateDate = DateTime.Now;
            model.StoreAttribute.CreatedBy = User.Identity.Name;
            db.Entry(model.StoreAttribute).State = System.Data.EntityState.Modified;
            db.SaveChanges();

            EditStoreAttributeModel nextModel = new EditStoreAttributeModel();
            string div = model.StoreAttribute.Division;
            string store = model.StoreAttribute.Store;

            return RedirectToAction("Edit", new { div = div, store = store});
        }

        public string VerifyStoreAttribute(StoreAttribute sa)
        {
            var existing = (from a in db.StoreAttributes where ((a.Division == sa.Division) && (a.Store == sa.Store)&&(a.Level==sa.Level)&&(a.Value==sa.Value)
                            &&(
                                ((sa.StartDate >= a.StartDate)&&(a.EndDate == null))
                                ||((sa.StartDate <= a.StartDate)&&(sa.EndDate == null))
                                || ((sa.StartDate <= a.StartDate) && (sa.EndDate >= a.StartDate))
                                || ((sa.StartDate >= a.StartDate) && (sa.StartDate <= a.EndDate))
                            )
                            ) select a);
            if (existing.Count() > 0)
            {
                return "Already have a like store for this value.";
            }

            return "";
                
        }

    }
}
