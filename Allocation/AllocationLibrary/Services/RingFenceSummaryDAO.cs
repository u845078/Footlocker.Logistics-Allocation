
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;

namespace Footlocker.Logistics.Allocation.Models.Services
{
    public class RingFenceSummaryDAO
    {
        Database _database;

        public RingFenceSummaryDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        
        public List<RingFenceSummary> GetRingFenceSummaries(string instanceID)
        {
            List<RingFenceSummary> _que;
            _que = new List<RingFenceSummary>();

            DbCommand SQLCommand;
            string SQL = "dbo.[GetRingFences]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instanceID", DbType.String, instanceID);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RingFenceSummaryFactory factory = new RingFenceSummaryFactory();

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