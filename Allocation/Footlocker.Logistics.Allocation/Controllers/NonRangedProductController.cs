using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class NonRangedProductController : AppController
    {
        #region HTTP GET Actions
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Telerik Grid Actions

        [GridAction]
        public ActionResult Grid_NonRangedProducts(string divCode)
        {
            // NOTE: Not donig a 'using' and disposing of context b/c we are allowing Telerik to enumerate to page
            var context = new DAO.AllocationContext();
            List<SqlParameter> parms = new List<SqlParameter>();

            // Construct stored proc non ranged items enumerable for Telerik to execute utilizing paging params
            var sprocSqlCmd = "GetNonRangedItems @Div";
            parms.Add(new SqlParameter("@Div", divCode));

            IEnumerable<ItemMasterDTO> nonRangedItems = context.Database.SqlQuery<ItemMasterDTO>(sprocSqlCmd, parms.ToArray()).ToList();
            
            // Return enumerable for Telerik handling code to execute
            return View(new GridModel(nonRangedItems));
        }

        #endregion

        #region JSON Actions

        // NOTE: Due to Telerik sending combobox ajax select as POST, we must have this as post rather than get
        [HttpPost]
        public ActionResult Ajax_Divisions()
        {
            return new JsonResult()
            {
                Data = new SelectList(currentUser.GetUserDivisions().OrderBy(d => d.DisplayName).ToList(), "DivCode", "DisplayName")
            };
        }

        #endregion
    }

    #region DTOs
    public class ItemMasterDTO
    {
        public long ItemID { get; set; }
        public string MerchantSku { get; set; }
        public string Description { get; set; }
    }
    #endregion
}