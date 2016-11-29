using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Services
{
    public class WSMDAO
    {
        Database _database;

        public WSMDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public List<WSM> GetWSM(string sku)
        {
            List<WSM> list = new List<WSM>();
            string SQL = "dbo.[GetWSM]";
            var sqlCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(sqlCommand, "@sku", DbType.String, sku);
            sqlCommand.CommandTimeout = 300;

            DataSet data = _database.ExecuteDataSet(sqlCommand);

            WSMFactory factory = new WSMFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    list.Add(factory.Create(dr));
                }
            }

            return list;
        }

        public List<QuantumSeasonalityData> getQuantumSeasonalityData(string sku)
        {
            List<QuantumSeasonalityData> results = new List<QuantumSeasonalityData>();
            string SQL = "dbo.[GetSeasonalityData]";
            var sqlCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(sqlCommand, "@sku", DbType.String, sku);
            sqlCommand.CommandTimeout = 300;

            DataSet data = _database.ExecuteDataSet(sqlCommand);

            WSMFactory factory = new WSMFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    results.Add(factory.CreateSeasonalData(dr));
                }
            }

            return results;
        }

    }
}
