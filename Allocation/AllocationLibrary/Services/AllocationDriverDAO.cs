using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;

namespace Footlocker.Logistics.Allocation.Services
{
    public class AllocationDriverDAO
    {
        Database _database;
        Database _USdatabase;
        Database _Europedatabase;
        string _prefix;
        readonly string europeDivisions;
        

        public AllocationDriverDAO(string europeDivisions, string db2PrefixDriver)
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
            _USdatabase = DatabaseFactory.CreateDatabase("DB2PROD_DRIVER");
            _Europedatabase = DatabaseFactory.CreateDatabase("DB2EURP_DRIVER");
            _prefix = db2PrefixDriver;
            this.europeDivisions = europeDivisions;
        }
        
        public void Save(AllocationDriver objectToSave, string user, bool updateMF)
        {
            if (updateMF)
            {
                Database db;

                if (europeDivisions.Contains(objectToSave.Division))                
                    db = _Europedatabase;                
                else                
                    db = _USdatabase;                

                DbCommand SQLCommandMF;
                string SQLMF;
                try
                {
                    SQLMF = "insert into " + _prefix + "TCQTM001 values (?, ?, ?, ?, ?, ?)";                                        

                    SQLCommandMF = db.GetSqlStringCommand(SQLMF);
                    db.AddInParameter(SQLCommandMF, "@1", DbType.String, objectToSave.Division);
                    db.AddInParameter(SQLCommandMF, "@2", DbType.String, objectToSave.Department);
                    db.AddInParameter(SQLCommandMF, "@3", DbType.String, objectToSave.ConvertDate.ToString("yyyy-MM-dd"));
                    db.AddInParameter(SQLCommandMF, "@4", DbType.String, objectToSave.AllocateDate.ToString("yyyy-MM-dd"));
                    db.AddInParameter(SQLCommandMF, "@5", DbType.String, user);
                    db.AddInParameter(SQLCommandMF, "@6", DbType.String, DateTime.Now.ToString("yyyy-MM-dd"));

                    db.ExecuteNonQuery(SQLCommandMF);
                }
                catch
                {
                    SQLMF = "update " + _prefix + "TCQTM001 set ";
                    SQLMF += " convert_date = ?, ";
                    SQLMF += " allocate_date = ?, ";
                    SQLMF += " user_name = ?, ";
                    SQLMF += " create_date = ? ";
                    SQLMF += " where retl_oper_div_code = ? ";
                    SQLMF += " and stk_dept_num = ? ";

                    SQLCommandMF = db.GetSqlStringCommand(SQLMF);
                    db.AddInParameter(SQLCommandMF, "@1", DbType.String, objectToSave.ConvertDate.ToString("yyyy-MM-dd"));
                    db.AddInParameter(SQLCommandMF, "@2", DbType.String, objectToSave.AllocateDate.ToString("yyyy-MM-dd"));
                    db.AddInParameter(SQLCommandMF, "@3", DbType.String, user);
                    db.AddInParameter(SQLCommandMF, "@4", DbType.String, DateTime.Now.ToString("yyyy-MM-dd"));
                    db.AddInParameter(SQLCommandMF, "@5", DbType.String, objectToSave.Division);
                    db.AddInParameter(SQLCommandMF, "@6", DbType.String, objectToSave.Department);

                    db.ExecuteNonQuery(SQLCommandMF);
                }
            }

            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.SaveAllocationDriver";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@Division", DbType.String, objectToSave.Division);
            _database.AddInParameter(SQLCommand, "@Department", DbType.String, objectToSave.Department);
            _database.AddInParameter(SQLCommand, "@AllocateDate", DbType.DateTime, objectToSave.AllocateDate);
            _database.AddInParameter(SQLCommand, "@ConvertDate", DbType.DateTime, objectToSave.ConvertDate);
            _database.AddInParameter(SQLCommand, "@OrderPlanningDate", DbType.DateTime, objectToSave.OrderPlanningDate);
            _database.AddInParameter(SQLCommand, "@CheckNormals", DbType.Boolean, objectToSave.CheckNormals);
            _database.AddInParameter(SQLCommand, "@MinihubInd", DbType.Boolean, objectToSave.StockedInMinihub);
            _database.AddInParameter(SQLCommand, "@CHANGE_BY", DbType.String, user);

            _database.ExecuteNonQuery(SQLCommand);
        }
        
        public List<AllocationDriver> GetAllocationDriverList(string division)
        {
            List<AllocationDriver> _que;
            _que = new List<AllocationDriver>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetAllocationDrivers";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@Division", DbType.String, division);

            DataSet data;
            data = _database.ExecuteDataSet(SQLCommand);

            AllocationDriverFactory factory = new AllocationDriverFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        public List<AllocationDriver> GetAllocationDriversForInstance(int instance)
        {
            List<AllocationDriver> _que;
            _que = new List<AllocationDriver>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetAllocationDriversForInstance";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instance", DbType.Int32, instance);

            DataSet data;
            data = _database.ExecuteDataSet(SQLCommand);

            AllocationDriverFactory factory = new AllocationDriverFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        public List<AllocationDriver> GetActiveAllocationDriversForInstance(int instance)
        {
            List<AllocationDriver> _que;
            _que = new List<AllocationDriver>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetActiveAllocationDriversForInstance";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instance", DbType.Int32, instance);

            DataSet data;
            data = _database.ExecuteDataSet(SQLCommand);

            AllocationDriverFactory factory = new AllocationDriverFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        public AllocationDriver GetAllocationDriver(string div, string dept)
        {
            DbCommand SQLCommand;
            string SQL = "dbo.GetAllocationDriver";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@dept", DbType.String, dept);

            DataSet data;
            data = _database.ExecuteDataSet(SQLCommand);

            AllocationDriverFactory factory = new AllocationDriverFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    return factory.Create(dr);
                }
            }
            return null;
        }
        
        public void DeleteAllocationDriver(string div, string dept)
        {
            DbCommand SQLCommandMF;
            string SQLMF;

            Database db;

            if (europeDivisions.Contains(div))            
                db = _Europedatabase;            
            else            
                db = _USdatabase;            

            SQLMF = "delete from " + _prefix + "TCQTM001 ";
            SQLMF += " where retl_oper_div_code = ? and ";
            SQLMF += "  stk_dept_num = ? ";

            SQLCommandMF = db.GetSqlStringCommand(SQLMF);
            db.AddInParameter(SQLCommandMF, "@1", DbType.String, div);
            db.AddInParameter(SQLCommandMF, "@2", DbType.String, dept);            

            db.ExecuteNonQuery(SQLCommandMF);

            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.DeleteAllocationDriver";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@dept", DbType.String, dept);

            _database.ExecuteNonQuery(SQLCommand);
        }
    }
}