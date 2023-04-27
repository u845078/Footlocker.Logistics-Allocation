using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.DAO;
using System.IO;
using Aspose.Excel;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;
using System.Web.Helpers;
using Footlocker.Logistics.Allocation.Common;
using System.Web.ApplicationServices;
using System.Web.Services.Description;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Support,Logistics")]
    public class DTSVendorGroupController : AppController
    {
        readonly AllocationContext db = new AllocationContext();
        readonly VendorGroupDetailDAO vendorGroupDetailDAO = new VendorGroupDetailDAO();

        public ActionResult Index(string message)
        {
            ViewData["message"] = message;
            return View(db.VendorGroups);
        }

        public ActionResult Create(string name)
        {
            string message="";
            try
            {
                VendorGroup group = new VendorGroup()
                {
                    Comment = name,
                    CreatedBy = currentUser.NetworkID,
                    CreateDate = DateTime.Now
                };

                db.VendorGroups.Add(group);
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                try
                {
                    message = ex2.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;
                }
                catch
                {
                    message = ex2.Message;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return RedirectToAction("Index", new { message });
        }

        public ActionResult Delete(int ID)
        {
            List<VendorGroupDetail> details = db.VendorGroupDetails.Where(vgd => vgd.GroupID == ID).ToList();
            foreach (VendorGroupDetail det in details)
            {
                db.VendorGroupDetails.Remove(det);
            }

            VendorGroup group = db.VendorGroups.Where(vg => vg.ID == ID).First();

            db.VendorGroups.Remove(group);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Details(int ID, string message)
        {
            ViewData["message"] = message;
            VendorGroupDetailsModel model = new VendorGroupDetailsModel()
            {
                HasDetails = db.VendorGroupDetails.Where(vgd => vgd.GroupID == ID).Any(),
                Header = db.VendorGroups.Where(vg => vg.ID == ID).FirstOrDefault()
            };

            ViewData["GroupID"] = ID;
            return View(model);
        }

        [GridAction]
        public ActionResult _RefreshGrid(int ID)
        {
            List<VendorGroupDetail> list = vendorGroupDetailDAO.GetVendorGroupDetails(ID);

            if (list.Count > 0)
            {
                List<string> uniqueNames = (from l in list
                                            select l.CreatedBy).Distinct().ToList();
                Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

                List<ApplicationUser> allUserNames = GetAllUserNamesFromDatabase();

                foreach (var item in uniqueNames)
                {
                    if (!item.Contains(" ") && !string.IsNullOrEmpty(item))
                    {
                        string userLookup = item.Replace('\\', '/');
                        userLookup = userLookup.Replace("CORP/", "");

                        if (userLookup.Substring(0, 1) == "u")
                            fullNamePairs.Add(item, allUserNames.Where(aun => aun.UserName == userLookup).Select(aun => aun.FullName).FirstOrDefault());
                        else
                            fullNamePairs.Add(item, item);
                    }
                    else
                        fullNamePairs.Add(item, item);
                }

                foreach (var item in fullNamePairs)
                {
                    list.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                }
            }

            return View(new GridModel(list));
        }

        public ActionResult DeleteDetail(int ID, string vendorNumber)
        {
            VendorGroupDetail det = db.VendorGroupDetails.Where(vgd => vgd.GroupID == ID && vgd.VendorNumber == vendorNumber).First();
            db.VendorGroupDetails.Remove(det);            

            VendorGroup group = db.VendorGroups.Where(vg => vg.ID == ID).First();
            group.Count--;
            db.SaveChanges();

            return RedirectToAction("Details", new { ID});
        }

        public ActionResult AddDetail(int ID, string vendorNumber)
        {
            string message = "";
            List<VendorGroupDetail> existing = db.VendorGroupDetails.Where(vgd => vgd.VendorNumber == vendorNumber).ToList();
            
            if (existing.Count() > 0)
            {
                int oldGroup = existing.First().GroupID;
                ViewData["OriginalGroup"] = db.VendorGroups.Where(vg => vg.ID == oldGroup).First().Name;
                ViewData["NewGroup"] = db.VendorGroups.Where(vg => vg.ID == ID).First().Name;

                //give them a confirmation screen about the move
                ViewData["GroupID"] = ID;
                ViewData["vendorNumber"] = vendorNumber;

                return View();                
            }
            else
            {
                try
                {
                    if (vendorGroupDetailDAO.IsVendorSetupForEDI(vendorNumber))
                    {
                        VendorGroupDetail det = new VendorGroupDetail()
                        {
                            GroupID = ID,
                            VendorNumber = vendorNumber,
                            CreateDate = DateTime.Now,
                            CreatedBy = currentUser.NetworkID
                        };

                        db.VendorGroupDetails.Add(det);

                        VendorGroup group = db.VendorGroups.Where(vg => vg.ID == ID).First();
                        group.Count += 1;
                        db.SaveChanges();
                    }
                    else                    
                        message = "Vendor must be setup for EDI before it can be added to a group.  Please email EDI.Support.";                    
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
                {
                    try
                    {
                        message = ex2.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;
                    }
                    catch
                    {
                        message = ex2.Message;
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }
            return RedirectToAction("Details", new { ID, message });
        }

        public ActionResult ConfirmMove(int ID, string vendorNumber)
        {
            VendorGroupDetail det = db.VendorGroupDetails.Where(vgd => vgd.VendorNumber == vendorNumber).First();
            VendorGroup group = db.VendorGroups.Where(vg => vg.ID == det.GroupID).First();
            group.Count--;
            db.SaveChanges();
            
            det.GroupID = ID;
            det.CreateDate = DateTime.Now;
            det.CreatedBy = currentUser.NetworkID;
            db.SaveChanges();

            group = db.VendorGroups.Where(vg => vg.ID == ID).First();
            group.Count++;
            db.SaveChanges();

            return RedirectToAction("Details", new { ID });
        }

        public ActionResult Edit(int ID)
        {
            VendorGroup group = db.VendorGroups.Where(vg => vg.ID == ID).First();

            return View(group);
        }

        [HttpPost]
        public ActionResult Edit(VendorGroup model)
        {
            model.CreateDate = DateTime.Now;
            model.CreatedBy = currentUser.NetworkID;

            db.Entry(model).State = System.Data.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ExcelTemplate(int ID)
        {
            VendorGroupSpreadsheet vendorGroupSpreadsheet = new VendorGroupSpreadsheet(appConfig, new ConfigService(), vendorGroupDetailDAO);
            Excel excelDocument;

            excelDocument = vendorGroupSpreadsheet.GetTemplate();

            excelDocument.Save("VendorGroupUpload.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);
            VendorGroupDetailsModel model = new VendorGroupDetailsModel()
            {
                HasDetails = db.VendorGroupDetails.Where(vgd => vgd.GroupID == ID).Any(),
                Header = db.VendorGroups.Where(vg => vg.ID == ID).FirstOrDefault()
            };
            return View("Details", model);
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments, int ID)
        {
            VendorGroupSpreadsheet vendorGroupSpreadsheet = new VendorGroupSpreadsheet(appConfig, new ConfigService(), vendorGroupDetailDAO);
            foreach (HttpPostedFileBase file in attachments)
            {
                vendorGroupSpreadsheet.Save(file, ID);

                if (vendorGroupSpreadsheet.errorList.Count() > 0)
                {
                    Session["errorList"] = vendorGroupSpreadsheet.errorList;
                    return Content(string.Format("{0} Errors on spreadsheet ({1} successfully uploaded)", vendorGroupSpreadsheet.errorList.Count(), vendorGroupSpreadsheet.validList.Count()));
                }
            }

            return Content("");
        }

        public ActionResult SeasonalityErrors()
        {
            VendorGroupSpreadsheet vendorGroupSpreadsheet = new VendorGroupSpreadsheet(appConfig, new ConfigService(), vendorGroupDetailDAO);
            Excel excelDocument;

            List<VendorGroupDetail> errorList = new List<VendorGroupDetail>();

            if (Session["errorList"] != null)
                errorList = (List<VendorGroupDetail>)Session["errorList"];

            excelDocument = vendorGroupSpreadsheet.GetErrors(errorList);
            excelDocument.Save("VendorGroupErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);

            return View();
        }
    }
}
