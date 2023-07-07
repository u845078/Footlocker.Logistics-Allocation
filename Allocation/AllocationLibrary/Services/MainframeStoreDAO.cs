
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
    public class MainframeStoreDAO
    {
        Database _USdatabase;
        Database _Europedatabase;
        readonly string europeDivisions;

        public MainframeStoreDAO(string europeDivisions)
        {
            _USdatabase = DatabaseFactory.CreateDatabase("DB2PROD_DRIVER");
            _Europedatabase = DatabaseFactory.CreateDatabase("DB2PROD_DRIVER");
            this.europeDivisions = europeDivisions;
        }

        public List<MainframeStore> GetClosingDates(List<string> stores, string div)
        {
            List<MainframeStore> _que;
            _que = new List<MainframeStore>();

            Database db;

            if (europeDivisions.Contains(div))            
                db = _Europedatabase;            
            else            
                db = _USdatabase;            

            DbCommand SQLCommandMF;
            string SQLMF;
            SQLMF = "select str_num, STR_CLOSED_DT from TC070017 where str_num in (";
            bool notfirst = false;
            foreach (string store in stores)
            {
                if (notfirst)                
                    SQLMF += ",";
                
                SQLMF += "'" + store + "'";
                notfirst = true;
            }

            SQLMF += ") ";

            SQLCommandMF = db.GetSqlStringCommand(SQLMF);
            DataSet data = db.ExecuteDataSet(SQLCommandMF);
            MainframeStore mfStore;
            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    mfStore = new MainframeStore()
                    {
                        Division = div,
                        Store = Convert.ToString(dr["str_num"]),
                        ClosedDate = Convert.ToString(dr["STR_CLOSED_DT"])
                    };
                    _que.Add(mfStore);
                }
            }
            return _que;
        }        
    }
}