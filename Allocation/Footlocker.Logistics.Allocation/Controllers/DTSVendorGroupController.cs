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

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Support,Logistics")]
    public class DTSVendorGroupController : AppController
    {
        AllocationContext db = new AllocationContext();

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
            VendorGroupDetailDAO dao = new VendorGroupDetailDAO();

            List<VendorGroupDetail> list = dao.GetVendorGroupDetails(ID);

            return View(new GridModel(list));
        }

        [GridAction]
        public ActionResult _RefreshLeadTimeGrid(int ID, string message)
        {
            List<VendorGroupLeadTime> list = (from a in db.VendorGroupLeadTimes where a.VendorGroupID == ID select a).ToList();
            return View(new GridModel(list));
        }

        public ActionResult DeleteDetail(int ID, string vendorNumber)
        {
            VendorGroupDetail det = db.VendorGroupDetails.Where(vgd => vgd.GroupID == ID && vgd.VendorNumber == vendorNumber).First();
            db.VendorGroupDetails.Remove(det);            

            VendorGroup group = db.VendorGroups.Where(vg => vg.ID == ID).First();
            group.Count = group.Count - 1;
            db.SaveChanges();

            return RedirectToAction("Details", new { ID = ID});
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
                    VendorGroupDetailDAO dao = new VendorGroupDetailDAO();
                    if (dao.IsVendorSetupForEDI(vendorNumber))
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
                    {
                        message = "Vendor must be setup for EDI before it can be added to a group.  Please email EDI.Support.";
                    }
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
            VendorGroupDetail det = (from a in db.VendorGroupDetails where (a.VendorNumber == vendorNumber) select a).First();
            VendorGroup group = (from a in db.VendorGroups where a.ID == det.GroupID select a).First();
            group.Count = group.Count - 1;
            db.SaveChanges();

            
            det.GroupID = ID;
            det.CreateDate = DateTime.Now;
            det.CreatedBy = currentUser.NetworkID;
            db.SaveChanges();

            group = (from a in db.VendorGroups where a.ID == ID select a).First();
            group.Count = group.Count + 1;
            db.SaveChanges();

            return RedirectToAction("Details", new { ID = ID });
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

        public ActionResult EditLeadTime(int ID, int ZoneID)
        {
            VendorGroupLeadTime group = (from a in db.VendorGroupLeadTimes where ((a.ZoneID == ZoneID) && (a.VendorGroupID == ID)) select a).First();

            return View(group);
        }

        [HttpPost]
        public ActionResult EditLeadTime(VendorGroupLeadTime model)
        {
            db.Entry(model).State = System.Data.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Details", new { ID = model.VendorGroupID });
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments, int ID)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            List<VendorGroupDetail> errors = new List<VendorGroupDetail>();
            int errorCount = 0;
            int addedCount = 0;
            VendorGroupDetailDAO dao = new VendorGroupDetailDAO();

            foreach (HttpPostedFileBase file in attachments)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];
                int row = 1;
                string vendornumber="";
                if (Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Vendor"))
                {
                    while (mySheet.Cells[row, 0].Value != null)
                    {
                        vendornumber = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(5,'0');
                        var existing = (from a in db.VendorGroupDetails where (a.VendorNumber == vendornumber) select a);
                        if (existing.Count() > 0)
                        {
                            errorCount++;
                            VendorGroupDetail vgd = existing.First();
                            vgd.ErrorMessage = "Already in group " + vgd.GroupID;
                            errors.Add(vgd);
                            try {
                                vgd.ErrorMessage = "Already in group " + (from a in db.VendorGroups where a.ID == vgd.GroupID select a).First().Name;
                            }
                            catch {
                            //if anything goes wrong with that sql, just ignore it.  The error will have the ID instead of the name
                            }
                        }
                        else if (!(dao.IsVendorSetupForEDI(vendornumber)))
                        {
                            errorCount++;
                            VendorGroupDetail det = new VendorGroupDetail();
                            det.GroupID = ID;
                            det.VendorNumber = vendornumber;
                            det.ErrorMessage = "Vendor must be setup for EDI before it can be added to a group.  Please email EDI.Support.";
                            errors.Add(det);
                        }
                        else
                        {
                            VendorGroupDetail det = new VendorGroupDetail();
                            try
                            {
                                det.GroupID = ID;
                                det.VendorNumber = vendornumber;
                                det.CreateDate = DateTime.Now;
                                det.CreatedBy = User.Identity.Name;
                                db.VendorGroupDetails.Add(det);
                                db.SaveChanges();

                                addedCount++;
                            }
                            catch (Exception ex)
                            {
                                errorCount++;
                                det.ErrorMessage = ex.Message;
                                errors.Add(det);
                            }
                        }

                        row++;
                    }

                    VendorGroup vg = (from a in db.VendorGroups where a.ID == ID select a).First();
                    vg.Count = vg.Count + addedCount;
                    db.SaveChanges();

                }
                else
                {
                    return Content("Incorrect header, first column must be Vendor.");
                }
            }
            if (errors.Count > 0)
            {
                Session["errorList"] = errors;
                return Content(errorCount + " Errors on spreadsheet (" + addedCount + " successfully uploaded)");

            }
            else
            {
                return Content("");
            }
        }

        public ActionResult SeasonalityErrors()
        {
            List<VendorGroupDetail> errorList = new List<VendorGroupDetail>();
            if (Session["errorList"] != null)
            {
                errorList = (List<VendorGroupDetail>)Session["errorList"];
            }

            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            Worksheet mySheet = excelDocument.Worksheets[0];
            int row = 1;
            mySheet.Cells[0, 0].PutValue("VendorNumber");
            mySheet.Cells[0, 1].PutValue("ErrorMessage");
            foreach (VendorGroupDetail p in errorList)
            {
                mySheet.Cells[row, 0].PutValue(p.VendorNumber);
                mySheet.Cells[row, 1].PutValue(p.ErrorMessage);

                row++;
            }

            excelDocument.Save("VendorGroupErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }
    }
}
