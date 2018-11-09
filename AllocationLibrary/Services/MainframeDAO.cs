
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;

namespace Footlocker.Logistics.Allocation.Services
{
    public class MainframeDAO
    {
        Database _USdatabase;
        Database _Europedatabase;
        string _prefix;

        public MainframeDAO()
        {
            _USdatabase = DatabaseFactory.CreateDatabase("DB2PROD");
            _Europedatabase = DatabaseFactory.CreateDatabase("DB2EURP");
            //_prefix = "DB2TEST.";
            _prefix = System.Configuration.ConfigurationManager.AppSettings["DB2PREFIX"];
        }

        public String GetAvailabityCodes(string division)
        {
            List<AllocationDriver> _que;
            _que = new List<AllocationDriver>();

            DbCommand SQLCommandMF;
            string SQLMF;

            Database db;

            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(division))
            {
                db = _Europedatabase;
            }
            else
            {
                db = _USdatabase;
            }

            //SQLMF = "delete from " + System.Configuration.ConfigurationManager.AppSettings["DB2PREFIX"] + "TCQTM001 ";
            SQLMF = "SELECT COL02001 ";
            SQLMF = SQLMF + " FROM TC050002 ";
            SQLMF = SQLMF + " WHERE PARMVLGP = '" + division.PadLeft(2,'0') + "0000000000000000000000000000' ";
            SQLMF = SQLMF + " AND PARMCODE = 'PR0009' ";
            
            SQLCommandMF = db.GetSqlStringCommand(SQLMF);
            DataSet data = db.ExecuteDataSet(SQLCommandMF);


            AllocationDriverFactory factory = new AllocationDriverFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    return Convert.ToString(dr[0]);
                }
            }
            return "";
        }


        
        public List<MainframeLink> GetMainframeLinks(string sku)
        {
            List<MainframeLink> _que;
            _que = new List<MainframeLink>();
            string[] tokens;

            tokens = sku.Split('-');

            DbCommand SQLCommand;
            string SQL = "select XDOCK_INTRN_NUM,WHSE_ID_NUM,CASELOT_NUMBER,RETL_OPER_DIV_CODE,STR_NUM,SACC_IND,LOCK_IND from tcwms010 ";
            SQL = SQL + "where ";
            SQL = SQL + "retl_oper_div_code = '" + tokens[0] + "' ";
            SQL = SQL + "and stk_dept_num = '" + tokens[1] + "' ";
            SQL = SQL + "and stk_num = '" + tokens[2] + "' ";
            SQL = SQL + "and stk_WDTH_COLOR_NUM = '" + tokens[3] + "' ";

            Database db;

            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(tokens[0]))
            {
                db = _Europedatabase;
            }
            else
            {
                db = _USdatabase;
            }
            SQLCommand = db.GetSqlStringCommand(SQL);

            DataSet data = new DataSet();
            data = db.ExecuteDataSet(SQLCommand);

            MainframeLinkFactory factory = new MainframeLinkFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }
    }
}