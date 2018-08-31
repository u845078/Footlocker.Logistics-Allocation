
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
    public class LegacyInventoryDAO
    {
        Database _database;

        public LegacyInventoryDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        
        public List<LegacyInventory> GetLegacyInventoryForSku(string sku)
        {
            List<LegacyInventory> _que;
            _que = new List<LegacyInventory>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetLegacyInventoryForSku";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@sku", DbType.String, sku);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            LegacyInventoryFactory factory = new LegacyInventoryFactory();

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