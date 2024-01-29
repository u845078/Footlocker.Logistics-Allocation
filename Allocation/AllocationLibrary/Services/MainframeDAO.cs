using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using System.Data.SqlClient;

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

            string parmvlgp = string.Format("{0}0000000000000000000000000000", division.PadLeft(2, '0'));

            SQLMF = "SELECT COL02001 ";
            SQLMF += " FROM TC050002 ";
            SQLMF += " WHERE PARMVLGP = ? ";
            SQLMF += " AND PARMCODE = 'PR0009' ";
            
            SQLCommandMF = db.GetSqlStringCommand(SQLMF);
            db.AddInParameter(SQLCommandMF, "@1", DbType.String, parmvlgp);

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
            SQL += "where retl_oper_div_code = ? ";
            SQL += "and stk_dept_num = ? ";
            SQL += "and stk_num = ? ";
            SQL += "and stk_WDTH_COLOR_NUM = ? ";

            Database db;

            if (europeDivisions.Contains(tokens[0]))            
                db = _Europedatabase;            
            else            
                db = _USdatabase;
            
            SQLCommand = db.GetSqlStringCommand(SQL);
            db.AddInParameter(SQLCommand, "@1", DbType.String, tokens[0]);
            db.AddInParameter(SQLCommand, "@2", DbType.String, tokens[1]);
            db.AddInParameter(SQLCommand, "@3", DbType.String, tokens[2]);
            db.AddInParameter(SQLCommand, "@4", DbType.String, tokens[3]);

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