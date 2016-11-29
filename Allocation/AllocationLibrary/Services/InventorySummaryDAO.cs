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
    public class InventorySummaryDAO
    {
        Database _database;

        public InventorySummaryDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        
        public List<InventorySummary> GetInventorySummaryList(long itemid)
        {
            List<InventorySummary> _que;
            _que = new List<InventorySummary>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetInventorySummary";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@itemid", DbType.Int64, itemid);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            InventorySummaryFactory factory = new InventorySummaryFactory();

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
