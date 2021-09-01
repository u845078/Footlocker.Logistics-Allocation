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
            model = (from a in model join b in currentUser.GetUserDivisions(AppName) on a.Division equals b.DivCode select a).ToList();
                     
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
            model.newStoreAttribute.LikeStoreDemandScalingFactor = 1;
            model.newStoreAttribute.StartDate = DateTime.Now;
            model.Divisions = currentUser.GetUserDivisions(AppName);
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
            model.FOBs = dao.GetFOBs(model.newStoreAttribute.Division);
            string message = ValidateStoreAttribute(model.newStoreAttribute);
            if (message != "")
            {
                ViewData["message"] = message;
                model.Divisions = currentUser.GetUserDivisions(AppName);
                model.FOBs = dao.GetFOBs(model.newStoreAttribute.Division);
                model.StoreAttributes = (from a in db.StoreAttributes where ((a.Division == model.newStoreAttribute.Division) && (a.Store == model.newStoreAttribute.Store)) select a).ToList();
                return View(model);
                //return RedirectToAction("Edit", new { div = model.newStoreAttribute.Division, store = model.newStoreAttribute.Store, message = message });
            }
            else
            {
                model.newStoreAttribute.CreateDate = DateTime.Now;
                model.newStoreAttribute.CreatedBy = User.Identity.Name;
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
                model.Divisions = currentUser.GetUserDivisions(AppName);

                return View(model);
            }
        }

        private void ResetStoreAttributeModel(EditStoreAttributeModel model, FamilyOfBusinessDAO dao)
        {
            model.Divisions = currentUser.GetUserDivisions(AppName);
            model.FOBs = dao.GetFOBs(model.newStoreAttribute.Division);
            model.StoreAttributes = (from a in db.StoreAttributes where ((a.Division == model.newStoreAttribute.Division) && (a.Store == model.newStoreAttribute.Store)) select a).ToList();
        }

        public ActionResult Create()
        {
            CreateStoreAttributeModel model = new CreateStoreAttributeModel();
            model.Divisions = currentUser.GetUserDivisions(AppName);
            model.StoreAttribute = new StoreAttribute();
            model.StoreAttribute.StartDate = DateTime.Now;
            model.StoreAttribute.Weight = 100;
            model.StoreAttribute.LikeStoreDemandScalingFactor = 1;
            FamilyOfBusinessDAO dao = new FamilyOfBusinessDAO();
            model.FOBs = dao.GetFOBs("");

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CreateStoreAttributeModel model)
        {
            try
            {
                FamilyOfBusinessDAO fobDAO = new FamilyOfBusinessDAO();
                string message = ValidateStoreAttribute(model.StoreAttribute, true);
                if (message != "")
                {
                    ViewData["message"] = message;
                    model.Divisions = currentUser.GetUserDivisions(AppName);
                    model.FOBs = fobDAO.GetFOBs("");
                    return View(model);
                    //return RedirectToAction("Edit", new { div = model.StoreAttribute.Division, store = model.StoreAttribute.Store, message = message });
                }
                else
                {
                    model.StoreAttribute.Store = model.StoreAttribute.Store.PadLeft(5, '0');
                    model.StoreAttribute.LikeStore = model.StoreAttribute.LikeStore.PadLeft(5, '0');
                    model.StoreAttribute.LikeDivision = model.StoreAttribute.Division;
                    model.StoreAttribute.CreateDate = DateTime.Now;
                    model.StoreAttribute.CreatedBy = User.Identity.Name;
                    db.StoreAttributes.Add(model.StoreAttribute);
                    db.SaveChanges();
                    return RedirectToAction("Edit", new { div = model.StoreAttribute.Division, store = model.StoreAttribute.Store });
                }
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                model.Divisions = currentUser.GetUserDivisions(AppName);
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
            FamilyOfBusinessDAO dao = new FamilyOfBusinessDAO();
            model.FOBs = dao.GetFOBs(div);
            model.StoreAttributes = (from a in db.StoreAttributes where ((a.Division == div) && (a.Store == store)) select a).ToList();
            foreach (StoreAttribute s in model.StoreAttributes)
            {
                if (s.Level == "FOB")
                {
                    s.ValueDescription = (from a in model.FOBs where a.Code == s.Value select a.Description).First();
                }
            }
            model.newStoreAttribute = new StoreAttribute();
            model.newStoreAttribute.Division = div;
            model.newStoreAttribute.Store = store;
            model.newStoreAttribute.LikeDivision = div;
            model.newStoreAttribute.Weight = 100;
            model.newStoreAttribute.StartDate = DateTime.Now;

            return View("Edit", model);
        }

        public ActionResult EditStoreAttribute(Int32 ID)
        {
            CreateStoreAttributeModel model = new CreateStoreAttributeModel();
            model.Divisions = currentUser.GetUserDivisions(AppName);
            model.StoreAttribute = (from a in db.StoreAttributes where a.ID == ID select a).First();
            FamilyOfBusinessDAO dao = new FamilyOfBusinessDAO();
            model.FOBs = dao.GetFOBs(model.StoreAttribute.Division);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditStoreAttribute(CreateStoreAttributeModel model)
        {
            FamilyOfBusinessDAO fobDAO = new FamilyOfBusinessDAO();
            var message = ValidateStoreAttribute(model.StoreAttribute);
            if (message != "")
            {
                ViewData["message"] = message;
                model.Divisions = currentUser.GetUserDivisions(AppName);
                model.FOBs = fobDAO.GetFOBs("");
                return View(model);

            }
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

        private string ValidateStoreAttribute(StoreAttribute sa, bool onCreation = false)
        {
            string errorMessage = "";
            if (onCreation)
            {
                // verify the store was provided
                if (string.IsNullOrEmpty(sa.Store))
                {
                    errorMessage = SetErrorMessage(errorMessage, "The \"Store\" field must be provided.");
                }
                else
                {
                    // verify the division and store combination is valid
                    if (!IsValidStore(sa.Division, sa.Store))
                    {
                        string invalidErrorMessage = string.Format(
                            "The division and store combination {0}-{1} is not an existing or valid store."
                            , sa.Division
                            , sa.Store);

                        errorMessage = SetErrorMessage(errorMessage, invalidErrorMessage);
                    }
                }
                
                // verify that there are no existing store attributes with the passed div/store combo
                bool storeAttributeExists = (  from a in db.StoreAttributes
                                              where a.Store.Equals(sa.Store) && a.Division.Equals(sa.Division)
                                             select a).Any();
                if (storeAttributeExists)
                {
                    string storeExistsErrorMessage = string.Format(
                        "There is already an existing store attribute for division {0}, store {1}."
                        , sa.Division
                        , sa.Store);

                    errorMessage = SetErrorMessage(errorMessage, storeExistsErrorMessage);
                }
            }

            // ensure the like store was populated
            if (string.IsNullOrEmpty(sa.LikeStore))
            {
                errorMessage = SetErrorMessage(errorMessage, "The \"Like Store\" field must be provided.");
            }
            else
            {
                // verify the like store/division combination is valid
                if (!IsValidStore(sa.Division, sa.LikeStore))
                {
                    string likeStoreExistsErrorMessage = string.Format(
                        "The division and like store combination {0}-{1} is not an existing or valid store."
                        , sa.Division
                        , sa.LikeStore);
                    errorMessage = SetErrorMessage(errorMessage, likeStoreExistsErrorMessage);
                }
            }

            // ensure the new attributes startdate is less than or equal to the enddate
            if (sa.StartDate > sa.EndDate)
            {
                errorMessage = SetErrorMessage(errorMessage, "The Start Date must be less than or equal to the End Date.");
            }

            // ensure the store is not equal to the like store
            if (!string.IsNullOrEmpty(sa.Store) && sa.Store.Equals(sa.LikeStore))
            {
                errorMessage = SetErrorMessage(errorMessage, "The like store number must be a different store.");
            }

            // ensure the level's value is of the right length and has the correct characters.
            if (sa.Level.Equals("FOB"))
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^\d{3}$");
                if (!(regex.IsMatch(sa.Value)))
                {
                    errorMessage = SetErrorMessage(errorMessage, "Invalid format for \"FOB\" level, expected ###.");
                }
            }
            else if (sa.Level.Equals("Dept"))
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^\d{2}$");
                if (!(regex.IsMatch(sa.Value)))
                {
                    errorMessage = SetErrorMessage(errorMessage, "Invalid format for \"Dept\" level, expected ##.");
                }
                else
                {
                    if (!currentUser.GetUserDevDept(AppName).Contains(string.Format("{0}-{1}", sa.Division, sa.Value)))
                    {
                        errorMessage = SetErrorMessage(errorMessage, "The provided department does not exist or you do not have permission for this department.");
                    }
                }
            }

            // retrieve existing store attributes that have the same division, store, like store, level
            List<StoreAttribute> existingStoreAttributes = (from esa in db.StoreAttributes
                                                            where esa.Division.Equals(sa.Division) &&
                                                                    esa.Store.Equals(sa.Store) &&
                                                                    esa.LikeStore.Equals(sa.LikeStore) &&
                                                                    esa.Level.Equals(sa.Level) &&
                                                                    !esa.ID.Equals(sa.ID)
                                                            select esa).ToList();
            if (existingStoreAttributes.Count() > 0)
            {
                StoreAttribute intersectingAttr = existingStoreAttributes.Where(esa => IntersectsExistingDateRange(esa, sa)).FirstOrDefault();
                string existingErrorMessage = "";
                if (sa.Level.Equals("DEPT") || sa.Level.Equals("FOB"))
                {
                    if (existingStoreAttributes.Any(esa => esa.Value.Equals(sa.Value)))
                    {
                        // retrieve fobs
                        FamilyOfBusinessDAO dao = new FamilyOfBusinessDAO();
                        List<FamilyOfBusiness> fobs = dao.GetFOBs(sa.Division);
                        string valueDescription = (from a in fobs where a.Code == sa.Value select a.Description).First();
                        if (intersectingAttr != null)
                        {
                            existingErrorMessage = string.Format(
                                "Already have an existing attribute with like store \"{0}\", level \"{1}\", and value \"{2}\" that intersects with the provided date range."
                                , intersectingAttr.LikeStore
                                , intersectingAttr.Level
                                , valueDescription);
                        }
                    }
                }
                // Level 'All' will have a NULL value, therefore no need to compare the values of the attributes
                else if (sa.Level.Equals("All") && intersectingAttr != null)
                {
                    existingErrorMessage = string.Format(
                        "Already have an existing attribute with like store \"{0}\", and level \"{1}\" that intersects with the provided date range."
                        , sa.LikeStore
                        , sa.Level);
                }

                if (existingErrorMessage != "")
                {
                    errorMessage = SetErrorMessage(errorMessage, existingErrorMessage);
                }
            }

            return errorMessage;
        }

        /// <summary>
        /// Check to see if the new store attribute that is being created intersects with
        /// an existing store attribute that is provided.  The comments above each
        /// if statement is defined as such: 
        /// a = the existing
        /// </summary>
        /// <param name="existingAttr">The existing store attribute</param>
        /// <param name="newAttr">The new store attribute</param>
        /// <returns></returns>
        private bool IntersectsExistingDateRange(StoreAttribute existingAttr, StoreAttribute newAttr)
        {
            // b[ a[  b] a] => such that a is existing and b is new
            if (existingAttr.StartDate >= newAttr.StartDate && existingAttr.EndDate >= newAttr.EndDate && existingAttr.StartDate <= newAttr.EndDate)
            {
                return true;
            }

            // a[ b[  a] b]
            if (existingAttr.StartDate <= newAttr.StartDate && existingAttr.EndDate <= newAttr.EndDate && existingAttr.EndDate >= newAttr.StartDate)
            {
                return true;
            }

            // a[ b[  b] a]
            if (existingAttr.StartDate <= newAttr.StartDate && existingAttr.EndDate >= newAttr.EndDate)
            {
                return true;
            }

            // b[ a[  a] b]
            if (existingAttr.StartDate >= newAttr.StartDate && existingAttr.EndDate <= newAttr.EndDate)
            {
                return true;
            }

            // b[ a[  a] --->b] (b's enddate is null) OR
            // a[ b[  a] --->b] (b's enddate is null)
            if (existingAttr.EndDate >= newAttr.StartDate && newAttr.EndDate == null)
            {
                return true;
            }

            if (existingAttr.EndDate == null && newAttr.EndDate == null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check to see if the division/store combination will return any results
        /// within the vValidStores view.  If so return true, else return false
        /// </summary>
        /// <param name="division">the division being validated</param>
        /// <param name="store">the store being validated</param>
        /// <returns></returns>
        private bool IsValidStore(string division, string store)
        {
            return db.vValidStores.Any(vs => vs.Division == division && vs.Store == store);
        }

        /// <summary>
        /// If the existing error message is empty this means no other errors have occured yet. Therefore
        /// just put the new message.  If it is not empty, then put the existing error message +
        /// a line break + the new error message
        /// </summary>
        /// <param name="existingErrorMessage">existing error message</param>
        /// <param name="newErrorMessage">new error message</param>
        /// <returns></returns>
        private string SetErrorMessage(string existingErrorMessage, string newErrorMessage)
        {
            return (existingErrorMessage.Equals(string.Empty)) ? newErrorMessage : existingErrorMessage + @"<br />" + newErrorMessage;
        }
    }
}
