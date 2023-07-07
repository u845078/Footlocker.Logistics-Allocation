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
        readonly Database _USdatabase;
        readonly Database _Europedatabase;
        readonly string europeDivisions;

        public MainframeDAO(string europeDivisions)
        {
            _USdatabase = DatabaseFactory.CreateDatabase("DB2PROD");
            _Europedatabase = DatabaseFactory.CreateDatabase("DB2EURP");
            this.europeDivisions = europeDivisions;
        }

        public string GetAvailabityCodes(string division)
        {
            DbCommand SQLCommandMF;
            string SQLMF;

            Database db;

            if (europeDivisions.Contains(division))            
                db = _Europedatabase;            
            else            
                db = _USdatabase;            
            
            SQLMF = "SELECT COL02001 ";
            SQLMF += " FROM TC050002 ";
            SQLMF += " WHERE PARMVLGP = '" + division.PadLeft(2,'0') + "0000000000000000000000000000' ";
            SQLMF += " AND PARMCODE = 'PR0009' ";
            
            SQLCommandMF = db.GetSqlStringCommand(SQLMF);
            DataSet data = db.ExecuteDataSet(SQLCommandMF);

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
            string SQL = "select XDOCK_INTRN_NUM, WHSE_ID_NUM, CASELOT_NUMBER, RETL_OPER_DIV_CODE, STR_NUM, SACC_IND, LOCK_IND from tcwms010 ";
            SQL += "where retl_oper_div_code = '" + tokens[0] + "' ";
            SQL += "and stk_dept_num = '" + tokens[1] + "' ";
            SQL += "and stk_num = '" + tokens[2] + "' ";
            SQL += "and stk_WDTH_COLOR_NUM = '" + tokens[3] + "' ";

            Database db;

            if (europeDivisions.Contains(tokens[0]))            
                db = _Europedatabase;            
            else            
                db = _USdatabase;
            
            SQLCommand = db.GetSqlStringCommand(SQL);

            DataSet data;
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