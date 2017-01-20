using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;
using System.Data.Entity;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Admin,IT,Support")]
    public class PurgeArchiveController : Controller
    {

        #region private members

        private PurgeArchiveDAO patDAO;
        private AllocationLibraryContext db;

        #endregion

        public PurgeArchiveController()
        {
            patDAO = new PurgeArchiveDAO();
            db = new AllocationLibraryContext();
        }

        #region Index

        [GridAction]
        public ActionResult Index()
        {
            List<Instance> list = (from a in db.Instances select a).ToList();
            return View(new GridModel(list));
        }

        /// <summary>
        /// Used in DetailGrid to populate list of purge types by instance
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Popoulated GridModel with purge types by instance</returns>
        [GridAction]
        public ActionResult PurgeArchiveTypesByInstance(string id)
        {
            int instanceID = Convert.ToInt32(id);
            List<PurgeArchiveType> list = patDAO.GetPurgeArchiveTypesByInstance(instanceID);
            return View(new GridModel(list));
        }

        /// <summary>
        /// Update the PurgeArchiveType selected by the user.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        [GridAction]
        public ActionResult UpdatePurgeArchiveTypes(string ID)
        {
            int purgeArchiveTypeID = Convert.ToInt32(ID);
            PurgeArchiveType pat = patDAO.GetPurgeArchiveTypeByID(purgeArchiveTypeID);

            if (pat != null)
            {
                if (TryUpdateModel(pat))
                {
                    //service call
                    patDAO.Update(pat, User.Identity.Name);
                }
            }
            List<PurgeArchiveType> result = patDAO.GetPurgeArchiveTypesByInstance(pat.InstanceID);
            return View(new GridModel(result));
        }

        #endregion

        #region Create

        /// <summary>
        /// Populate PurgeArchiveTypeModel with the instances which will be passed to the view.
        /// </summary>
        /// <returns>The 'Create' View</returns>
        public ActionResult Create()
        {
            PurgeArchiveTypeModel model = new PurgeArchiveTypeModel();
            model.Instances = PopulateInstances();
            return View(model);
        }

        /// <summary>
        /// Retrieve PurgeArchiveTypeModel from view and create a purge archive type
        /// for each selected instance.
        /// </summary>
        /// <param name="model">PurgeArchiveTypeModel populated from view</param>
        /// <returns>Index View</returns>
        [HttpPost]
        public ActionResult Create(PurgeArchiveTypeModel model)
        {
            List<PurgeArchiveType> modelsToCreate = new List<PurgeArchiveType>();
            List<InstanceModel> selectedInstances = model.Instances.Where(instance => instance.Selected).ToList();

            //ensure at least one instance is selected
            if (selectedInstances.Count() > 0)
            {
                //trim archivetype and archivetypedescription before possibly creating numerous models
                model.purgeArchiveType.ArchiveType = model.purgeArchiveType.ArchiveType.Trim();
                model.purgeArchiveType.ArchiveTypeDescription = model.purgeArchiveType.ArchiveTypeDescription.Trim();

                //ensure archivetype is unique and does not already exist for the selected instances
                if (ValidateNonExistentType(model.purgeArchiveType, selectedInstances))
                {
                    foreach (InstanceModel i in selectedInstances)
                    {
                        //create model for each instance
                        modelsToCreate.Add(new PurgeArchiveType
                        {
                            ArchiveType = model.purgeArchiveType.ArchiveType,
                            ArchiveTypeDescription = model.purgeArchiveType.ArchiveTypeDescription,
                            DaysUntilPurge = model.purgeArchiveType.DaysUntilPurge,
                            ActiveInd = model.purgeArchiveType.ActiveInd,
                            InstanceID = i.Instance.ID
                        });
                    }

                    //service call
                    patDAO.Create(modelsToCreate, User.Identity.Name);
                    return RedirectToAction("Index");
                }
                else
                {
                    model.Instances = PopulateInstances();
                    return View(model);
                }
            }
            else
            {
                ViewData["instanceError"] = "You must select at least one instance.";
                model.Instances = PopulateInstances();
                return View(model);
            }
        }

        #endregion

        #region Helper Methods (Validation, Population)

        /// <summary>
        /// Ensure that the new PurgeArchiveType to be created does not have an identical ArchiveType.
        /// </summary>
        /// <param name="pat">PurgeArchiveType model</param>
        /// <param name="instances">List of selected instances</param>
        /// <returns>Boolean determinate of existent types</returns>
        public bool ValidateNonExistentType(PurgeArchiveType pat, List<InstanceModel> instances)
        {
            bool result = true;

            foreach (InstanceModel i in instances)
            {
                List<PurgeArchiveType> list = patDAO.GetPurgeArchiveTypes().Where
                                              (model =>
                                                model.ArchiveType == pat.ArchiveType &&
                                                model.InstanceID == i.Instance.ID
                                              ).ToList();
                if (list.Count > 0)
                {
                    result = false;
                    string errorMessage = string.Format("The Purge Type already exists for {0}", i.Instance.Name);
                    ViewData["existentError"] += errorMessage + (i.Equals(instances.Last()) ? "" : @"<br />");
                }
            }
            return result;
        }

        /// <summary>
        /// Helper method to populate instances to be passed to the View
        /// </summary>
        /// <returns>List of instances</returns>
        public List<InstanceModel> PopulateInstances()
        {
            List<InstanceModel> result = new List<InstanceModel>();
            List<Instance> instances = (from a in db.Instances select a).ToList();
            //convert to InstanceModel for View
            result = instances.Select(instance => new InstanceModel
                     {
                        Instance = instance,
                        Selected = false
                     }).ToList();

            return result;
        }

        #endregion
    }
}
