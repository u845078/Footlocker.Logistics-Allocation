using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
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
            
            SQLCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@category", DbType.String, category);

            DataSet data;
            data = _database.ExecuteDataSet(SQLCommand);

            Department dept;
            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    dept = new Department()
                    {
                        DivCode = Convert.ToString(dr["div"]),
                        DeptNumber = Convert.ToString(dr["dept"])
                    };

                    list.Add(dept);
                }
            }

            return list;
        }
    }
}