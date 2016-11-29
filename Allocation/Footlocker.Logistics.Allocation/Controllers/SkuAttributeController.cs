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
    public class SkuAttributeController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index()
        {
            List<SkuAttributeHeader> headers = (from a in db.SkuAttributeHeaders where 1==1 orderby a.Division, a.Dept, a.Category select a).ToList();
            List<Division> divisions = this.Divisions();
            return View((from a in headers join b in divisions on a.Division equals b.DivCode select a).ToList());
        }

        [GridAction]
        public ActionResult _Index()
        {
            List<SkuAttributeHeader> headers = (from a in db.SkuAttributeHeaders where 1 == 1 orderby a.Division, a.Dept, a.Category select a).ToList();
            List<Division> divisions = this.Divisions();
            return View(new GridModel((from a in headers join b in divisions on a.Division equals b.DivCode select a).ToList()));
            //return View((from a in headers join b in divisions on a.Division equals b.DivCode select a).ToList());
        }


        [CheckPermission(Roles = "Director of Allocation,VP of Allocation,Admin,Support,Advanced Merchandiser Processes")]
        public ActionResult Create(string div)
        {
            SkuAttributeModel model = new SkuAttributeModel();

            model.Divisions = Divisions();
            if ((div == null) || (div == ""))
            {
                div = model.Divisions[0].DivCode;
            }
            model.Division = div;
            model.Departments = DepartmentService.ListDepartments(div);
            model.WeightActive = 100;

            model.Attributes = new List<SkuAttributeDetail>();
            model.Attributes.Add(new SkuAttributeDetail("BrandID", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Category", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("color1", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("color2", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("color3", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Department", true, 0));
            model.Attributes.Add(new SkuAttributeDetail("Gender", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("LifeOfSku", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Material", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Size", true, 0));
            model.Attributes.Add(new SkuAttributeDetail("SizeRange", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Skuid1", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Skuid2", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Skuid3", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Skuid4", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Skuid5", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("TeamCode", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("VendorNumber", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("PlayerID", false, 0));

            model.Attributes = (from a in model.Attributes orderby a.SortOrder, a.AttributeType ascending select a).ToList();


            return View(model);
        }

        [CheckPermission(Roles = "Director of Allocation,VP of Allocation,Admin,Support,Advanced Merchandiser Processes")]
        [HttpPost]
        public ActionResult Create(SkuAttributeModel model)
        {
            SkuAttributeHeader header = new SkuAttributeHeader();
            var existing = (from a in db.SkuAttributeHeaders where ((a.Division == model.Division) && (a.Dept == model.Department) && ((a.Category == model.Category) || ((a.Category == null) && (model.Category == null)))) select a);
            if (existing.Count() > 0)
            {
                ViewData["message"] = "This department/category is already setup, please use go Back to List and use Edit.";
                model.Divisions = Divisions();
                model.Departments = DepartmentService.ListDepartments(model.Division);
                return View(model);
            }

            List<SkuAttributeDetail> details = new List<SkuAttributeDetail>();
            header.Division = model.Division;
            int total=0;
            if (Footlocker.Common.WebSecurityService.UserHasDivision(UserName, "Allocation", model.Division))
            {

                header.Dept = model.Department;
                header.Category = model.Category;
                header.CreatedBy = User.Identity.Name;
                header.CreateDate = DateTime.Now;
                header.WeightActiveInt = model.WeightActive;

                foreach (SkuAttributeDetail det in model.Attributes)
                {
                    total += det.WeightInt;
                    details.Add(det);
                }

                if ((total == 100)||(total==0))
                {
                    db.SkuAttributeHeaders.Add(header);
                    db.SaveChanges();
                    foreach (SkuAttributeDetail det in details)
                    {
                        det.HeaderID = header.ID;
                        db.SkuAttributeDetails.Add(det);
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    model.Divisions = Divisions();
                    model.Departments = DepartmentService.ListDepartments(model.Division);
                    model.Message = "Total must equal 100, it was " + total;
                    return View(model);
                }
            }
            else
            {
                model.Divisions = Divisions();
                model.Departments = DepartmentService.ListDepartments(model.Division);
                model.Message = "You are not authorized for division " + model.Division;
                return View(model);
            }
        }

        [CheckPermission(Roles = "Director of Allocation,VP of Allocation,Admin,Support,Advanced Merchandiser Processes")]
        public ActionResult Edit(int ID)
        {
            SkuAttributeModel model = new SkuAttributeModel();
            SkuAttributeHeader header = (from a in db.SkuAttributeHeaders where a.ID == ID select a).First();

            string div = header.Division;

            model.Divisions = Divisions();
            model.Division = div;
            model.Departments = DepartmentService.ListDepartments(div);
            model.Department = header.Dept;
            model.Category = header.Category;
            model.WeightActive = header.WeightActiveInt;

            model.Attributes = (from a in db.SkuAttributeDetails where a.HeaderID == header.ID select a).ToList();
            model.Attributes = (from a in model.Attributes orderby a.SortOrder, a.AttributeType ascending select a).ToList();

            return View(model);
        }

        [CheckPermission(Roles = "Director of Allocation,VP of Allocation,Admin,Support,Advanced Merchandiser Processes")]
        [HttpPost]
        public ActionResult Edit(SkuAttributeModel model)
        {

            if (WebSecurityService.UserHasDepartment(UserName, "Allocation", model.Division, model.Department))
            {
                int total = 0;
                total = model.Attributes.Sum(a => a.WeightInt);
           
                if ((total == 100)||(total == 0))
                {
                    foreach (SkuAttributeDetail det in model.Attributes)
                    {
                        db.Entry(det).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                    }

                    SkuAttributeHeader header = (from a in db.SkuAttributeHeaders where ((a.Division == model.Division) && (a.Dept == model.Department) && ((a.Category == model.Category) || ((a.Category == null)&&(model.Category==null)))) select a).First();
                    header.WeightActiveInt = model.WeightActive;
                    header.CreatedBy = User.Identity.Name;
                    header.CreateDate = DateTime.Now;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                else
                {
                    model.Divisions = Divisions();
                    model.Departments = DepartmentService.ListDepartments(model.Division);
                    model.Message = "Total must equal 100, it was " + total;
                    return View(model);
                }
            }
            else
            {
                model.Divisions = Divisions();
                model.Departments = DepartmentService.ListDepartments(model.Division);
                model.Message = "You don't have permission to update this division/dept.";
                return View(model);
            }

        }

        [CheckPermission(Roles = "Director of Allocation,VP of Allocation,Admin,Support,Advanced Merchandiser Processes")]
        public ActionResult Delete(int ID)
        {
            SkuAttributeHeader header = (from a in db.SkuAttributeHeaders where a.ID == ID select a).First();
            db.SkuAttributeHeaders.Remove(header);
            db.SaveChanges();
            var query = (from a in db.SkuAttributeDetails where a.HeaderID == ID select a);
            foreach (SkuAttributeDetail det in query.ToList())
            {
                db.SkuAttributeDetails.Remove(det);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

    }
}
