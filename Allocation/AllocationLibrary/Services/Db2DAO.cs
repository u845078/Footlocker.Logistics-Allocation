//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Text;

//using Microsoft.Practices.EnterpriseLibrary.Data;

//using Footlocker.Logistics.Allocation.Models;

//namespace Footlocker.Logistics.Allocation.Services
//{
//    /// <summary>
//    /// Provides an object representation of a database two data access object.
//    /// </summary>
//    public class Db2DAO
//    {
//        /// <summary>
//        /// Gets the command timeout.
//        /// </summary>
//        protected static int CommandTimeout
//        {
//            get
//            {
//                int returnValue = 0;
//                string commandTimeout = ConfigurationManager.AppSettings["CommandTimeout"];

//                returnValue = Convert.ToInt32(commandTimeout);
//                return returnValue;
//            }
//        }
//        /// <summary>
//        /// Gets or sets the Quantum instance.
//        /// </summary>
//        protected QuantumInstance Instance { get; private set; }

//        /// <summary>
//        /// Gets or set the database context.
//        /// </summary>
//        protected Database Db { get; private set; }

//        /// <summary>
//        /// Initialize a new instance of the Db2DAO class.
//        /// </summary>
//        /// <param name="identifier">The interface identifier to be used during initialization.</param>
//        protected Db2DAO(QuantumInstance instance)
//            : this(instance, Db2DAO.GetDatabaseFromInstance(instance))
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of the Db2DAO class.
//        /// </summary>
//        /// <param name="instance">The interface identifier to use during initialization.</param>
//        /// <param name="db">The database to use during initialization.</param>
//        protected Db2DAO(QuantumInstance instance, Database db)
//        {
//            this.Instance = instance;
//            this.Db = db;
//        }

//        /// <summary>
//        /// Convert the specified string to a DB2 character field.
//        /// </summary>
//        /// <param name="value">The string to be converted.</param>
//        /// <param name="maxLength">The maximum length of the DB2 character field.</param>
//        /// <returns>The converted value.</returns>
//        protected static string ConvertStringToDb2Char(string value, int maxLength)
//        {
//            string truncatedString = value.Length > maxLength ? value.Substring(0, maxLength) : value;

//            return truncatedString.ToUpper();
//        }

//        /// <summary>
//        /// Get database from the Quantum instance.
//        /// </summary>
//        /// <param name="instance">The Quantum instance for which the database is required.</param>
//        /// <returns>The database corresponding to the specified Quantum instance.</returns>
//        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown Quantum instance is specified.</exception>
//        private static Database GetDatabaseFromInstance(QuantumInstance instance)
//        {
//            Database returnValue = null;
//            string connectionString = String.Empty;

//            if (instance.IsNorthAmerica() || instance.IsAsiaPacific())
//            {
//                connectionString = "DB2PROD";
//            }
//            else
//            {
//                if (instance.IsEurope())
//                {
//                    connectionString = "DB2EURP";
//                }
//                else
//                {
//                    throw new ArgumentOutOfRangeException("instance", instance
//                        , "An unknown interface identifier has been specified.");
//                }
//            }
//            returnValue = DatabaseFactory.CreateDatabase(connectionString);
//            return returnValue;
//        }
//    }
//}
