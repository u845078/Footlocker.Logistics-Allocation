using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class ConfigController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        [CheckPermission(Roles = "Support,IT")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        [CheckPermission(Roles = "Support,IT")]
        public ActionResult _Index()
        {
            List<Instance> list = db.Instances.ToList();
            return View(new GridModel(list));
        }

        [GridAction]
        public ActionResult _ConfigDetails(int instanceid)
        {
            List<Config> list = (from a in db.Configs.Include("ConfigParam") 
                                 where a.InstanceID == instanceid 
                                 select a).ToList();
            //workaround, worked on localhost before, but got circular reference error on server
            return View(new GridModel(list.Select(x => new { InstanceID = x.InstanceID,
                                                             ParamID = x.ParamID,
                                                             ParamName = x.ParamName,
                                                            Value = x.Value,
                                                            CreatedBy = x.CreatedBy,
                                                            CreateDate = x.CreateDate,
                                                            UpdatedBy = x.UpdatedBy,
                                                            UpdateDate = x.UpdateDate})));
        }

        public JsonResult _ConfigParam(int paramid)
        {
            ConfigParam c = (from a in db.ConfigParams where a.ParamID == paramid select a).First();
            //workaround, worked on localhost before, but got circular reference error on server
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) { Data = c.Comment } };
        }

        [CheckPermission(Roles = "IT")]
        public ActionResult Edit(int instanceid, int paramid)
        {
            EditConfigModel model = new EditConfigModel();

            model.Config = (from a in db.Configs 
                            where (a.InstanceID == instanceid) && 
                            (a.ParamID == paramid) 
                            select a).First();

            model.Param = (from a in db.ConfigParams 
                           where a.ParamID == paramid
                           select a).First();

            return View(model);
        }

        [CheckPermission(Roles = "IT")]
        [HttpPost]
        public ActionResult Edit(EditConfigModel model)
        {
            model.Config.UpdateDate = DateTime.Now;
            model.Config.UpdatedBy = UserName;
            db.Entry(model.Config).State = System.Data.EntityState.Modified;
            db.SaveChanges(UserName);

            return RedirectToAction("Index");
        }

        [CheckPermission(Roles = "IT")]
        public ActionResult Create(int instanceid)
        {
            CreateConfigModel mainModel = new CreateConfigModel();
            Config model = new Config();
            model.InstanceID = instanceid;
            mainModel.Config = model;
            mainModel.Params = (from a in db.ConfigParams select a).ToList();
            return View(mainModel);
        }

        [CheckPermission(Roles = "IT")]
        [HttpPost]
        public ActionResult Create(CreateConfigModel model)
        {
            model.Config.CreateDate = DateTime.Now;
            model.Config.CreatedBy = UserName;

            db.Configs.Add(model.Config);
            try
            {
                db.SaveChanges(UserName);
            }
            catch (Exception ex)
            {
                while (ex.Message.Contains("inner exception"))
                {
                    ex = ex.InnerException;
                }
                if (ex.Message.Contains("PRIMARY KEY"))
                {
                    ViewData["Message"] = "Config value already setup.  Please use Edit instead.";
                }
                else
                {
                    ViewData["Message"] = ex.Message;
                }
                model.Params = (from a in db.ConfigParams select a).ToList();
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [CheckPermission(Roles = "IT")]
        public ActionResult CreateParam()
        {
            CreateConfigParamModel model = new CreateConfigParamModel();
            model.Params = (from a in db.ConfigParams select a).ToList();
            model.Param = new ConfigParam();
            return View(model);
        }

        [CheckPermission(Roles = "IT")]
        [HttpPost]
        public ActionResult CreateParam(CreateConfigParamModel model)
        {

            db.ConfigParams.Add(model.Param);
            try
            {
                db.SaveChanges(UserName);
            }
            catch (Exception ex)
            {
                while (ex.Message.Contains("inner exception"))
                {
                    ex = ex.InnerException;
                }
                if (ex.Message.Contains("PRIMARY KEY"))
                {
                    ViewData["Message"] = "Config param already setup.";
                }
                else
                {
                    ViewData["Message"] = ex.Message;
                }
                model.Params = (from a in db.ConfigParams select a).ToList();
                return View(model);
            }
            return RedirectToAction("Index");
        }

    }
}
