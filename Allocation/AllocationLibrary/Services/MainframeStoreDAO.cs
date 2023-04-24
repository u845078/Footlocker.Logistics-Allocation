
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
        //string _prefix;

        public MainframeStoreDAO()
        {
            _USdatabase = DatabaseFactory.CreateDatabase("DB2PROD_DRIVER");
            _Europedatabase = DatabaseFactory.CreateDatabase("DB2PROD_DRIVER");
            //_prefix = System.Configuration.ConfigurationManager.AppSettings["DB2PREFIX"];
        }


        public List<MainframeStore> GetClosingDates(List<string> stores, string div)
        {
            List<MainframeStore> _que;
            _que = new List<MainframeStore>();

            Database db;

            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(div))
            {
                db = _Europedatabase;
            }
            else
            {
                db = _USdatabase;
            }

            DbCommand SQLCommandMF;
            string SQLMF;
            SQLMF = "select str_num, STR_CLOSED_DT from TC070017 where str_num in (";
            Boolean notfirst = false;
            foreach (string store in stores)
            {
                if (notfirst)
                {
                    SQLMF = SQLMF + ",";
                }
                SQLMF = SQLMF + "'" + store + "'";
                notfirst = true;
            }

            SQLMF = SQLMF + ") ";

            SQLCommandMF = db.GetSqlStringCommand(SQLMF);
            DataSet data = db.ExecuteDataSet(SQLCommandMF);
            MainframeStore mfStore;
            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    mfStore = new MainframeStore();
                    mfStore.Division = div;
                    mfStore.Store = Convert.ToString(dr["str_num"]);
                    mfStore.ClosedDate = Convert.ToString(dr["STR_CLOSED_DT"]);
                    _que.Add(mfStore);
                }
            }
            return _que;
        }

        
    }
}