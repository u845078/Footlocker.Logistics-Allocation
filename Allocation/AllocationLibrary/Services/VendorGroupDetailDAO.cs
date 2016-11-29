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
    public class VendorGroupDetailDAO
    {
        Database _database;
        Database _databaseDB2;

        public VendorGroupDetailDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
            _databaseDB2 = DatabaseFactory.CreateDatabase("DB2PROD");
        }

        
        public List<VendorGroupDetail> GetVendorGroupDetails(int ID)
        {
            List<VendorGroupDetail> _que;
            _que = new List<VendorGroupDetail>();

            DbCommand SQLCommand;
            string SQL = "dbo.[getVendorGroupDetails]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@ID", DbType.String, ID);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            VendorGroupDetailFactory factory = new VendorGroupDetailFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        public Boolean IsVendorSetupForEDI(string vendor)
        {
            List<VendorGroupDetail> _que;
            _que = new List<VendorGroupDetail>();

            DbCommand SQLCommand;
            string SQL = "select AUTO_APPROVAL_IND from TCEDI001 where ";
            SQL = SQL + " VND_ID='" + vendor.PadLeft(5,'0') + "'";

            SQLCommand = _databaseDB2.GetSqlStringCommand(SQL);

            DataSet data = new DataSet();
            data = _databaseDB2.ExecuteDataSet(SQLCommand);

            VendorGroupDetailFactory factory = new VendorGroupDetailFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    if (Convert.ToString(dr["AUTO_APPROVAL_IND"]) == "Y")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
