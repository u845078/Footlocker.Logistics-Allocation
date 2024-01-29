using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class ConfigController : AppController
    {
        readonly DAO.AllocationContext db = new DAO.AllocationContext();

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
            List<Config> list = db.Configs.Include("ConfigParam").Where(c => c.InstanceID == instanceid).ToList();

            List<string> uniqueNames = (from l in list
                                        select l.CreatedBy).Distinct().ToList();

            List<string> uniqueNames2 = (from l in list
                                         where !string.IsNullOrEmpty(l.UpdatedBy)
                                        select l.UpdatedBy).Distinct().ToList();

            Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

            foreach (var item in uniqueNames)
            {
                fullNamePairs.Add(item, getFullUserNameFromDatabase(item.Replace('\\', '/')));
            }

            foreach (var item in uniqueNames2)
            {
                if (!fullNamePairs.ContainsKey(item))
                    fullNamePairs.Add(item, getFullUserNameFromDatabase(item.Replace('\\', '/')));
            }

            foreach (var item in fullNamePairs)
            {
                list.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                list.Where(x => x.UpdatedBy == item.Key).ToList().ForEach(y => y.UpdatedBy = item.Value);
            }

            //workaround, worked on localhost before, but got circular reference error on server
            return View(new GridModel(list.Select(x => new { x.InstanceID,
                                                             x.ParamID,
                                                             x.ParamName,
                                                             x.Value,
                                                             x.CreatedBy,
                                                             x.CreateDate,
                                                             x.UpdatedBy,
                                                             x.UpdateDate })));
        }

        public JsonResult _ConfigParam(int paramid)
        {
            ConfigParam c = db.ConfigParams.Where(cp => cp.ParamID == paramid).First();

            //workaround, worked on localhost before, but got circular reference error on server
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) { Data = c.Comment } };
        }

        [CheckPermission(Roles = "IT")]
        public ActionResult Edit(int instanceid, int paramid)
        {
            EditConfigModel model = new EditConfigModel()
            {
                Config = db.Configs.Where(c => c.InstanceID == instanceid && c.ParamID == paramid).First(),
                Param = db.ConfigParams.Where(cp => cp.ParamID == paramid).First()
            };

            return View(model);
        }

        [CheckPermission(Roles = "IT")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditConfigModel model)
        {
            model.Config.UpdateDate = DateTime.Now;
            model.Config.UpdatedBy = currentUser.NetworkID;
            db.Entry(model.Config).State = System.Data.EntityState.Modified;
            db.SaveChanges(currentUser.NetworkID);

            return RedirectToAction("Index");
        }

        [CheckPermission(Roles = "IT")]
        public ActionResult Create(int instanceid)
        {            
            Config model = new Config()
            {
                InstanceID = instanceid
            };

            CreateConfigModel mainModel = new CreateConfigModel()
            {
                Config = model,
                Params = db.ConfigParams.ToList()
            };
                        
            return View(mainModel);
        }

        [CheckPermission(Roles = "IT")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateConfigModel model)
        {
            model.Config.CreateDate = DateTime.Now;
            model.Config.CreatedBy = currentUser.NetworkID;

            db.Configs.Add(model.Config);
            try
            {
                db.SaveChanges(currentUser.NetworkID);
            }
            catch (Exception ex)
            {
                while (ex.Message.Contains("inner exception"))
                {
                    ex = ex.InnerException;
                }
                if (ex.Message.Contains("PRIMARY KEY"))
                {
                    ViewData["Message"] = "Config value already set up. Please use Edit instead.";
                }
                else
                {
                    ViewData["Message"] = ex.Message;
                }

                model.Params = db.ConfigParams.ToList();
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [CheckPermission(Roles = "IT")]
        public ActionResult CreateParam()
        {
            CreateConfigParamModel model = new CreateConfigParamModel()
            {
                Params = db.ConfigParams.ToList(),
                Param = new ConfigParam()
            };

            return View(model);
        }

        [CheckPermission(Roles = "IT")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateParam(CreateConfigParamModel model)
        {
            db.ConfigParams.Add(model.Param);
            try
            {
                db.SaveChanges(currentUser.NetworkID);
            }
            catch (Exception ex)
            {
                while (ex.Message.Contains("inner exception"))
                {
                    ex = ex.InnerException;
                }

                if (ex.Message.Contains("PRIMARY KEY"))                
                    ViewData["Message"] = "Config param already setup.";                
                else                
                    ViewData["Message"] = ex.Message;                

                model.Params = db.ConfigParams.ToList();
                return View(model);
            }
            return RedirectToAction("Index");
        }
    }
}
