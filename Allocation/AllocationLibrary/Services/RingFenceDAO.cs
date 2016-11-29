
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Factories;
using Footlocker.Logistics.Allocation.Services;
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

            RingFenceSummaryDAO summaryDAO = new RingFenceSummaryDAO();
            Int32 instanceid = (from a in db.InstanceDivisions where a.Division == rf.Division select a.InstanceID).First();
            List<RingFenceSummary> list = summaryDAO.GetRingFenceSummaries(Convert.ToString(instanceid));

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
            int existingRingFenceQty = 0;
            int currentRingFenceQty = 0;
            string warehouse;
            if (data.Tables.Count > 0)
            {
                DistributionCenter dc;
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    det = new RingFenceDetail();
                    det.Size = Convert.ToString(dr["STK_SIZE_NUM"]);
                    warehouse = Convert.ToString(dr["WHSE_ID_NUM"]);
                    
                    dc = (from a in db.DistributionCenters where a.MFCode == warehouse select a).FirstOrDefault();
                    if (dc != null)
                    {
                        det.Warehouse = dc.Name;
                        det.DCID = dc.ID;
                    }
                    det.RingFenceID = rf.ID;
                    det.PO = Convert.ToString(dr["PO_NUM"]);
                    det.PriorityCode = Convert.ToString(dr["PRIORITY_CODE"]);
                    existingRingFenceQty = (from a in list where ((a.Sku == rf.Sku) && (a.Size == det.Size) && (a.DC == warehouse) && (a.PO == det.PO)) select a.Qty).Sum();

                    var currQtyQuery = (from a in db.RingFenceDetails
                                        where ((a.RingFenceID == det.RingFenceID) && 
                                               (a.Size == det.Size) &&
                                               (a.ActiveInd == "1"))
                                        select a.Qty);
                    if (currQtyQuery.Count() > 0)
                    {
                        currentRingFenceQty = currQtyQuery.Sum();
                    }
                    else
                    {
                        currentRingFenceQty = 0;
                    }

                    det.AvailableQty = Convert.ToInt32(dr["due_in"]) - existingRingFenceQty + currentRingFenceQty;
                    det.DueIn = Convert.ToDateTime(dr["EXPECTED_DELV_DATE"]);
                    _que.Add(det);
                }
            }
            return _que;
        }


        
        public List<RingFenceDetail> GetTransloadPOs(RingFence rf)
        {
            RingFenceSummaryDAO summaryDAO = new RingFenceSummaryDAO();
            Int32 instanceid = (from a in db.InstanceDivisions where a.Division == rf.Division select a.InstanceID).First();
            List<RingFenceSummary> list = summaryDAO.GetRingFenceSummaries(Convert.ToString(instanceid));

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
            int existingRingFenceQty = 0;
            int currentRingFenceQty = 0;
            string warehouse;
            if (data.Tables.Count > 0)
            {
                DistributionCenter dc;
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    det = new RingFenceDetail();
                    det.Size = Convert.ToString(dr["Size"]);
                    warehouse = Convert.ToString(dr["Store"]);

                    dc = (from a in db.DistributionCenters where a.MFCode == warehouse select a).FirstOrDefault();
                    if (dc != null)
                    {
                        det.Warehouse = dc.Name;
                        det.DCID = dc.ID;
                    }
                    det.RingFenceID = rf.ID;
                    det.PO = Convert.ToString(dr["InventoryID"]).Split('-')[0];
                    det.PriorityCode = "";
                    existingRingFenceQty = (from a in list where ((a.Sku == rf.Sku) && (a.Size == det.Size) && (a.DC == warehouse) && (a.PO == det.PO)) select a.Qty).Sum();

                    var currQtyQuery = (from a in db.RingFenceDetails
                                        where ((a.RingFenceID == det.RingFenceID) && 
                                               (a.Size == det.Size) &&
                                               (a.ActiveInd == "1"))
                                        select a.Qty);
                    if (currQtyQuery.Count() > 0)
                    {
                        currentRingFenceQty = currQtyQuery.Sum();
                    }
                    else
                    {
                        currentRingFenceQty = 0;
                    }

                    det.AvailableQty = Convert.ToInt32(dr["StockQty"]) - existingRingFenceQty + currentRingFenceQty;
                    string date = Convert.ToString(dr["AvailableDate"]);
                    det.DueIn = Convert.ToDateTime(date.Substring(4, 2) + "-" + date.Substring(6, 2) + "-" + date.Substring(0, 4));
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
            int existingRingFenceQty = 0;
            int currentRingFenceQty = 0;

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
            string tempWarehouse;
            RingFenceDetail det;
            if (data.Tables.Count > 0)
            {
                DistributionCenter dc;
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    det = new RingFenceDetail();
                    det.Size = Convert.ToString(dr["STK_SIZE_NUM"]).PadLeft(3, '0');
                    tempWarehouse = Convert.ToString(dr["WHSE_ID_NUM"]);
                    dc = (from a in db.DistributionCenters where a.MFCode == tempWarehouse select a).FirstOrDefault();
                    if (dc != null)
                    {
                        det.Warehouse = dc.Name;
                        det.DCID = dc.ID;
                        if (currentRingfenceID != null)
                        {
                            det.RingFenceID = Convert.ToInt64(currentRingfenceID);
                        }
                        det.PO = "";
                        existingRingFenceQty = (from a in list where ((a.Sku == sku) && (a.Size == det.Size) && (a.DC == tempWarehouse) && ((a.PO == "N\\A") || (a.PO == ""))) select a.Qty).Sum();
                        var currQtyQuery = (from a in db.RingFenceDetails
                                            where ((a.RingFenceID == det.RingFenceID) && 
                                                   (a.Size == det.Size) && 
                                                   (a.DCID == dc.ID) && 
                                                   (a.ActiveInd == "1") &&
                                                   ((a.PO == "N\\A") || (a.PO == "")))
                                            select a.Qty);
                        if (currQtyQuery.Count() > 0)
                        {
                            currentRingFenceQty = currQtyQuery.Sum();
                        }
                        else
                        {
                            currentRingFenceQty = 0;
                        }

                        det.AvailableQty = Convert.ToInt32(dr["ALLOCATABLE_BS_QTY"]) - existingRingFenceQty + currentRingFenceQty;
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
                DistributionCenter dc;
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    det = new RingFenceDetail();
                    det.Size = Convert.ToString(dr["CL_SCHED_NUM"]).PadLeft(5, '0');
                    tempWarehouse = Convert.ToString(dr["WHSE_ID_NUM"]);
                    dc = (from a in db.DistributionCenters where a.MFCode == tempWarehouse select a).FirstOrDefault();
                    if (dc != null)
                    {
                        det.Warehouse = dc.Name;
                        det.DCID = dc.ID;
                        if (currentRingfenceID != null)
                        {
                            det.RingFenceID = Convert.ToInt64(currentRingfenceID);
                        }
                        det.PO = "";
                        existingRingFenceQty = (from a in list where ((a.Sku == sku) && (a.Size == det.Size) && (a.DC == tempWarehouse) && ((a.PO == "N\\A") || (a.PO == ""))) select a.Qty).Sum();
                        var currQtyQuery = (from a in db.RingFenceDetails
                                            where ((a.RingFenceID == det.RingFenceID) && 
                                                   (a.Size == det.Size) && 
                                                   (a.DCID == dc.ID) && 
                                                   (a.ActiveInd == "1") &&
                                                   ((a.PO == "N\\A") || (a.PO == "")))
                                            select a.Qty);
                        if (currQtyQuery.Count() > 0)
                        {
                            currentRingFenceQty = currQtyQuery.Sum();
                        }
                        else
                        {
                            currentRingFenceQty = 0;
                        }

                        det.AvailableQty = Convert.ToInt32(dr["ALLOCATABLE_CL_QTY"]) - existingRingFenceQty + currentRingFenceQty;
                        _que.Add(det);
                    }
                }
            }
            return _que;
        }


        public void UpdateRingFence(RingFence rf, string user)
        {
            rf.CreateDate = DateTime.Now;
            rf.CreatedBy = user;
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

        public void UpdateRingFenceDetail(RingFenceDetail det, string user)
        {
            db.Entry(det).State = EntityState.Modified;
            db.SaveChanges();

            RingFenceHistory history = new RingFenceHistory();
            history.RingFenceID = det.RingFenceID;
            history.DCID = det.DCID;
            history.PO = det.PO;
            history.Qty = det.Qty;
            history.Action = "Update Det";
            history.CreateDate = DateTime.Now;
            history.CreatedBy = user;
            db.RingFenceHistory.Add(history);
            db.SaveChanges();

            RingFence rf = (from a in db.RingFences where a.ID == det.RingFenceID select a).First();            
            rf.Qty = (from a in db.RingFenceDetails
                      where a.RingFenceID == det.RingFenceID &&
                            a.ActiveInd == "1"
                      select a.Qty).Sum();
            rf.CreateDate = DateTime.Now;
            rf.CreatedBy = user;
            db.Entry(rf).State = EntityState.Modified;
            db.SaveChanges();

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