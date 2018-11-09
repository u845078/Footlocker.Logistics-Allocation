
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Services
{
    public class CategoryDAO
    {
        Database _database;

        public CategoryDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public List<Department> GetDepartmentsForCategory(string div, string category)
        {
            List<Department> list = new List<Department>();
            DbCommand SQLCommand;
            string SQL = "dbo.[GetDeptForCategory]";
            Microsoft.Practices.EnterpriseLibrary.Data.Database _database = Footlocker.Common.DatabaseService.GetSqlDatabase("AllocationContext");
            SQLCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@category", DbType.String, category);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            Department dept;
            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    dept = new Department();
                    dept.DivCode = Convert.ToString(dr["div"]);
                    dept.DeptNumber = Convert.ToString(dr["dept"]);
                    list.Add(dept);
                }
            }

            return list;

        }
    }
}