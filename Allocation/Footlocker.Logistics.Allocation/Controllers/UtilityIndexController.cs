using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [HandleAjaxValidationError(Order = 3)]
    [LogError(Order = 2)]
    [HandleAjaxError(Order = 1)]
    [HandleError(Order = 0)]
    [CheckPermission(Roles = "Director of Allocation,Admin,Support")]
    public class UtilityIndexController : AppController
    {
        #region HTTP GET Actions

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        #endregion

        #region JSON Actions

        // NOTE: Due to Telerik sending combobox ajax select as POST, we must have this as post rather than get
        [HttpPost]
        public ActionResult Ajax_FOBsByDivision(string division)
        {
            IList<FOB> fobs = null;
            using (var context = new DAO.AllocationContext())
            {
                // Get all fobs by division
                fobs = context.FOBs.Where(fob => string.Equals(fob.Division, division)).OrderBy(f => f.Description).ToList();
            }

            return new JsonResult() { Data = new SelectList(fobs, "ID", "Description") };
        }

        // NOTE: Due to Telerik sending combobox ajax select as POST, we must have this as post rather than get
        [HttpPost]
        public ActionResult Ajax_Divisions()
        {
            return new JsonResult() { 
                Data = new SelectList(currentUser.GetUserDivisions().OrderBy(d => d.DivisionName).ToList(), "DivCode", "DivisionName")
            };
        }

        #endregion
    }
}
