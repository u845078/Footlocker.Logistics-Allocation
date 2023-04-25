using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using Footlocker.Logistics.Allocation.Models;
using System.IO;
using System.Xml.Serialization;

namespace Footlocker.Logistics.Allocation.Services
{
    public class RangePlanDAO
    {
        public const int _DEFAULT_MAX_LEADTIME = 5;
        Database _database;
        private readonly AllocationLibraryContext db;

        public RangePlanDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
            db = new AllocationLibraryContext();
        }

        public void DeleteRangePlan(long planID)
        {
            DbCommand SQLCommand;
            string SQL = "DeleteRangePlanData";
            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@planID", DbType.Int64, planID);
            SQLCommand.CommandTimeout = 120;
            _database.ExecuteNonQuery(SQLCommand);
        }

        public void CopyRangePlan(string fromSKU, string toSKU, long toItemID, string toDescription, bool copyOPRequest, string userID)
        {
            DbCommand SQLCommand;
            string SQL = "CopyRangePlan";
            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@fromSKU", DbType.String, fromSKU);
            _database.AddInParameter(SQLCommand, "@toSKU", DbType.String, toSKU);
            _database.AddInParameter(SQLCommand, "@toItemID", DbType.Int64, toItemID);
            _database.AddInParameter(SQLCommand, "@toDescription", DbType.String, toDescription);
            _database.AddInParameter(SQLCommand, "@copyOPRequest", DbType.Boolean, copyOPRequest);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, userID);

            SQLCommand.CommandTimeout = 120;
            _database.ExecuteNonQuery(SQLCommand);
        }

        public long GetRangePlanID(string sku)
        {
            return db.RangePlans.Where(rp => rp.Sku == sku)
                .Select(rp => rp.Id).FirstOrDefault();
        }

        public RangePlan GetRangePlan(string sku)
        {
            return db.RangePlans.Where(rp => rp.Sku == sku).FirstOrDefault();
        }

        public RangePlan GetRangePlan(long planID)
        {
            return db.RangePlans.Where(rp => rp.Id == planID).FirstOrDefault();
        }

        /// <summary>
        /// This will update Delivery Groups and associated RangePlans, RangePlanDetails records
        /// </summary>
        /// <param name="deliveryGroups"></param>
        public void UpdateDeliveryGroups(List<DeliveryGroup> deliveryGroups, string user)
        {
            DbCommand SQLCommand;
            string SQL = "dbo.UpdateDeliveryGroups";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(deliveryGroups.GetType());
            xs.Serialize(sw, deliveryGroups);
            string xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xmlDetails", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, user);
            //SQLCommand.CommandTimeout = 300;

            _database.ExecuteNonQuery(SQLCommand);
        }

        public List<StoreLookup> GetStoreLookupsForPlan(long planID)
        {
            RangePlan p = db.RangePlans.Where(rp => rp.Id == planID).FirstOrDefault();
            string skuDivision = p.Sku.Substring(0, 2);

            List<StoreLookup> list = (from store in db.StoreLookups
                                      join det in db.RangePlanDetails
                                      on new { store.Division, store.Store } equals new { det.Division, det.Store }
                                      where det.ID == planID && det.Division == skuDivision
                                      select store).ToList();

            return list;
        }

        /// <summary>
        /// This will update the range plan update date/by fields and save the change
        /// </summary>
        /// <param name="planID"></param>
        /// <param name="userName"></param>
        public void UpdateRangePlanDate(long planID, string userName)
        {
            RangePlan p = db.RangePlans.Where(rp => rp.Id == planID).FirstOrDefault();
            p.UpdatedBy = userName;
            p.UpdateDate = DateTime.Now;
            db.SaveChanges(userName);
        }

        public void UpdateRangeHeader(long planID, WebUser user)
        {
            RangePlan p = db.RangePlans.Where(rp => rp.Id == planID).First();

            try
            {
                p.StartDate = (from a in db.DeliveryGroups
                               where a.PlanID == planID
                               select a.StartDate).Min();
            }
            catch
            { }

            try
            {
                p.EndDate = (from a in db.RangePlanDetails
                             where a.ID == planID
                             select a.EndDate).Max();
            }
            catch
            { }

            p.UpdateDate = DateTime.Now;
            p.UpdatedBy = user.NetworkID;
            db.SaveChanges(user.NetworkID);
        }

        public List<RangePlan> GetRangesForUser(WebUser webUser, string appName)
        {
            List<string> userDivDepts = webUser.GetUserDivDept(appName);
            List<string> divs = webUser.GetUserDivList(appName);

            var query = (from rp in db.RangePlans
                         join im in db.ItemMasters
                         on rp.ItemID equals im.ID
                         join di in divs
                         on im.Div equals di
                         join cd in db.ControlDates
                         on im.InstanceID equals cd.InstanceID
                         join opr in db.OrderPlanningRequests
                         on rp.Id equals opr.PlanID into opj
                         from outerjoin in opj.DefaultIfEmpty()
                         select new { RangePlan = rp, Division = im.Div, Department = im.Dept, cd.RunDate, outerjoin.StartSend, outerjoin.EndSend }).ToList();

            foreach (var queryRow in query)
            {
                queryRow.RangePlan.ActiveOP = false;

                if (queryRow.RangePlan.EvergreenSKU)
                    queryRow.RangePlan.ActiveOP = true;
                else
                {
                    if (queryRow.StartSend.HasValue && queryRow.EndSend.HasValue)
                    {
                        if (queryRow.RunDate >= queryRow.StartSend.Value && queryRow.RunDate <= queryRow.EndSend.Value)
                            queryRow.RangePlan.ActiveOP = true;
                    }
                }
            }

            List<RangePlan> model = query.Where(q => userDivDepts.Contains(q.Division + "-" + q.Department))
                                          .Select(q => q.RangePlan)
                                          .OrderBy(q => q.Sku)
                                          .ToList();

            return model;
        }
    }
}
