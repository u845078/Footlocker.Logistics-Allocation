
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
    public class EcommWeightDAO
    {
        Database _database;

        public EcommWeightDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        
        public List<EcommWeight> GetEcommWeightList(string dept)
        {
            List<EcommWeight> _que;
            _que = new List<EcommWeight>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetEcommWeights";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@dept", DbType.String, dept);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            EcommWeightFactory factory = new EcommWeightFactory();

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