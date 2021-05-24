using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Validation;
using Footlocker.Logistics.Allocation.Factories;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Head Merchandiser,Merchandiser,Div Logistics,Director of Allocation,Admin,Support")]
    public class EcomCustXrefController : AppController
    {
        readonly AllocationLibraryContext db = new AllocationLibraryContext();
        private Repository<EcomCustomerFulfillmentXref> ecomXrefRepository;

        public EcomCustXrefController()
        {
            ecomXrefRepository = new Repository<EcomCustomerFulfillmentXref>(new AllocationLibraryContext());
        }

        //
        // GET: /EcomCustXref/

        public ViewResult Index()
        {
            List<EcomCustomerFulfillmentXref> model;

            model = (from e in db.EcomCustomerFulfillmentXrefs.Include("FulfillmentCenter")
                     select e).ToList();

            Dictionary<string, string> names = new Dictionary<string, string>();
            var users = (from a in model
                         select a.LastModifiedUser).Distinct();

            foreach (string userID in users)
            {
                names.Add(userID, getFullUserNameFromDatabase(userID.Replace('\\', '/')));
            }

            foreach (var item in model)
            {
                item.LastModifiedUser = names[item.LastModifiedUser];
            }

            return View(model);
        }

        public ActionResult Create()
        {
            EcomCustomerFulfillmentXrefModel model = new EcomCustomerFulfillmentXrefModel()
            {
                DataRec = new EcomCustFulfillmentXref()
            };

            model = SetDropDowns(model);

            return View(model);            
        }

        [HttpPost]
        public ActionResult Create(EcomCustomerFulfillmentXrefModel model)
        {
            List<ValidationResult> validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model.DataRec, new ValidationContext(model.DataRec, null, null), validationResults);
            if (isValid)
            {
                EcomCustomerFulfillmentXref newData = EcomCustFulfillmentFactory.CreateDBRec(model.DataRec, FullUserName);
                db.EcomCustomerFulfillmentXrefs.Add(newData);
                db.SaveChanges();
                TempData["message"] = "Record has been successfully created.";
                return RedirectToAction("Index");
            }
            else
            {
                model = SetDropDowns(model);

                return View(model);
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            EcomCustomerFulfillmentXref rec = db.EcomCustomerFulfillmentXrefs.Find(id);
            if (rec == null)
            {
                return HttpNotFound();
            }

            EcomCustomerFulfillmentXrefModel model = new EcomCustomerFulfillmentXrefModel()
            {
                DataRec = new EcomCustFulfillmentXref(), 
                ID = id.Value
            };

            model.DataRec = EcomCustFulfillmentFactory.CreateValidationRec(rec);
            model = SetDropDowns(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(EcomCustomerFulfillmentXrefModel model)
        {
            List<ValidationResult> validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model.DataRec, new ValidationContext(model.DataRec, null, null), validationResults);
            if (isValid)
            {
                EcomCustomerFulfillmentXref editedData = db.EcomCustomerFulfillmentXrefs.Where(e => e.FulfillmentXrefID == model.ID).FirstOrDefault();
                editedData = EcomCustFulfillmentFactory.CreateUpdatedDBRec(model.DataRec, editedData, FullUserName);
                
                db.SaveChanges();
                TempData["message"] = "Record has been updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                model = SetDropDowns(model);

                return View(model);
            }
        }

        public ActionResult Delete(int id)
        {
            EcomCustomerFulfillmentXref deleteData = db.EcomCustomerFulfillmentXrefs.Where(e => e.FulfillmentXrefID == id).FirstOrDefault();

            if (deleteData == null)
                TempData["message"] = "Record could not be found. Please refresh the grid and try again.";
            else
            {
                db.EcomCustomerFulfillmentXrefs.Remove(deleteData);
                db.SaveChanges();
                TempData["message"] = "Record has been deleted successfully.";
            }

            return RedirectToAction("Index");
        }

        #region JSON Result routines
        public JsonResult GetStateCodesJson(string Id)
        {
            List<SelectListItem> stateCodes = GetStateCodes(Id);
            return Json(new SelectList(stateCodes.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEcomStoresJson(string Id)
        {
            List<SelectListItem> ecomStores = GetECOMStores(Id);
            return Json(new SelectList(ecomStores.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Dropdown routines
        private EcomCustomerFulfillmentXrefModel SetDropDowns(EcomCustomerFulfillmentXrefModel model)
        {
            List<DistributionCenter> fulfillmentCenterList = (from fc in db.DistributionCenters
                                                              where fc.IsFulfillmentCenter == true
                                                              select fc).ToList();
            model.FulfillmentCenters = new SelectList(fulfillmentCenterList, "ID", "displayValue", model.DataRec.FulfillmentCenterID);

            model.CountryCodes = new List<SelectListItem>();

            foreach (var country in db.Countries)
            {
                model.CountryCodes.Add(new SelectListItem { Text = country.DisplayName, Value = country.Code });
            }

            model.StateCodes = GetStateCodes(model.DataRec.CountryCode);
            model.ECOMStores = GetECOMStores(model.DataRec.FulfillmentCenterID.ToString());

            return model;
        }

        public List<SelectListItem> GetStateCodes(string countryCode)
        {
            List<SelectListItem> stateCodes = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(countryCode))
            {
                foreach (var state in db.States.Where(s => s.CountryCode == countryCode))
                {
                    stateCodes.Add(new SelectListItem
                    {
                        Text = state.DisplayName,
                        Value = state.Code
                    });
                }
            }

            if (stateCodes.Count == 0)
                stateCodes.Add(new SelectListItem() { Text = "** None **", Value = "" });

            return stateCodes;
        }

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
                        Text = string.Format("{0}-{1} - {2}", rec.Division, rec.Store, rec.Name),
                        Value = string.Format("{0}-{1}", rec.Division, rec.Store)
                    });
                }
            }

            if (ecomStores.Count == 0)
                ecomStores.Add(new SelectListItem() { Text = "** None **", Value = "" });

            return ecomStores;
        }
        #endregion
    }
}
