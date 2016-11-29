using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class InvalidTransactionController : AppController
    {
        #region HTTP GET Actions

        public ActionResult Index()
        {
            return View();
        }

        #endregion

        #region Telerik Grid Actions

        [GridAction]
        public ActionResult Grid_ProductAgg(string divCode)
        {
            // NOTE: Not donig a 'using' and disposing of context b/c we are allowing Telerik to enumerate to page
            var context = new DAO.AllocationContext();

            var invalidTransactions = context.InvalidTransactions
                                        .Include("Item")
                                        .GroupBy(t => new { DivCode = t.Item.Div, ItemID = t.ItemID, MerchantSku = t.Item.MerchantSku })
                                        .Where(g => g.Key.DivCode == divCode)
                                        .Select(t => new ProductInvTransAgg() {
                                            TotalQty = t.Sum(it => it.TotalQty),
                                            TotalRetail = t.Sum(it => it.TotalRetail),
                                            ItemID = t.Key.ItemID,
                                            MerchantSku = t.Key.MerchantSku
                                        })
                                        .OrderByDescending(agg => agg.TotalQty);

            return View(new GridModel(invalidTransactions));
        }

        [GridAction]
        public ActionResult Grid_LocationAgg(string divCode)
        {
            // NOTE: Not donig a 'using' and disposing of context b/c we are allowing Telerik to enumerate to page
            var context = new DAO.AllocationContext();

            var invalidTransactions = context.InvalidTransactions
                                        .Include("Location")
                                        .GroupBy(t => new { 
                                            DivCode = t.Location.Division, 
                                            LocationID = t.LocationID, 
                                            Store = t.Location.Store ,
                                            StoreDivCode = t.LocationID.Substring(0, 2)
                                        })
                                        .Where(g => g.Key.DivCode == divCode)
                                        .Select(t => new LocationInvTransAgg() {
                                            TotalQty = t.Sum(it => it.TotalQty),
                                            TotalRetail = t.Sum(it => it.TotalRetail),
                                            LocationID = t.Key.LocationID,
                                            Store = t.Key.Store,
                                            StoreDivCode = t.Key.StoreDivCode
                                        })
                                        .OrderByDescending(agg => agg.TotalQty);

            return View(new GridModel(invalidTransactions));
        }
        


        [GridAction]
        public ActionResult Grid_TransactionsByProduct(long itemID)
        {
            // NOTE: Not donig a 'using' and disposing of context b/c we are allowing Telerik to enumerate to page
            var context = new DAO.AllocationContext();

            var invalidTransactions = context.InvalidTransactions
                                        .Include("Item")
                                        .Include("Location")
                                        .Where(it => it.ItemID == itemID)
                                        .GroupBy(t => new
                                        {
                                            DivCode = t.Item.Div,
                                            ItemID = t.ItemID,
                                            MerchantSku = t.Item.MerchantSku,
                                            LocationID = t.LocationID,
                                            Store = t.Location.Store,
                                            StoreDivCode = t.LocationID.Substring(0, 2)
                                        })
                                        .Select(t => new ProductLocationInvTransAgg()
                                        {
                                            ItemID = t.Key.ItemID,
                                            MerchantSku = t.Key.MerchantSku,
                                            LocationID = t.Key.LocationID,
                                            Store = t.Key.Store,
                                            StoreDivCode = t.Key.StoreDivCode,
                                            TotalQty = t.Sum(it => it.TotalQty),
                                            TotalRetail = t.Sum(it => it.TotalRetail)
                                        })
                                        .OrderByDescending(agg => agg.TotalQty);

            return View(new GridModel(invalidTransactions));
        }

        [GridAction]
        public ActionResult Grid_TransactionsByLocation(string locationID)
        {
            // NOTE: Not donig a 'using' and disposing of context b/c we are allowing Telerik to enumerate to page
            var context = new DAO.AllocationContext();

            var invalidTransactions = context.InvalidTransactions
                                        .Include("Item")
                                        .Include("Location")
                                        .Where(it => it.LocationID == locationID)
                                        .GroupBy(t => new
                                        {
                                            DivCode = t.Item.Div,
                                            ItemID = t.ItemID,
                                            MerchantSku = t.Item.MerchantSku,
                                            LocationID = t.LocationID,
                                            Store = t.Location.Store,
                                            StoreDivCode = t.LocationID.Substring(0, 2)
                                        })
                                        .Select(t => new ProductLocationInvTransAgg()
                                        {
                                            ItemID = t.Key.ItemID,
                                            MerchantSku = t.Key.MerchantSku,
                                            LocationID = t.Key.LocationID,
                                            Store = t.Key.Store,
                                            StoreDivCode = t.Key.StoreDivCode,
                                            TotalQty = t.Sum(it => it.TotalQty),
                                            TotalRetail = t.Sum(it => it.TotalRetail)
                                        })
                                        .OrderByDescending(agg => agg.TotalQty);

            return View(new GridModel(invalidTransactions));
        }
        
        [GridAction]
        public ActionResult Grid_TransactionsByProductLocation(long itemID, string locationID)
        {
            // NOTE: Not donig a 'using' and disposing of context b/c we are allowing Telerik to enumerate to page
            var context = new DAO.AllocationContext();

            var invalidTransactions = context.InvalidTransactions
                                        .Include("Item")
                                        .Include("Location")
                                        .Where(it => it.ItemID == itemID && it.LocationID == locationID)
                                        .GroupBy(t => new
                                        {
                                            DivCode = t.Item.Div,
                                            ItemID = t.ItemID,
                                            MerchantSku = t.Item.MerchantSku,
                                            SessionID = t.SessionID,
                                            TransactionDate = t.TransDt,
                                            LocationID = t.LocationID,
                                            Store = t.Location.Store,
                                            StoreDivCode = t.LocationID.Substring(0, 2)
                                        })
                                        .Select(t => new DatedProductLocationInvTransAgg()
                                        {
                                            TotalQty = t.Sum(it => it.TotalQty),
                                            TotalRetail = t.Sum(it => it.TotalRetail),
                                            ItemID = t.Key.ItemID,
                                            MerchantSku = t.Key.MerchantSku,
                                            SessionID = t.Key.SessionID,
                                            TransactionDate = t.Key.TransactionDate,
                                            LocationID = t.Key.LocationID,
                                            Store = t.Key.Store,
                                            StoreDivCode = t.Key.StoreDivCode
                                        })
                                        .OrderByDescending(agg => agg.TotalQty);

            return View(new GridModel(invalidTransactions));
        }

        #endregion

        #region JSON Actions

        // NOTE: Due to Telerik sending combobox ajax select as POST, we must have this as post rather than get
        [HttpPost]
        public ActionResult Ajax_Divisions()
        {
            return new JsonResult()
            {
                Data = new SelectList(Divisions().OrderBy(d => d.DisplayName).ToList(), "DivCode", "DisplayName")
            };
        }

        #endregion
    }

    #region DTOs

    public class TransactionAggBase
    {
        public int TotalQty { get; set; }
        public decimal TotalRetail { get; set; }
    }

    public class ProductInvTransAgg : TransactionAggBase
    {
        public long ItemID { get; set; }
        public string MerchantSku { get; set; }
    }

    public class LocationInvTransAgg : TransactionAggBase
    {
        public string LocationID { get; set; }
        public string StoreDivCode { get; set; }
        public string Store { get; set; }
    }

    public class ProductLocationInvTransAgg : ProductInvTransAgg
    {
        public string LocationID { get; set; }
        public string StoreDivCode { get; set; }
        public string Store { get; set; }
    }

    public class DatedProductLocationInvTransAgg : ProductLocationInvTransAgg
    {
        public int SessionID { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    #endregion
}