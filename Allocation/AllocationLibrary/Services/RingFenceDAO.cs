
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
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
        private Repository<ItemMaster> skuRespository;
        public List<DistributionCenter> distributionCenters = new List<DistributionCenter>();        

        public RingFenceDAO()
        {
            _database = DatabaseFactory.CreateDatabase("DB2PROD");
            _databaseEurope = DatabaseFactory.CreateDatabase("DB2EURP");
            skuRespository = new Repository<ItemMaster>(new AllocationLibraryContext());
            distributionCenters = db.DistributionCenters.ToList();
        }

        /// <summary>
        /// Returns a list of ring fences ordered by create date descending.
        /// </summary>
        /// <param name="validDivisions">A list of divisions that you want to match the ring fences against</param>
        /// <returns></returns>
        public List<RingFence> GetValidRingFences(List<string> validDivisions)
        {
            List<RingFence> rfList = db.RingFences.Where(rf => rf.Qty > 0 && validDivisions.Contains(rf.Division)).ToList();

            return rfList.OrderByDescending(x => x.CreateDate).ToList();
        }

        public List<GroupedRingFence> GetValidRingFenceGroups(List<string> validDivisions)
        {
            List<GroupedRingFence> rfList = db.GroupedRingFences.Where(rf => rf.TotalQuantity > 0 && 
                                                                             validDivisions.Contains(rf.Division)).ToList();

            return rfList.OrderByDescending(x => x.CreateDate).ToList();
        }

        /// <summary>
        /// Use this to calculate the value for a ring fence header quantity
        /// </summary>
        /// <param name="uniqueCaselotNameQtys">This is a tuple with the caselot name and the total quantity. Item1 => the ItemPack's name (caselot size i.e. "00009"). Item2 => the ItemPack's TotalQty
        /// </param>
        /// <param name="ringFenceDetails">All the ring fence detail records for a header</param>
        /// <returns></returns>
        public int CalculateHeaderQty(List<Tuple<string, int>> uniqueCaselotNameQtys, List<RingFenceDetail> ringFenceDetails)
        {
            int totalQuantity = 0;
            foreach (var rfd in ringFenceDetails)
            {
                if (rfd.ActiveInd.Equals("1"))
                {
                    if (rfd.Size.Length.Equals(5))
                    {                        
                        var caselotDetail = uniqueCaselotNameQtys.Where(ucq => ucq.Item1 == rfd.Size).FirstOrDefault();
                        
                        if (caselotDetail != null)                        
                            totalQuantity += rfd.Qty * caselotDetail.Item2;                        
                    }
                    else                    
                        totalQuantity += rfd.Qty;                    
                }
            }
            return totalQuantity;
        }

        public RingFenceDetail BuildFutureRingFenceDetail(string sku, string stockSizeNumber, string warehouseCode, 
            long currentRingfenceID, string poNumber, string priorityCode, //List<RingFenceSummary> rfSummaryList, 
            int allocatableQty, DateTime expectedDeliveryDate, List<InventoryReductions> reductionData,
            List<RingFenceDetail> ringFenceDetails)
        {
            RingFenceDetail det = new RingFenceDetail();
            DistributionCenter dc;
            int existingRingFenceQty = 0;
            int currentRingFenceQty = 0;

            det.Size = stockSizeNumber;

            // retrieve dc
            dc = distributionCenters.Where(d => d.MFCode.Equals(warehouseCode)).FirstOrDefault();

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
            // retrieve possible reduction record
            var reduction = reductionData.Where(rd => rd.Sku.Equals(sku) &&
                                                      rd.Size.Equals(stockSizeNumber) &&
                                                      rd.MFCode.Equals(warehouseCode) &&
                                                      rd.PO.Equals(poNumber)).FirstOrDefault();

            existingRingFenceQty = (reduction != null) ? reduction.Qty : 0;

            currentRingFenceQty = 0;

            // retrieve ringfencedetails summed quantity
            var ringFenceDetail = ringFenceDetails.Where(rfd => rfd.RingFenceID.Equals(currentRingfenceID) &&
                                                                rfd.Size.Equals(stockSizeNumber) &&
                                                                rfd.DCID.Equals(dc.ID) &&
                                                                rfd.PO.Equals(poNumber)).FirstOrDefault();

            currentRingFenceQty = (ringFenceDetail != null) ? ringFenceDetail.Qty : 0;
            det.AvailableQty = allocatableQty - existingRingFenceQty + currentRingFenceQty;
            det.DueIn = expectedDeliveryDate;
            return det;
        }

        /// <summary>
        /// Tuple is defined as such:
        /// Item1 => Sku
        /// Item2 => Size
        /// Item3 => PO
        /// Item4 => Warehouse ID
        /// </summary>
        /// <param name="uniqueCombos"></param>
        /// <returns></returns>
        public List<WarehouseInventory> GetFuturePOsNew(List<Tuple<string, string, string, string>> uniqueCombos)
         {
            List<Tuple<bool, string>> generatedSQLStatements = new List<Tuple<bool, string>>();
            List<WarehouseInventory> returnValue = new List<WarehouseInventory>();
            var futureSizes = uniqueCombos.Where(uc => uc.Item2.Length.Equals(3)).ToList();
            var futureCaselots = uniqueCombos.Where(uc => uc.Item2.Length.Equals(5)).ToList();
            int batchSize = 20;

            if (futureSizes.Count > 0)
            {
                for (int i = 0; i < futureSizes.Count; i += batchSize)
                {
                    if ((i + batchSize) > futureSizes.Count)
                    {
                        batchSize = futureSizes.Count - i;
                    }
                    var batchFutureSizes = futureSizes.GetRange(i, batchSize).ToList();
                    generatedSQLStatements = BuildFutureWarehouseAvailableQuery(batchFutureSizes, true);
                    returnValue.AddRange(this.ExecuteFutureSQLandParseValues(generatedSQLStatements, true));
                }
            }

            batchSize = 20;

            if (futureCaselots.Count > 0)
            {
                for (int i = 0; i < futureCaselots.Count; i += batchSize)
                {
                    if ((i + batchSize) > futureCaselots.Count)
                    {
                        batchSize = futureCaselots.Count - i;
                    }
                    var batchFutureCaselots = futureCaselots.GetRange(i, batchSize).ToList();
                    generatedSQLStatements = BuildFutureWarehouseAvailableQuery(batchFutureCaselots, false);
                    returnValue.AddRange(this.ExecuteFutureSQLandParseValues(generatedSQLStatements, false));
                }
            }
            return returnValue;
        }

        private List<WarehouseInventory> ExecuteFutureSQLandParseValues(List<Tuple<bool, string>> generatedSQLStatements, bool futureSizeCombos)
        {
            List<WarehouseInventory> returnValue = new List<WarehouseInventory>();
            string parsedDivision, parsedDepartment, parsedStockNumber, parsedWidthColor, parsedSize, parsedPO, parsedDCID;
            int parsedQuantity;
            Database db = null;

            foreach (var sqlStatement in generatedSQLStatements)
            {
                db = (sqlStatement.Item1) ? _databaseEurope : _database;
                DbCommand SQLCommand = db.GetSqlStringCommand(sqlStatement.Item2);
                DataSet returnedData = db.ExecuteDataSet(SQLCommand);

                if (returnedData.Tables.Count > 0)
                {
                    foreach (DataRow dr in returnedData.Tables[0].Rows)
                    {
                        parsedDivision = Convert.ToString(dr["retl_oper_div_code"]);
                        parsedDepartment = Convert.ToString(dr["stk_dept_num"]);
                        parsedStockNumber = Convert.ToString(dr["stk_num"]);
                        parsedWidthColor = Convert.ToString(dr["wdth_color_num"]);
                        parsedSize = Convert.ToString(dr["stk_size_num"]);
                        parsedPO = Convert.ToString(dr["po_num"]);
                        parsedDCID = Convert.ToString(dr["whse_id_num"]);
                        parsedQuantity = Convert.ToInt32(dr["quantity"]);
                        string sku = string.Format("{0}-{1}-{2}-{3}", parsedDivision, parsedDepartment, parsedStockNumber, parsedWidthColor);

                        returnValue.Add(new WarehouseInventory(sku, parsedSize, parsedDCID, parsedPO, parsedQuantity));
                    }
                }
            }

            return returnValue;
        }

        private List<Tuple<bool, string>> BuildFutureWarehouseAvailableQuery(List<Tuple<string, string, string, string>> uniqueCombos, bool futureSizeCombos)
        {
            List<Tuple<bool, string>> returnValue = new List<Tuple<bool, string>>();
            StringBuilder builder = new StringBuilder();
            string division, department, stockNumber, widthColor, whereConditionFormat, whereCondition, selectStatement, groupByStatement;
            string europeDivision = System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"];

            // this will create the select and where condition formats dependent if it is a size or caselot
            if (futureSizeCombos)
            {
                // size combos come from table tkpod005 (solid detail)
                selectStatement = @"select
                                          wc.retl_oper_div_code
                                        , wc.stk_dept_num
                                        , wc.stk_num
                                        , wc.wdth_color_num
                                        , h.expected_delv_date
                                        , h.retl_oper_div_code
                                        , h.po_num
                                        , wc.status_ind
                                        , h.priority_code
                                        , sd.whse_id_num
                                        , rtrim(sd.stk_size_num) as stk_size_num
                                        , SUM(sd.order_qty - sd.received_qty) as quantity
                                   from TKPOD001 h, TKPOD003 wc, TKPOD005 sd
                                  where ";
                whereConditionFormat = @" (h.retl_oper_div_code = wc.retl_oper_div_code and
                                           wc.retl_oper_div_code = sd.retl_oper_div_code and
                                           h.po_num = wc.po_num and
                                           wc.po_num = sd.po_num and
                                           h.edit_phase_ind = 'A' and
                                           wc.status_ind in (' ', 'P', 'R') and
                                           wc.stk_dept_num = sd.stk_dept_num and
                                           wc.stk_num = sd.stk_num and
                                           wc.wdth_color_num = sd.wdth_color_num and
                                           h.po_num = '{0}' and
                                           h.retl_oper_div_code = '{1}' and
                                           wc.stk_dept_num = '{2}' and
                                           wc.stk_num = '{3}' and
                                           wc.wdth_color_num = '{4}' and
                                           sd.stk_size_num = '{5}' and
                                           sd.whse_id_num = '{6}')";

                groupByStatement = @"group by wc.retl_oper_div_code, wc.stk_dept_num, wc.stk_num, wc.wdth_color_num, h.expected_delv_date, h.retl_oper_div_code, h.po_num, wc.status_ind, h.priority_code, sd.whse_id_num, sd.stk_size_num";
            }
            else
            {
                // caselot combos come from table tkpod007
                selectStatement = @"select
                                          wc.retl_oper_div_code
                                        , wc.stk_dept_num
                                        , wc.stk_num
                                        , wc.wdth_color_num
                                        , h.expected_delv_date
                                        , h.retl_oper_div_code
                                        , h.po_num
                                        , wc.status_ind
                                        , h.priority_code
                                        , rd.whse_id_num
                                        , rd.caselot_number as stk_size_num
                                        , sum(rd.order_qty - rd.received_qty) AS quantity
                                   from TKPOD001 h, TKPOD003 wc, TKPOD007 rd
                                  where ";
                whereConditionFormat = @" (h.retl_oper_div_code = wc.retl_oper_div_code and
                                           wc.retl_oper_div_code = rd.retl_oper_div_code and
                                           h.po_num = wc.po_num and
                                           wc.po_num = rd.po_num and
                                           h.edit_phase_ind = 'A' and
                                           wc.status_ind in (' ', 'P', 'R') and
                                           wc.stk_dept_num = rd.stk_dept_num and
                                           wc.stk_num = rd.stk_num and
                                           wc.wdth_color_num = rd.wdth_color_num and
                                           h.po_num = '{0}' and
                                           h.retl_oper_div_code = '{1}' and
                                           wc.stk_dept_num = '{2}' and
                                           wc.stk_num = '{3}' and
                                           wc.wdth_color_num = '{4}' and
                                           rd.caselot_number = '{5}' and
                                           rd.whse_id_num = '{6}')";

                groupByStatement = "group by wc.retl_oper_div_code, wc.stk_dept_num, wc.stk_num, wc.wdth_color_num, h.expected_delv_date, h.retl_oper_div_code, h.po_num, wc.status_ind, h.priority_code, rd.whse_id_num, rd.caselot_number";
            }

            var europeCombos = uniqueCombos.Where(uc => europeDivision.Contains(uc.Item1.Split('-')[0])).ToList();
            var nonEuropeCombos = uniqueCombos.Where(uc => !europeDivision.Contains(uc.Item1.Split('-')[0])).ToList();

            if (europeCombos.Count > 0)
            {
                builder = builder.Clear();
                builder.Append(selectStatement);
                foreach (var combo in europeCombos)
                {
                    string[] skuTokens = combo.Item1.Split('-');
                    division = skuTokens[0];
                    department = skuTokens[1];
                    stockNumber = skuTokens[2];
                    widthColor = skuTokens[3];

                    whereCondition = string.Format(whereConditionFormat, combo.Item3, division, department, stockNumber, widthColor, combo.Item2, combo.Item4);

                    if (europeCombos.Last().Equals(combo))
                    {
                        builder.Append(whereCondition);
                    }
                    else
                    {
                        builder.Append(whereCondition + "OR");
                    }
                }
                builder.Append(groupByStatement);
                returnValue.Add(Tuple.Create(true, builder.ToString()));
            }

            if (nonEuropeCombos.Count > 0)
            {
                builder = builder.Clear();
                builder.Append(selectStatement);
                foreach (var combo in nonEuropeCombos)
                {
                    string[] skuTokens = combo.Item1.Split('-');
                    division = skuTokens[0];
                    department = skuTokens[1];
                    stockNumber = skuTokens[2];
                    widthColor = skuTokens[3];

                    whereCondition = string.Format(whereConditionFormat, combo.Item3, division, department, stockNumber, widthColor, combo.Item2, combo.Item4);

                    if (nonEuropeCombos.Last().Equals(combo))
                    {
                        builder.Append(whereCondition);
                    }
                    else
                    {
                        builder.Append(whereCondition + "OR");
                    }
                }

                builder.Append(groupByStatement);
                returnValue.Add(Tuple.Create(false, builder.ToString()));
            }

            return returnValue;
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
            SQL += "c.WHSE_ID_NUM,RTRIM(c.STK_SIZE_NUM) as STK_SIZE_NUM,SUM(c.ORDER_QTY - c.RECEIVED_QTY) as due_in ";
            SQL += " from tkpod001 a,tkpod003 b, tkpod005 c ";
            SQL += " where ";
            SQL += " a.retl_oper_div_code = b.retl_oper_div_code ";
            SQL += " and b.retl_oper_div_code = c.retl_oper_div_code ";
            SQL += " and a.PO_NUM = b.PO_NUM ";
            SQL += " and b.PO_NUM = c.PO_NUM ";
            SQL += " and b.STATUS_IND in (' ','P','R') ";
            SQL += " and a.edit_phase_ind = 'A' ";
            SQL += " and b.STK_DEPT_NUM = c.STK_DEPT_NUM ";
            SQL += " and b.STK_NUM = c.STK_NUM  ";
            SQL += " and b.WDTH_COLOR_NUM = c.WDTH_COLOR_NUM ";
            SQL += " and a.RETL_OPER_DIV_CODE = '" + rf.Division + "'";
            SQL += " and b.STK_DEPT_NUM = '" + dept + "' ";
            SQL += " and b.STK_NUM = '" + stock + "' ";
            SQL += " and b.WDTH_COLOR_NUM = '" + color + "' ";
            SQL += " and c.WHSE_ID_NUM != '' ";
            SQL += " group by a.EXPECTED_DELV_DATE, a.retl_oper_div_code, a.PO_NUM,a.PRIORITY_CODE,b.STATUS_IND,c.WHSE_ID_NUM,c.STK_SIZE_NUM ";
            SQL += " having SUM(c.ORDER_QTY - c.RECEIVED_QTY) > 0 ";

            SQL += " UNION ALL ";

            SQL += " select a.EXPECTED_DELV_DATE, a.retl_oper_div_code, a.PO_NUM,b.STATUS_IND,a.PRIORITY_CODE, ";
            SQL += " c.WHSE_ID_NUM,c.CASELOT_NUMBER as STK_SIZE_NUM,SUM(c.ORDER_QTY - c.RECEIVED_QTY) as due_in ";
            SQL += " from tkpod001 a, tkpod003 b, tkpod007 c ";
            SQL += " where a.retl_oper_div_code = b.retl_oper_div_code ";
            SQL += " and b.retl_oper_div_code = c.retl_oper_div_code ";
            SQL += " and a.PO_NUM = b.PO_NUM ";
            SQL += " and b.PO_NUM = c.PO_NUM ";
            SQL += " and a.edit_phase_ind = 'A' ";
            SQL += " and b.STATUS_IND in (' ','P','R') ";
            SQL += " and b.STK_DEPT_NUM = c.STK_DEPT_NUM ";
            SQL += " and b.STK_NUM = c.STK_NUM  ";
            SQL += " and b.WDTH_COLOR_NUM = c.WDTH_COLOR_NUM ";
            SQL += " and a.RETL_OPER_DIV_CODE = '" + rf.Division + "'";
            SQL += " and b.STK_DEPT_NUM = '" + dept + "' ";
            SQL += " and b.STK_NUM = '" + stock + "' ";
            SQL += " and b.WDTH_COLOR_NUM = '" + color + "' ";
            SQL += " and c.WHSE_ID_NUM != '' ";
            SQL += " group by a.EXPECTED_DELV_DATE, a.retl_oper_div_code, a.PO_NUM,a.PRIORITY_CODE,b.STATUS_IND,c.WHSE_ID_NUM,c.CASELOT_NUMBER ";
            SQL += " having SUM(c.ORDER_QTY - c.RECEIVED_QTY) > 0 ";

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);

            DataSet data = new DataSet();
            data = currDatabase.ExecuteDataSet(SQLCommand);

            RingFenceDetail det;
            if (data.Tables.Count > 0)
            {
                List<DataRow> futureInventory = data.Tables[0].AsEnumerable().ToList();
                List<DataRow> newFutureInventory = new List<DataRow>();

                // if RFID = 0 then this is not a RF that is already in the DB
                if (rf.ID == 0)
                {
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
                }

                List<InventoryReductions> reductionData = new List<InventoryReductions>();
                List<RingFenceDetail> ringFenceDetails = new List<RingFenceDetail>();

                var uniqueCombos = futureInventory.Select(fi => Tuple.Create(rf.Sku, Convert.ToString(fi["STK_SIZE_NUM"]), Convert.ToString(fi["WHSE_ID_NUM"]), Convert.ToString(fi["PO_NUM"]))).Distinct().ToList();

                // populate lists to pass into BuildFutureRingFenceDetails
                this.PopulateFutureRingFenceData(ref reductionData, ref ringFenceDetails, uniqueCombos, rf);

                foreach (DataRow dr in futureInventory)
                {
                    det = BuildFutureRingFenceDetail(rf.Sku, Convert.ToString(dr["STK_SIZE_NUM"]),
                                 Convert.ToString(dr["WHSE_ID_NUM"]), rf.ID, Convert.ToString(dr["PO_NUM"]),
                                 Convert.ToString(dr["PRIORITY_CODE"]), //list, 
                                 Convert.ToInt32(dr["due_in"]),
                                 Convert.ToDateTime(dr["EXPECTED_DELV_DATE"]), reductionData, ringFenceDetails);

                    _que.Add(det);
                }
            }
            return _que;
        }

        /*
         * uniqueCombos tuple is defined below:
         * Item1 => Sku
         * Item2 => Size
         * Item3 => DCID
         * Item4 => PO
         */
        public void PopulateFutureRingFenceData(ref List<InventoryReductions> reductionData, ref List<RingFenceDetail> ringFenceDetails, List<Tuple<string, string, string, string>> uniqueCombos, RingFence rf)
        {
            // retrieve reduction data
            var baseReductionData = db.InventoryReductions.Where(ir => ir.Sku.Equals(rf.Sku)).ToList();
            reductionData = baseReductionData.Where(br => uniqueCombos.Any(uc => uc.Item1.Equals(br.Sku) &&
                                                                                     uc.Item2.Equals(br.Size) &&
                                                                                     uc.Item3.Equals(br.MFCode) &&
                                                                                     uc.Item4.Equals(br.PO))).ToList();

            // retrieve current ringfencedetail data
            ringFenceDetails = db.RingFenceDetails.Where(rfd => rfd.RingFenceID.Equals(rf.ID)).ToList();          
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

                // if RFID = 0 then this is not a RF that is already in the DB
                if (rf.ID != 0)
                {
                    var ringfencedetails = db.RingFenceDetails.Where(rfd => rfd.RingFenceID.Equals(rf.ID)).ToList();
                    if (ringfencedetails.Count > 0)
                    {
                        foreach (RingFenceDetail rfd in ringfencedetails)
                        {
                            var newRow = data.Tables[0].AsEnumerable().Where(r => Convert.ToString(r["Size"]) == rfd.Size &&
                                                                                  Convert.ToString(r["InventoryID"]).Split('-')[0] == rfd.PO &&
                                                                                  Convert.ToString(r["Store"]) == rfd.Warehouse);
                            newFutureInventory.AddRange(newRow);
                        }

                        futureInventory = newFutureInventory;
                    }
                }

                List<InventoryReductions> reductionData = new List<InventoryReductions>();
                List<RingFenceDetail> ringFenceDetails = new List<RingFenceDetail>();

                var uniqueCombos = futureInventory.Select(fi => Tuple.Create(rf.Sku,
                                                                        Convert.ToString(fi["Size"]), 
                                                                        Convert.ToString(fi["Store"]),
                                                                        Convert.ToString(fi["InventoryID"]).Split('-')[0]))
                                                  .Distinct().ToList();

                // populate lists to pass into BuildFutureRingFenceDetails
                this.PopulateFutureRingFenceData(ref reductionData, ref ringFenceDetails, uniqueCombos, rf);

                foreach (DataRow dr in futureInventory)
                {
                    string availableDateString = Convert.ToString(dr["AvailableDate"]);
                    DateTime availableDate = Convert.ToDateTime(availableDateString.Substring(4, 2) + "-" +
                        availableDateString.Substring(6, 2) + "-" + availableDateString.Substring(0, 4));

                    det = BuildFutureRingFenceDetail(rf.Sku, Convert.ToString(dr["Size"]),
                                    Convert.ToString(dr["Store"]), rf.ID, 
                                    Convert.ToString(dr["InventoryID"]).Split('-')[0], "", 
                                    Convert.ToInt32(dr["StockQty"]), availableDate, reductionData,
                                    ringFenceDetails);
                    _que.Add(det);
                }
            }
            return _que;
        }

        /// <summary>
        /// Will generate SQL to be executed on the mainframe to determine the quantities for unique sizes OR caselots passed.
        /// </summary>
        /// <param name="uniqueCombos">the unique combos to create the where condition of the select statement.</param>
        /// <param name="sizeCombos">The boolean to determine if the unique combos are size combos are caselot combos.</param>
        /// <returns>The generated SQL to be executed on the mainframe.</returns>
        private string BuildWarehouseAvailableQuery(List<Tuple<string, string, string>> uniqueCombos, bool sizeCombos)
        {
            StringBuilder builder = new StringBuilder();
            string division, department, stockNumber, widthColor, whereConditionFormat, whereCondition;

            if (sizeCombos)
            {
                builder.Append("SELECT RETL_OPER_DIV_CD, STK_DEPT_NUM, STK_NUM, STK_WC_NUM, WHSE_ID_NUM, STK_SIZE_NUM, ALLOCATABLE_BS_QTY, PICK_RSRV_BS_QTY FROM TC052002 WHERE ");
                whereConditionFormat = " (RETL_OPER_DIV_CD = '{0}' AND STK_DEPT_NUM = '{1}' AND STK_NUM = '{2}' AND STK_WC_NUM = '{3}' AND STK_SIZE_NUM = '{4}'";
            }
            else
            {
                builder.Append("SELECT RETL_OPER_DIV_CD, STK_DEPT_NUM, STK_NUM, STK_WC_NUM, WHSE_ID_NUM, CL_SCHED_NUM, ALLOCATABLE_CL_QTY, PICK_RSRV_CL_QTY FROM TC052010 WHERE ");
                whereConditionFormat = " (RETL_OPER_DIV_CD = '{0}' AND STK_DEPT_NUM = '{1}' AND STK_NUM = '{2}' AND STK_WC_NUM = '{3}' AND CL_SCHED_NUM = '{4}'";
            }

            foreach (var combo in uniqueCombos)
            {
                // split sku into tokens delimited by the '-' symbol and store them locally
                string[] skuTokens = combo.Item1.Split('-');
                division = skuTokens[0];
                department = skuTokens[1];
                stockNumber = skuTokens[2];
                widthColor = skuTokens[3];

                whereCondition = string.Format(whereConditionFormat, division, department, stockNumber, widthColor, combo.Item2);

                if (!string.IsNullOrEmpty(combo.Item3))
                {
                    whereCondition += string.Format(" AND WHSE_ID_NUM = '{0}')", combo.Item3);
                }
                else
                {
                    whereCondition += ")";
                }

                if (uniqueCombos.Last().Equals(combo))
                {
                    builder.Append(whereCondition);
                }
                else
                {
                    builder.Append(whereCondition + " OR");
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Will execute the generated SQL and parse the values of returned from the mainframe.
        /// </summary>
        /// <param name="SQL">SQL to be executed.</param>
        /// <param name="division">needed to grab the correct database for the mainframe.</param>
        /// <param name="sizeCombos">To determine if this is a call to get the quantities for sizes or caselots (they come from two different tables).</param>
        /// <returns>The parsed values from the mainframe.</returns>
        private List<WarehouseInventory> ExecuteSQLAndParseValues(string SQL, string division, bool sizeCombos)
        {
            List<WarehouseInventory> returnValue = new List<WarehouseInventory>();
            string parsedDivision, parsedDepartment, parsedStockNumber, parsedWidthColor, parsedSize, parsedDCID, allocatableQuantityColumnName, sizeColumnName, pickReserveColumnName;
            int parsedAllocatableQuantity, parsedPickReserveQuantity;
            Database db = null;

            db = (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(division)) ? _databaseEurope : _database;
            if (sizeCombos)
            {
                allocatableQuantityColumnName = "ALLOCATABLE_BS_QTY";
                pickReserveColumnName = "PICK_RSRV_BS_QTY";
                sizeColumnName = "STK_SIZE_NUM";
            }
            else
            {
                allocatableQuantityColumnName = "ALLOCATABLE_CL_QTY";
                pickReserveColumnName = "PICK_RSRV_CL_QTY";
                sizeColumnName = "CL_SCHED_NUM";
            }

            DbCommand SQLCommand = db.GetSqlStringCommand(SQL);
            DataSet returnedData = db.ExecuteDataSet(SQLCommand);
            if (returnedData.Tables.Count > 0)
            {
                foreach (DataRow dr in returnedData.Tables[0].Rows)
                {

                    parsedDivision = Convert.ToString(dr["RETL_OPER_DIV_CD"]);
                    parsedDepartment = Convert.ToString(dr["STK_DEPT_NUM"]);
                    parsedStockNumber = Convert.ToString(dr["STK_NUM"]);
                    parsedWidthColor = Convert.ToString(dr["STK_WC_NUM"]);
                    parsedSize = Convert.ToString(dr[sizeColumnName]).PadLeft(3, '0');
                    parsedAllocatableQuantity = Convert.ToInt32(dr[allocatableQuantityColumnName]);
                    parsedPickReserveQuantity = Convert.ToInt32(dr[pickReserveColumnName]);
                    parsedDCID = Convert.ToString(dr["WHSE_ID_NUM"]);
                    string sku = string.Format("{0}-{1}-{2}-{3}", parsedDivision, parsedDepartment, parsedStockNumber, parsedWidthColor);
                    returnValue.Add(new WarehouseInventory(sku, parsedSize, parsedDCID, parsedAllocatableQuantity, parsedPickReserveQuantity));
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Respective tuple values
        /// Item1 => Sku
        /// Item2 => Size
        /// Item3 => DC (MF Distribution Center ID, different than Allocation's DCID)
        /// </summary>
        /// <param name="uniqueCombos"></param>
        public List<WarehouseInventory> GetWarehouseAvailableNew(List<Tuple<string, string, string>> uniqueCombos)
        {
            List<WarehouseInventory> returnValue = new List<WarehouseInventory>();
            //WarehouseInventoryDAO d = new WarehouseInventoryDAO();
            string division = string.Empty, SQL = string.Empty;
            var uniqueSizeCombos = uniqueCombos.Where(c => c.Item2.Length.Equals(3)).ToList();
            var uniqueCaselotCombos = uniqueCombos.Where(c => c.Item2.Length.Equals(5)).ToList();
            int batchSize = 50;

            if (uniqueSizeCombos.Count > 0)
            {
                for (int i = 0; i < uniqueSizeCombos.Count; i += batchSize)
                {
                    if ((i + batchSize) > uniqueSizeCombos.Count)
                    {
                        batchSize = uniqueSizeCombos.Count - i;
                    }
                    var batchedUniqueSizeCombos = uniqueSizeCombos.GetRange(i, batchSize).ToList();
                    division = uniqueSizeCombos.Select(sc => sc.Item1.Split('-')[0]).FirstOrDefault();
                    SQL = this.BuildWarehouseAvailableQuery(batchedUniqueSizeCombos, true);
                    returnValue.AddRange(this.ExecuteSQLAndParseValues(SQL, division, true));
                }
            }

            batchSize = 50;

            if (uniqueCaselotCombos.Count > 0)
            {
                for (int i = 0; i < uniqueCaselotCombos.Count; i += batchSize)
                {
                    if ((i + batchSize) > uniqueCaselotCombos.Count)
                    {
                        batchSize = uniqueCaselotCombos.Count - i;
                    }
                    var batchedUniqueCaselotCombos = uniqueCaselotCombos.GetRange(i, batchSize).ToList();
                    division = uniqueCaselotCombos.Select(sc => sc.Item1.Split('-')[0]).FirstOrDefault();
                    SQL = this.BuildWarehouseAvailableQuery(batchedUniqueCaselotCombos, false);
                    returnValue.AddRange(this.ExecuteSQLAndParseValues(SQL, division, false));
                }
            }

            // reduce ringfence quantity
            return returnValue;
        }

        public List<WarehouseInventory> ReduceRingFenceQuantities(List<WarehouseInventory> warehouseInventory)
        {
            var uniqueSkus = warehouseInventory.Select(wi => wi.Sku).Distinct().ToList();
            var uniqueDivisions = warehouseInventory.Select(wid => wid.Sku.Substring(0, 2)).Distinct().ToList();            

            var inventoryReductions = (from ir in db.InventoryReductionsByType
                                       where uniqueSkus.Contains(ir.Sku)
                                       select ir).ToList();

            var divisionCache = (from dc in db.AllocationDivisions
                                 where uniqueDivisions.Contains(dc.DivisionCode)
                                 select dc).ToList();

            foreach (var wi in warehouseInventory)
            {
                var tempIR = inventoryReductions.Where(ir => ir.Sku == wi.Sku &&
                                                             ir.Size == wi.size &&
                                                             ir.PO == wi.PO &&
                                                             ir.MFCode == wi.DistributionCenterID).FirstOrDefault();
                if (tempIR != null)
                {
                    wi.HasSeparateECOMInventory = (from d in divisionCache
                                                   where d.DivisionCode == tempIR.Sku.Substring(0, 2)
                                                   select d.HasSeparateECOMInventory).FirstOrDefault();

                    wi.ringFenceQuantity = tempIR.RingFenceQuantity;
                    wi.rdqQuantity = tempIR.RDQQuantity;
                    wi.orderQuantity = Convert.ToInt32(tempIR.OrderQuantity);                    
                }
            }

            return warehouseInventory;
        }

        public List<WarehouseInventory> ReduceAvailableInventory(List<WarehouseInventory> availableInventory)
        {
            int instanceID;
            List<string> aiSKUs = availableInventory.Select(ai => ai.Sku).ToList();
            string division = aiSKUs.First().Split('-')[0];
            instanceID = db.InstanceDivisions.Where(id => id.Division.Equals(division))
                                             .Select(id => id.InstanceID)
                                             .FirstOrDefault();

            if (availableInventory.Count > 0)
            {
                var invReductions = db.InventoryReductions.Where(ir => aiSKUs.Contains(ir.Sku)).ToList();

                foreach (var ai in availableInventory)
                {
                    var inventoryReduction = invReductions.Where(ir => ir.Sku.Equals(ai.Sku) &&
                                                                       ir.Size.Equals(ai.size) &&
                                                                       ir.MFCode.Equals(ai.DistributionCenterID) &&
                                                                       ir.PO.Equals(ai.PO) &&
                                                                       ir.InstanceID.Equals(instanceID)).FirstOrDefault();

                    if (inventoryReduction != null)
                    {
                        ai.quantity -= inventoryReduction.Qty;
                    }
                }
            }


            return availableInventory;
        }

        public List<RingFenceDetail> GetWarehouseAvailable(string sku, string size, long? currentRingFenceID)
        {
            List<WarehouseInventory> warehouseInventory;
            List<RingFenceDetail> rfDetailData;

            RingFenceDataFactory rfDataFactory = new RingFenceDataFactory();
            WarehouseInventoryDAO warehouseInventoryDAO = new WarehouseInventoryDAO(sku, "-1");

            warehouseInventory = warehouseInventoryDAO.GetWarehouseInventory(WarehouseInventoryDAO.InventoryListType.ListAllSizes);

            rfDetailData = rfDataFactory.CreateRFDetailsFromWarehouseInventory(warehouseInventory, currentRingFenceID);

            return rfDetailData;
        }

        public bool CanUserUpdateRingFence(RingFence rf, WebUser user, string appName, out string errorMessage)
        {
            bool result = true;
            errorMessage = "";

            if (!user.HasDivision(appName, rf.Division))
            {
                result = false;
                errorMessage = string.Format("You do not have permission to ring fence for division {0}", rf.Division);
            }
            else if (!user.HasDivDept(appName, rf.Division, rf.Department))
            {
                result = false;
                errorMessage = string.Format("You do not have permission to ring fence for department {0}", rf.Department);
            }

            return result;
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
				result = false;
				errorMessage = "You do not have permission to ring fence for department " + rf.Department;
            }

            return result;
        }

        public bool isEcommWarehouse(string division, string store)
        {
            return db.EcommWarehouses.Where(ew => ew.Division == division && ew.Store == store).Count() > 0;
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

                if (skuRespository.Get(sm => sm.MerchantSku == rf.Sku).Count() == 0)
                {
                    errorMessage = "Invalid Sku, does not exist";
                    return false;
                }
            }

            return result;
        }

        public static void SetRingFenceHeaderQtyAndHistory(List<long> ringfences, string user)
        {
            Database database = DatabaseFactory.CreateDatabase("AllocationContext");

            DbCommand SQLCommand;
            string SQL = "dbo.[SetRingFenceHeaderQty]";

            foreach (long id in ringfences)
            {
                SQLCommand = database.GetStoredProcCommand(SQL);
                database.AddInParameter(SQLCommand, "@id", DbType.Int64, id);
                database.AddInParameter(SQLCommand, "@user", DbType.String, user);

                database.ExecuteNonQuery(SQLCommand);
            }
        }
    }
}