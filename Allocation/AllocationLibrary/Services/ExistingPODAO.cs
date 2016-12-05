
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Factories;

namespace Footlocker.Logistics.Allocation.Models.Services
{
    public class ExistingPODAO
    {
        #region Fields
        private readonly string _EUROPE_DIV_CODE_STRING = "31";
        private readonly string _EUROPE_DB_SETTING_ID_STRING = "DB2EURP";
        private readonly string _DB_SETTING_ID_STRING = "DB2PROD";

        #endregion
        
        #region Non-Public Methods

        private Database CreateDatabase(string division)
        {
            var dbSettingID = division.Equals(_EUROPE_DIV_CODE_STRING) ? _EUROPE_DB_SETTING_ID_STRING : _DB_SETTING_ID_STRING;
            return DatabaseFactory.CreateDatabase(dbSettingID);
        }

        #endregion

        #region Public Methods
        public List<ExistingPO> GetExistingPO(string div, string PO)
        {
            var existingPOs = new List<ExistingPO>();

            // Construct Query
            string SQL = "select d.WHSE_ID_NUM, a.PO_NUM, a.EXPECTED_DELV_DATE, b.RETL_OPER_DIV_CODE, b.stk_dept_num, b.stk_num, b.WDTH_COLOR_NUM, b.TOT_WC_RETL_AMT, c.GENL_STK_DESC,sum(d.order_qty) as qty,";
            SQL = SQL + " a.po_status_code, a.vend_num, a.crte_date, sum(d.received_qty) as receive_qty";
            SQL = SQL + " from TKPOD001 a, TKPOD003 b, TKPOD002 c, TKPOD005 d ";
            SQL = SQL + " where a.RETL_OPER_DIV_CODE = b.RETL_OPER_DIV_CODE";
            SQL = SQL + " and a.PO_NUM = b.PO_NUM and a.PO_NUM = c.PO_NUM";
            SQL = SQL + " and b.RETL_OPER_DIV_CODE = c.RETL_OPER_DIV_CODE and b.stk_dept_num = c.stk_dept_num and b.stk_num = c.stk_num";
            SQL = SQL + " and d.RETL_OPER_DIV_CODE = c.RETL_OPER_DIV_CODE and d.stk_dept_num = c.stk_dept_num and d.stk_num = c.stk_num";
            SQL = SQL + " and b.WDTH_COLOR_NUM = d.WDTH_COLOR_NUM and d.PO_NUM = C.PO_NUM ";
            SQL = SQL + " and a.PO_STATUS_CODE in ('O','R','P',' ')";
            //SQL = SQL + " and d.WHSE_ID_NUM != ' '";
            SQL = SQL + " and a.PO_NUM = '" + PO + "'";
            SQL = SQL + " and a.RETL_OPER_DIV_CODE = '" + div + "'";
            SQL = SQL + " group by d.WHSE_ID_NUM, a.PO_NUM, a.EXPECTED_DELV_DATE, b.RETL_OPER_DIV_CODE, b.stk_dept_num, ";
            SQL = SQL + " b.stk_num, b.WDTH_COLOR_NUM, b.TOT_WC_RETL_AMT, c.GENL_STK_DESC, a.po_status_code, a.vend_num, a.crte_date";

            SQL = SQL + " UNION ALL";

            SQL = SQL + " select d.WHSE_ID_NUM, a.PO_NUM, a.EXPECTED_DELV_DATE, b.RETL_OPER_DIV_CODE, b.stk_dept_num, b.stk_num, b.WDTH_COLOR_NUM, b.TOT_WC_RETL_AMT, c.GENL_STK_DESC";
            SQL = SQL + " ,sum(d.order_qty * (STK_SIZE_QTY_1 +STK_SIZE_QTY_2 +STK_SIZE_QTY_3 +STK_SIZE_QTY_4 +STK_SIZE_QTY_5 +STK_SIZE_QTY_6 +STK_SIZE_QTY_7 +STK_SIZE_QTY_8 +STK_SIZE_QTY_9 +STK_SIZE_QTY_10 +STK_SIZE_QTY_11 +STK_SIZE_QTY_12 +STK_SIZE_QTY_13 +STK_SIZE_QTY_14 +STK_SIZE_QTY_15 +STK_SIZE_QTY_16 +STK_SIZE_QTY_17 +STK_SIZE_QTY_18)) as qty,";
            SQL = SQL + " a.po_status_code, a.vend_num, a.crte_date, sum(d.received_qty) as receive_qty";
            SQL = SQL + " from TKPOD001 a, TKPOD003 b, TKPOD002 c, TKPOD007 d, TCFIL038 e ";
            SQL = SQL + " where a.RETL_OPER_DIV_CODE = b.RETL_OPER_DIV_CODE";
            SQL = SQL + " and a.PO_NUM = b.PO_NUM and a.PO_NUM = c.PO_NUM";
            SQL = SQL + " and b.RETL_OPER_DIV_CODE = c.RETL_OPER_DIV_CODE and b.stk_dept_num = c.stk_dept_num and b.stk_num = c.stk_num";
            SQL = SQL + " and d.RETL_OPER_DIV_CODE = c.RETL_OPER_DIV_CODE and d.stk_dept_num = c.stk_dept_num and d.stk_num = c.stk_num";
            SQL = SQL + " and b.WDTH_COLOR_NUM = d.WDTH_COLOR_NUM and d.PO_NUM = C.PO_NUM ";
            SQL = SQL + " and a.PO_STATUS_CODE in ('O','R','P',' ')";
            //SQL = SQL + " and d.WHSE_ID_NUM != ' '";
            SQL = SQL + " and a.PO_NUM = '" + PO + "'";
            SQL = SQL + " and a.RETL_OPER_DIV_CODE = '" + div + "'";
            SQL = SQL + " and e.CASELOT_NUM = d.CASELOT_NUMBER";
            SQL = SQL + " and e.RETL_OPER_DIV_CODE = d.RETL_OPER_DIV_CODE";
            SQL = SQL + " group by d.WHSE_ID_NUM, a.PO_NUM, a.EXPECTED_DELV_DATE, b.RETL_OPER_DIV_CODE, b.stk_dept_num,";
            SQL = SQL + " b.stk_num, b.WDTH_COLOR_NUM, b.TOT_WC_RETL_AMT, c.GENL_STK_DESC,";
            SQL = SQL + " a.po_status_code, a.vend_num, a.crte_date";

            // Create database (connecting to appropriate server for FLE if need be)
            var database = CreateDatabase(div);

            // Establish Connection
            using (var SQLCommand = database.GetSqlStringCommand(SQL))
            {
                // Execute Query
                var data = database.ExecuteDataSet(SQLCommand);

                // Load domain objects
                if (data.Tables.Count > 0)
                {
                    var factory = new ExistingPOFactory();

                    foreach (DataRow dr in data.Tables[0].Rows)
                    {
                        existingPOs.Add(factory.Create(dr));
                    }
                }
            }

            if (existingPOs.Count == 0)
            {
                //alternate query.  Sometimes they share the caselot on the TCFIL038 table.
                //so we'll run that query without the division linked to get that record.
                //left the original query because we didn't want duplicates
                SQL = " select d.WHSE_ID_NUM, a.PO_NUM, a.EXPECTED_DELV_DATE, b.RETL_OPER_DIV_CODE, b.stk_dept_num, b.stk_num, b.WDTH_COLOR_NUM, b.TOT_WC_RETL_AMT, c.GENL_STK_DESC";
                SQL = SQL + " ,sum(d.order_qty * (STK_SIZE_QTY_1 +STK_SIZE_QTY_2 +STK_SIZE_QTY_3 +STK_SIZE_QTY_4 +STK_SIZE_QTY_5 +STK_SIZE_QTY_6 +STK_SIZE_QTY_7 +STK_SIZE_QTY_8 +STK_SIZE_QTY_9 +STK_SIZE_QTY_10 +STK_SIZE_QTY_11 +STK_SIZE_QTY_12 +STK_SIZE_QTY_13 +STK_SIZE_QTY_14 +STK_SIZE_QTY_15 +STK_SIZE_QTY_16 +STK_SIZE_QTY_17 +STK_SIZE_QTY_18)) as qty,";
                SQL = SQL + " a.po_status_code, a.vend_num, a.crte_date, sum(d.received_qty) as receive_qty";
                SQL = SQL + " from TKPOD001 a, TKPOD003 b, TKPOD002 c, TKPOD007 d, TCFIL038 e ";
                SQL = SQL + " where a.RETL_OPER_DIV_CODE = b.RETL_OPER_DIV_CODE";
                SQL = SQL + " and a.PO_NUM = b.PO_NUM and a.PO_NUM = c.PO_NUM";
                SQL = SQL + " and b.RETL_OPER_DIV_CODE = c.RETL_OPER_DIV_CODE and b.stk_dept_num = c.stk_dept_num and b.stk_num = c.stk_num";
                SQL = SQL + " and d.RETL_OPER_DIV_CODE = c.RETL_OPER_DIV_CODE and d.stk_dept_num = c.stk_dept_num and d.stk_num = c.stk_num";
                SQL = SQL + " and b.WDTH_COLOR_NUM = d.WDTH_COLOR_NUM and d.PO_NUM = C.PO_NUM ";
                SQL = SQL + " and a.PO_STATUS_CODE in ('O','R','P',' ')";
                //SQL = SQL + " and d.WHSE_ID_NUM != ' '";
                SQL = SQL + " and a.PO_NUM = '" + PO + "'";
                SQL = SQL + " and a.RETL_OPER_DIV_CODE = '" + div + "'";
                SQL = SQL + " and e.CASELOT_NUM = d.CASELOT_NUMBER";
                //SQL = SQL + " and e.RETL_OPER_DIV_CODE = d.RETL_OPER_DIV_CODE";
                SQL = SQL + " group by d.WHSE_ID_NUM, a.PO_NUM, a.EXPECTED_DELV_DATE, b.RETL_OPER_DIV_CODE, b.stk_dept_num, ";
                SQL = SQL + " b.stk_num, b.WDTH_COLOR_NUM, b.TOT_WC_RETL_AMT, c.GENL_STK_DESC, ";
                SQL = SQL + " a.po_status_code, a.vend_num, a.crte_date";

                using (var SQLCommand = database.GetSqlStringCommand(SQL))
                {
                    // Execute Query
                    var data = database.ExecuteDataSet(SQLCommand);

                    // Load domain objects
                    if (data.Tables.Count > 0)
                    {
                        var factory = new ExistingPOFactory();

                        foreach (DataRow dr in data.Tables[0].Rows)
                        {
                            existingPOs.Add(factory.Create(dr));
                        }
                    }
                }

            }

            return existingPOs;
        }

        public List<ExistingPO> GetExistingPOsForSku(string div, string sku, bool openOnly)
        {
            var existingPOs = new List<ExistingPO>();
            

            // Split delimited SKU string out
            string[] tokens = sku.Split('-');
                        
            // Build Query
            string SQL = "select DISTINCT d.WHSE_ID_NUM, a.PO_NUM, a.EXPECTED_DELV_DATE, b.RETL_OPER_DIV_CODE, b.stk_dept_num, b.stk_num, b.WDTH_COLOR_NUM, b.TOT_WC_RETL_AMT, c.GENL_STK_DESC,sum(d.order_qty) as qty,";
            SQL = SQL + " a.po_status_code, a.vend_num, a.crte_date, sum(d.received_qty) as receive_qty";
            SQL = SQL + " from TKPOD001 a, TKPOD003 b, TKPOD002 c, TKPOD005 d ";
            SQL = SQL + " where a.RETL_OPER_DIV_CODE = b.RETL_OPER_DIV_CODE";
            SQL = SQL + " and a.PO_NUM = b.PO_NUM and a.PO_NUM = c.PO_NUM and a.PO_NUM = d.PO_NUM";
            SQL = SQL + " and b.RETL_OPER_DIV_CODE = c.RETL_OPER_DIV_CODE and b.stk_dept_num = c.stk_dept_num and b.stk_num = c.stk_num";
            SQL = SQL + " and b.RETL_OPER_DIV_CODE = d.RETL_OPER_DIV_CODE and b.stk_dept_num = d.stk_dept_num and b.stk_num = d.stk_num";

            if (openOnly)
                SQL = SQL + " and a.PO_STATUS_CODE in ('O',' ')";
            else
                SQL = SQL + " and a.PO_STATUS_CODE not in ('D')";

            //SQL = SQL + " and d.WHSE_ID_NUM != ' '";
            SQL = SQL + " and b.stk_dept_num = '" + tokens[1] + "'";
            SQL = SQL + " and b.stk_num = '" + tokens[2] + "'";
            SQL = SQL + " and a.RETL_OPER_DIV_CODE = '" + div + "'";
            SQL = SQL + " group by d.WHSE_ID_NUM, a.PO_NUM, a.EXPECTED_DELV_DATE, b.RETL_OPER_DIV_CODE, b.stk_dept_num,";
            SQL = SQL + " b.stk_num, b.WDTH_COLOR_NUM, b.TOT_WC_RETL_AMT, c.GENL_STK_DESC, a.po_status_code, a.vend_num, a.crte_date";

            SQL = SQL + " UNION ALL ";

            SQL = SQL + "select DISTINCT d.WHSE_ID_NUM, a.PO_NUM, a.EXPECTED_DELV_DATE, b.RETL_OPER_DIV_CODE, b.stk_dept_num, b.stk_num, b.WDTH_COLOR_NUM, b.TOT_WC_RETL_AMT, c.GENL_STK_DESC";
            SQL = SQL + " ,sum(d.order_qty * (STK_SIZE_QTY_1 +STK_SIZE_QTY_2 +STK_SIZE_QTY_3 +STK_SIZE_QTY_4 +STK_SIZE_QTY_5 +STK_SIZE_QTY_6 +STK_SIZE_QTY_7 +STK_SIZE_QTY_8 +STK_SIZE_QTY_9 +STK_SIZE_QTY_10 +STK_SIZE_QTY_11 +STK_SIZE_QTY_12 +STK_SIZE_QTY_13 +STK_SIZE_QTY_14 +STK_SIZE_QTY_15 +STK_SIZE_QTY_16 +STK_SIZE_QTY_17 +STK_SIZE_QTY_18)) as qty,";
            SQL = SQL + " a.po_status_code, a.vend_num, a.crte_date, sum(d.received_qty) as receive_qty";
            SQL = SQL + " from TKPOD001 a, TKPOD003 b, TKPOD002 c, TKPOD007 d, TCFIL038 e ";
            SQL = SQL + " where a.RETL_OPER_DIV_CODE = b.RETL_OPER_DIV_CODE";
            SQL = SQL + " and a.PO_NUM = b.PO_NUM and a.PO_NUM = c.PO_NUM and a.PO_NUM = d.PO_NUM";
            SQL = SQL + " and b.RETL_OPER_DIV_CODE = c.RETL_OPER_DIV_CODE and b.stk_dept_num = c.stk_dept_num and b.stk_num = c.stk_num";
            SQL = SQL + " and b.RETL_OPER_DIV_CODE = d.RETL_OPER_DIV_CODE and b.stk_dept_num = d.stk_dept_num and b.stk_num = d.stk_num";
            
            if (openOnly)
                SQL = SQL + " and a.PO_STATUS_CODE in ('O',' ')";
            else
                SQL = SQL + " and a.PO_STATUS_CODE not in ('D')";

            //SQL = SQL + " and d.WHSE_ID_NUM != ' '";
            SQL = SQL + " and b.stk_dept_num = '" + tokens[1] + "'";
            SQL = SQL + " and b.stk_num = '" + tokens[2] + "'";
            SQL = SQL + " and a.RETL_OPER_DIV_CODE = '" + div + "'";
            SQL = SQL + " and e.CASELOT_NUM = d.CASELOT_NUMBER";
            SQL = SQL + " and e.RETL_OPER_DIV_CODE = d.RETL_OPER_DIV_CODE";
            SQL = SQL + " group by d.WHSE_ID_NUM, a.PO_NUM, a.EXPECTED_DELV_DATE, b.RETL_OPER_DIV_CODE, ";
            SQL = SQL + " b.stk_dept_num, b.stk_num, b.WDTH_COLOR_NUM, b.TOT_WC_RETL_AMT, c.GENL_STK_DESC,";
            SQL = SQL + " a.po_status_code, a.vend_num, a.crte_date";

            // Create database (connecting to appropriate server for FLE if need be)
            var database = CreateDatabase(div);

            // Establish Connection
            using (var SQLCommand = database.GetSqlStringCommand(SQL))
            {
                // Execute Query
                var data = database.ExecuteDataSet(SQLCommand);

                // Load Domain Objects
                if (data.Tables.Count > 0)
                {
                    var factory = new ExistingPOFactory();

                    foreach (DataRow dr in data.Tables[0].Rows)
                    {
                        existingPOs.Add(factory.Create(dr));                
                    }
                }
            }

            return existingPOs;
        }

        #endregion
    }
}