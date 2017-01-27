
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Factories;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Common;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Models.Services
{
    public class RingFenceDAO
    {
        Database _database;
        Database _databaseEurope;
        AllocationLibraryContext db = new AllocationLibraryContext();

        public RingFenceDAO()
        {
            _database = DatabaseFactory.CreateDatabase("DB2PROD");
            _databaseEurope = DatabaseFactory.CreateDatabase("DB2EURP");
        }

        /// <summary>
        /// Returns a list of ring fences ordered by create date descending.
        /// </summary>
        /// <param name="validDivisions">A list of divisions that you want to match the ring fences against</param>
        /// <returns></returns>
        public List<RingFence> GetValidRingFences(List<Division> validDivisions)
        {
            List<RingFence> list = (from a in db.RingFences
                                    where a.Qty > 0
                                    select a).ToList();

            list = (from a in list
                    join d in validDivisions on a.Division equals d.DivCode
                    select a).OrderByDescending(x => x.CreateDate).ToList();

            return list;
        }

        public RingFenceDetail BuildFutureRingFenceDetail(string sku, string stockSizeNumber, string warehouseCode, 
            long currentRingfenceID, string poNumber, string priorityCode, List<RingFenceSummary> rfSummaryList, 
            int allocatableQty, DateTime expectedDeliveryDate)
        {
            RingFenceDetail det = new RingFenceDetail();
            DistributionCenter dc;
            int existingRingFenceQty = 0;
            int currentRingFenceQty = 0;

            det.Size = stockSizeNumber;
            dc = (from a in db.DistributionCenters
                  where a.MFCode == warehouseCode
                  select a).FirstOrDefault();

            if (dc != null)
            {
                det.Warehouse = dc.Name;
                det.DCID = dc.ID;
            }

            det.RingFenceID = currentRingfenceID;
            det.PO = poNumber;
            det.ActiveInd = "1";
            det.ringFenceStatusCode = "1";
            det.PriorityCode = priorityCode;
            //existingRingFenceQty = (from a in rfSummaryList
            //                        where ((a.Sku == sku) &&
            //                                (a.Size == det.Size) &&
            //                                (a.DC == warehouseCode) &&
            //                                (a.PO == det.PO))
            //                        select a.Qty).Sum();

            existingRingFenceQty = (from a in db.InventoryReductions
                                      where ((a.Sku == sku) &&
                                             (a.Size == det.Size) &&
                                             (a.MFCode == warehouseCode) &&
                                             (a.PO == det.PO))
                                      select a.Qty).DefaultIfEmpty(0).Sum();

            //if (existingRingFenceQty != myNewExistingRFQty)
            //    myNewExistingRFQty = 0;

            currentRingFenceQty = 0;

            currentRingFenceQty = (from a in db.RingFenceDetails
                                    where ((a.RingFenceID == det.RingFenceID) &&
                                            (a.Size == det.Size) &&
                                            (a.DCID == dc.ID) &&
                                            (a.PO == det.PO) &&
                                            (a.ActiveInd == "1"))
                                    select a.Qty).DefaultIfEmpty(0).Sum();

            det.AvailableQty = allocatableQty - existingRingFenceQty + currentRingFenceQty;
            det.DueIn = expectedDeliveryDate;
            return det;
        }

        public List<RingFenceDetail> GetFuturePOs(RingFence rf)
        {
            Database currDatabase=null;
            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(rf.Division))
            {
                currDatabase = _databaseEurope;
            }
            else
            {
                currDatabase = _database;
            }
            List<RingFenceDetail> _que;
            _que = new List<RingFenceDetail>();
            
            Int32 instanceid = (from a in db.InstanceDivisions
                                where a.Division == rf.Division
                                select a.InstanceID).First();

            DbCommand SQLCommand;
            string stock, color, dept;
            string[] tokens = rf.Sku.Split('-');
            dept = tokens[1];
            stock = tokens[2];
            color = tokens[3];

            string SQL = "select a.EXPECTED_DELV_DATE, a.retl_oper_div_code, a.PO_NUM,b.STATUS_IND,a.PRIORITY_CODE, ";
            SQL = SQL + "c.WHSE_ID_NUM,RTRIM(c.STK_SIZE_NUM) as STK_SIZE_NUM,SUM(c.ORDER_QTY - c.RECEIVED_QTY) as due_in ";
            SQL = SQL + " from tkpod001 a,tkpod003 b, tkpod005 c ";
            SQL = SQL + " where ";
            SQL = SQL + " a.retl_oper_div_code = b.retl_oper_div_code ";
            SQL = SQL + " and b.retl_oper_div_code = c.retl_oper_div_code ";
            SQL = SQL + " and a.PO_NUM = b.PO_NUM ";
            SQL = SQL + " and b.PO_NUM = c.PO_NUM ";
            SQL = SQL + " and b.STATUS_IND in (' ','P','R') ";
            SQL = SQL + " and b.STK_DEPT_NUM = c.STK_DEPT_NUM ";
            SQL = SQL + " and b.STK_NUM = c.STK_NUM  ";
            SQL = SQL + " and b.WDTH_COLOR_NUM = c.WDTH_COLOR_NUM ";
            SQL = SQL + " and a.RETL_OPER_DIV_CODE = '" + rf.Division + "'";
            SQL = SQL + " and b.STK_DEPT_NUM = '" + dept + "' ";
            SQL = SQL + " and b.STK_NUM = '" + stock + "' ";
            SQL = SQL + " and b.WDTH_COLOR_NUM = '" + color + "' ";
            SQL = SQL + " and c.WHSE_ID_NUM != '' ";
            SQL = SQL + " group by a.EXPECTED_DELV_DATE, a.retl_oper_div_code, a.PO_NUM,a.PRIORITY_CODE,b.STATUS_IND,c.WHSE_ID_NUM,c.STK_SIZE_NUM ";
            SQL = SQL + " having SUM(c.ORDER_QTY - c.RECEIVED_QTY) > 0 ";

            SQL = SQL + " UNION ALL ";

            SQL = SQL + " select a.EXPECTED_DELV_DATE, a.retl_oper_div_code, a.PO_NUM,b.STATUS_IND,a.PRIORITY_CODE, ";
            SQL = SQL + " c.WHSE_ID_NUM,c.CASELOT_NUMBER as STK_SIZE_NUM,SUM(c.ORDER_QTY - c.RECEIVED_QTY) as due_in ";
            SQL = SQL + " from tkpod001 a,tkpod003 b, tkpod007 c ";
            SQL = SQL + " where ";
            SQL = SQL + " a.retl_oper_div_code = b.retl_oper_div_code ";
            SQL = SQL + " and b.retl_oper_div_code = c.retl_oper_div_code ";
            SQL = SQL + " and a.PO_NUM = b.PO_NUM ";
            SQL = SQL + " and b.PO_NUM = c.PO_NUM ";
            SQL = SQL + " and b.STATUS_IND in (' ','P','R') ";
            SQL = SQL + " and b.STK_DEPT_NUM = c.STK_DEPT_NUM ";
            SQL = SQL + " and b.STK_NUM = c.STK_NUM  ";
            SQL = SQL + " and b.WDTH_COLOR_NUM = c.WDTH_COLOR_NUM ";
            SQL = SQL + " and a.RETL_OPER_DIV_CODE = '" + rf.Division + "'";
            SQL = SQL + " and b.STK_DEPT_NUM = '" + dept + "' ";
            SQL = SQL + " and b.STK_NUM = '" + stock + "' ";
            SQL = SQL + " and b.WDTH_COLOR_NUM = '" + color + "' ";
            SQL = SQL + " and c.WHSE_ID_NUM != '' ";
            SQL = SQL + " group by a.EXPECTED_DELV_DATE, a.retl_oper_div_code, a.PO_NUM,a.PRIORITY_CODE,b.STATUS_IND,c.WHSE_ID_NUM,c.CASELOT_NUMBER ";
            SQL = SQL + " having SUM(c.ORDER_QTY - c.RECEIVED_QTY) > 0 ";

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);

            DataSet data = new DataSet();
            data = currDatabase.ExecuteDataSet(SQLCommand);

            RingFenceDetail det;
            if (data.Tables.Count > 0)
            {
                List<DataRow> futureInventory = data.Tables[0].AsEnumerable().ToList();
                List<DataRow> newFutureInventory = new List<DataRow>();

                if (rf.ringFenceDetails.Count() > 0)
                {
                    foreach (RingFenceDetail rfd in rf.ringFenceDetails)
                    {
                        var newRow = data.Tables[0].AsEnumerable().Where(r => ((string)r["STK_SIZE_NUM"]) == rfd.Size &&
                                                                              ((string)r["PO_NUM"]) == rfd.PO &&
                                                                              ((string)r["WHSE_ID_NUM"]) == rfd.Warehouse);
                        newFutureInventory.AddRange(newRow);
                    }

                    futureInventory = newFutureInventory;
                }

                //RingFenceSummaryDAO summaryDAO = new RingFenceSummaryDAO();
                //List<RingFenceSummary> list = summaryDAO.GetRingFenceSummaries(Convert.ToString(instanceid));

                foreach (DataRow dr in futureInventory)
                {
                    det = BuildFutureRingFenceDetail(rf.Sku, Convert.ToString(dr["STK_SIZE_NUM"]),
                                 Convert.ToString(dr["WHSE_ID_NUM"]), rf.ID, Convert.ToString(dr["PO_NUM"]),
                                 Convert.ToString(dr["PRIORITY_CODE"]), null, Convert.ToInt32(dr["due_in"]),
                                 Convert.ToDateTime(dr["EXPECTED_DELV_DATE"]));

                    _que.Add(det);
                }
            }
            return _que;
        }
        
        public List<RingFenceDetail> GetTransloadPOs(RingFence rf)
        {            
            Int32 instanceid = (from a in db.InstanceDivisions
                                where a.Division == rf.Division
                                select a.InstanceID).First();

            List<RingFenceDetail> _que;
            _que = new List<RingFenceDetail>();
            Database database = DatabaseFactory.CreateDatabase("AllocationContext");

            DbCommand SQLCommand;
            string SQL = "dbo.[getTransloads]";

            SQLCommand = database.GetStoredProcCommand(SQL);
            database.AddInParameter(SQLCommand, "@sku", DbType.String, rf.Sku);

            DataSet data = new DataSet();
            data = database.ExecuteDataSet(SQLCommand);

            RingFenceDetail det;
            if (data.Tables.Count > 0)
            {
                List<DataRow> futureInventory = data.Tables[0].AsEnumerable().ToList();
                List<DataRow> newFutureInventory = new List<DataRow>();

                if (rf.ringFenceDetails.Count() > 0)
                {
                    foreach (RingFenceDetail rfd in rf.ringFenceDetails)
                    {
                        var newRow = data.Tables[0].AsEnumerable().Where(r => Convert.ToString(r["Size"]) == rfd.Size &&
                                                                              Convert.ToString(r["InventoryID"]).Split('-')[0] == rfd.PO &&
                                                                              Convert.ToString(r["Store"]) == rfd.Warehouse);
                        newFutureInventory.AddRange(newRow);
                    }

                    futureInventory = newFutureInventory;
                }

                //RingFenceSummaryDAO summaryDAO = new RingFenceSummaryDAO();
                //List<RingFenceSummary> list = summaryDAO.GetRingFenceSummaries(Convert.ToString(instanceid));

                foreach (DataRow dr in futureInventory)
                {
                    string availableDateString = Convert.ToString(dr["AvailableDate"]);
                    DateTime availableDate = Convert.ToDateTime(availableDateString.Substring(4, 2) + "-" +
                        availableDateString.Substring(6, 2) + "-" + availableDateString.Substring(0, 4));

                    det = BuildFutureRingFenceDetail(rf.Sku, Convert.ToString(dr["Size"]),
                                    Convert.ToString(dr["Store"]), rf.ID, 
                                    Convert.ToString(dr["InventoryID"]).Split('-')[0], "", null,
                                    Convert.ToInt32(dr["StockQty"]), availableDate);
                    _que.Add(det);
                }
            }
            return _que;
        }

        public List<RingFenceDetail> GetWarehouseAvailable(RingFence rf)
        {
            List<RingFenceDetail> _que = GetWarehouseAvailableCommon(rf.Sku, rf.Size, "", rf.ID);

            return _que;
        }

        public RingFenceDetail BuildCurrentRingFenceDetail(string sku, string stockSizeNumber, string warehouseCode, long? currentRingfenceID, List<RingFenceSummary> rfSummaryList, int allocatableQty)
        {
            RingFenceDetail det = new RingFenceDetail();
            DistributionCenter dc;
            int existingRingFenceQty = 0;
            int currentRingFenceQty = 0;

            det.Size = stockSizeNumber;
            dc = (from a in db.DistributionCenters
                  where a.MFCode == warehouseCode
                  select a).FirstOrDefault();

            if (dc != null)
            {
                det.Warehouse = dc.Name;
                det.DCID = dc.ID;
                if (currentRingfenceID != null)
                {
                    det.RingFenceID = Convert.ToInt64(currentRingfenceID);
                }
                det.PO = "";
                det.ActiveInd = "1";
                det.ringFenceStatusCode = "4";
                existingRingFenceQty = (from a in rfSummaryList
                                        where ((a.Sku == sku) &&
                                               (a.Size == det.Size) &&
                                               (a.DC == warehouseCode) &&
                                              ((a.PO == "N\\A") || (a.PO == "")))
                                        select a.Qty).Sum();

                currentRingFenceQty = (from a in db.RingFenceDetails
                                    where ((a.RingFenceID == det.RingFenceID) &&
                                           (a.Size == det.Size) &&
                                           (a.DCID == dc.ID) &&
                                           (a.ActiveInd == "1") &&
                                           ((a.PO == "N\\A") || (a.PO == "")))
                                    select a.Qty).DefaultIfEmpty(0).Sum();

                det.AvailableQty = allocatableQty - existingRingFenceQty + currentRingFenceQty;
                return det;
            }
            else
                return null;
        }

        /// <summary>
        /// This will find the warehouse available from the mainframe and reduce it by ringfenced quantities.
        /// </summary>
        /// <param name="sku">merchant sku 31-12-12345-00</param>
        /// <param name="size">size or caselot</param>
        /// <param name="warehouse">(optional) 2 digit mf code</param>
        /// <param name="currentRingfenceID">(optional) ID of ringfence, null if you aren't checking for a specific ringfence</param>
        /// <returns></returns>
        public List<RingFenceDetail> GetWarehouseAvailableCommon(string sku, string size, string warehouse, long? currentRingfenceID)
        {
            string stock, color, dept, div;
            string[] tokens = sku.Split('-');
            div = tokens[0];
            dept = tokens[1];
            stock = tokens[2];
            color = tokens[3];

            Int32 instanceid = (from a in db.InstanceDivisions
                                where a.Division == div
                                select a.InstanceID).First();

            RingFenceSummaryDAO summaryDAO = new RingFenceSummaryDAO();
            List<RingFenceSummary> list = summaryDAO.GetRingFenceSummaries(Convert.ToString(instanceid));

            Database currDatabase = null;
            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(div))
            {
                currDatabase = _databaseEurope;
            }
            else
            {
                currDatabase = _database;
            }
            List<RingFenceDetail> _que;
            _que = new List<RingFenceDetail>();

            DbCommand SQLCommand;
            string SQL = "select WHSE_ID_NUM,STK_SIZE_NUM,ALLOCATABLE_BS_QTY from TC052002 ";
            SQL = SQL + " where ";
            SQL = SQL + "retl_oper_div_cd = '" + div + "' ";
            SQL = SQL + "and stk_dept_num = '" + dept + "' ";
            SQL = SQL + "and stk_num = '" + stock + "' ";
            SQL = SQL + "and stk_wc_num = '" + color + "' ";
            if (warehouse != "")
            {
                SQL = SQL + "and WHSE_ID_NUM = '" + warehouse + "' ";
            }
            //SQL = SQL + "and WHSE_ID_NUM = '" + warehouse + "' ";
            //SQL = SQL + "and STK_SIZE_NUM = " + Convert.ToInt32(size);

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);

            DataSet data = new DataSet();
            data = currDatabase.ExecuteDataSet(SQLCommand);
            RingFenceDetail det;
            if (data.Tables.Count > 0)
            {                
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    det = BuildCurrentRingFenceDetail(sku, Convert.ToString(dr["STK_SIZE_NUM"]).PadLeft(3, '0'),
                        Convert.ToString(dr["WHSE_ID_NUM"]), currentRingfenceID, list,
                        Convert.ToInt32(dr["ALLOCATABLE_BS_QTY"]));

                    if (det != null)
                    {
                        _que.Add(det);
                    }
                }
            }

            //now pull caselots
            SQL = "select WHSE_ID_NUM,CL_SCHED_NUM,ALLOCATABLE_CL_QTY from TC052010 ";
            SQL = SQL + " where ";
            SQL = SQL + "retl_oper_div_cd = '" + div + "' ";
            SQL = SQL + "and stk_dept_num = '" + dept + "' ";
            SQL = SQL + "and stk_num = '" + stock + "' ";
            SQL = SQL + "and stk_wc_num = '" + color + "' ";
            if (warehouse != "")
            {
                SQL = SQL + "and WHSE_ID_NUM = '" + warehouse + "' ";
            }
            //SQL = SQL + "and WHSE_ID_NUM = '" + warehouse + "' ";
            //SQL = SQL + "and STK_SIZE_NUM = " + Convert.ToInt32(size);

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);

            data = currDatabase.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    det = BuildCurrentRingFenceDetail(sku, Convert.ToString(dr["CL_SCHED_NUM"]).PadLeft(5, '0'),
                        Convert.ToString(dr["WHSE_ID_NUM"]), currentRingfenceID, list,
                        Convert.ToInt32(dr["ALLOCATABLE_CL_QTY"]));

                    if (det != null)
                    {
                        _que.Add(det);
                    }
                }
            }
            return _que;
        }        

        public bool canUserUpdateRingFence(RingFence rf, string userName, out string errorMessage)
        {
            bool result = true;
            errorMessage = "";

            //TODO:  Do we want department level security???
            if (!(WebSecurityService.UserHasDivision(userName, "Allocation", rf.Division)))
            {
                result = false;
                errorMessage = "You do not have permission to ring fence for division " + rf.Division;
            }
            else if (!(WebSecurityService.UserHasDepartment(userName, "Allocation", rf.Division, rf.Department)))
            {
                errorMessage = "You do not have permission to ring fence for department " + rf.Department;
            }

            return result;
        }

        public bool isEcommWarehouse(string division, string store)
        {
            if (store == "00800")
                return true;
            else if (store == "00900" && division == "31")
                return true;
            else
            {
                return (from a in db.EcommWarehouses
                        where ((a.Division == division) &&
                               (a.Store == store))
                        select a).Count() > 0;
            }
        }

        public bool isValidRingFence(RingFence rf, string userName, out string errorMessage)
        {
            bool result = true;
            errorMessage = "";

            if (rf.Division != rf.Sku.Substring(0, 2))
            {
                errorMessage = "Invalid Sku, division does not match selection.";
                return false;
            }
            else
            {
                if (!canUserUpdateRingFence(rf, userName, out errorMessage))
                {
                    return false;
                }

                int count = (from a in db.ItemMasters
                             where (a.MerchantSku == rf.Sku)
                             select a).Count();
                if (count == 0)
                {
                    errorMessage = "Invalid Sku, does not exist";
                    return false;
                }
            }

            return result;
        }

        public void UpdateRingFence(RingFence rf, string user)
        {
            // not sure if we should keep setting these create columns
            rf.CreateDate = DateTime.Now;
            rf.CreatedBy = user;

            rf.LastModifiedDate = DateTime.Now;
            rf.LastModifiedUser = user;
            
            db.Entry(rf).State = EntityState.Modified;
            db.SaveChanges();

            RingFenceHistory history = new RingFenceHistory();
            history.RingFenceID = rf.ID;
            history.Qty = rf.Qty;
            history.Division = rf.Division;
            history.EndDate = rf.EndDate;
            history.Size = rf.Size;
            history.Sku = rf.Sku;
            history.StartDate = rf.StartDate;
            history.Store = rf.Store;
            history.Action = "Update";
            history.CreateDate = DateTime.Now;
            history.CreatedBy = user;
            db.RingFenceHistory.Add(history);
            db.SaveChanges();
        }

        public int GetRingFenceDetailTotalQuantity(long ringFenceID)
        {
            int newQty;

            newQty = (from a in db.RingFenceDetails
                      where a.RingFenceID == ringFenceID &&
                            a.ActiveInd == "1"
                      select a.Qty).Sum();

            return newQty;
        }

        public void UpdateRingFenceDetail(RingFenceDetail det, string user)
        {
            db.Entry(det).State = EntityState.Modified;
            db.SaveChanges();

            RingFence rf = (from a in db.RingFences
                            where a.ID == det.RingFenceID
                            select a).First();

            RingFenceHistory history = new RingFenceHistory();
            history.RingFenceID = det.RingFenceID;
            history.Division = rf.Division;
            history.Store = rf.Store;
            history.Sku = rf.Sku;
            history.Size = det.Size;
            history.DCID = det.DCID;
            history.PO = det.PO;
            history.Qty = det.Qty;
            history.StartDate = rf.StartDate;
            history.EndDate = rf.EndDate;
            history.Action = "Update Det";
            history.CreateDate = DateTime.Now;
            history.CreatedBy = user;
            db.RingFenceHistory.Add(history);
            db.SaveChanges();

            int newQty = GetRingFenceDetailTotalQuantity(det.RingFenceID);

            if (rf.Qty != newQty)
            {
                rf.Qty = newQty;
                UpdateRingFence(rf, user);
            }           
        }

        public void SetRingFenceHeaderQtyAndHistory(List<Int64> ringfences)
        {
            Database database = DatabaseFactory.CreateDatabase("AllocationContext");

            DbCommand SQLCommand;
            string SQL = "dbo.[SetRingFenceHeaderQty]";

            foreach (Int64 id in ringfences)
            {
                SQLCommand = database.GetStoredProcCommand(SQL);
                database.AddInParameter(SQLCommand, "@id", DbType.Int64, id);

                database.ExecuteNonQuery(SQLCommand);
            }
        }
    }
}