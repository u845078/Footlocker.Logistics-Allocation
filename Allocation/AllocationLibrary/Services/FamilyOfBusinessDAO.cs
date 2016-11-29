
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Models.Factories;

namespace Footlocker.Logistics.Allocation.Services
{
    public class FamilyOfBusinessDAO
    {
        Database _database;

        public FamilyOfBusinessDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public List<FamilyOfBusiness> GetFOBs(string div)
        {
            List<FamilyOfBusiness> list = new List<FamilyOfBusiness>();
            DbCommand SQLCommand;
            string SQL = "dbo.[GetFOBs]";

            SQLCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            FamilyOfBusinessFactory factory =new FamilyOfBusinessFactory();
            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    list.Add(factory.Create(dr));
                }
            }

            return list;

        }
    }
}