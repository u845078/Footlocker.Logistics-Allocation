using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Services
{
    /// <summary>
    /// Provides an object representation of an Interface Log Data Access Object.
    /// </summary>
    public class InterfaceLogDAO : Db2DAO
    {
        /// <summary>
        /// Defines the interface identifier length.
        /// </summary>
        private const int InterfaceIdentifierLength = 2;

        /// <summary>
        /// Defines the interface file name length.
        /// </summary>
        private const int InterfaceFileNameLength = 8;

        /// <summary>
        /// Defines the updating program length.
        /// </summary>
        private const int UpdatingProgramLength = 8;

        /// <summary>
        /// Defines the creating program name length.
        /// </summary>
        private const int CreatingProgramNameLength = 8;

        /// <summary>
        /// Defines the interface file description length.
        /// </summary>
        private const int InterfaceFileDescLength = 40;


        public string ConnectionString
        {
            get 
            {
                return this.Db.ConnectionStringWithoutCredentials;
            }
        }

        /// <summary>
        /// Initializes a new instance of the InterfaceLogDAO class.
        /// </summary>
        /// <param name="identifier">The interface identifier to use during initialization.</param>
        public InterfaceLogDAO(QuantumInstance instance)
            : base(instance)
        {
        }

        /// <summary>
        /// Insert an interface log.
        /// </summary>
        /// <param name="file">The interface file for which the interface log will be inserted.</param>
        /// <param name="creatingProgramName">The creating program name of the interface log to be inserted.</param>
        /// <param name="serverRecordCount">The server record count of the interface log to be inserted.</param>
        public void Insert(InterfaceFile file, string creatingProgramName, decimal serverRecordCount)
        {
            this.Insert(file.Name, creatingProgramName, file.Description, serverRecordCount);
        }

        /// <summary>
        /// Insert an interface log.
        /// </summary>
        /// <param name="interfaceFileName">The interface file name of the interface log to be inserted.</param>
        /// <param name="creatingProgramName">The creating program name of the interface log to be inserted.</param>
        /// <param name="interfaceFileDescription">The interface file description of the interface log to be inserted.</param>
        /// <param name="serverRecordCount">The server record count of the interface log to be inserted.</param>
        /// <exception cref="DataException">Thrown when an exception occurs while inserting the interface log.</exception>
        public void Insert(string interfaceFileName, string creatingProgramName, string interfaceFileDescription
            , decimal serverRecordCount)
        {
            string sql = @"INSERT
                           INTO     TCQTM002
                                  ( INTERFACE_ID
                                  , INTERFCE_FILE_NAME
                                  , CREATE_DTTM
                                  , UPDATING_PGM
                                  , CRTE_PGM_NAME
                                  , INTERFCE_FILE_DESC
                                  , HOST_REC_COUNT
                                  , SERVER_REC_COUNT
                                  , LAST_DTTM_UPDATE
                                  )
                           VALUES ( ?
                                  , ?
                                  , CURRENT_TIMESTAMP
                                  , SPACE(8)
                                  , ?
                                  , ?
                                  , 0
                                  , ?
                                  , '9999-12-31-23:59:59.999999'
                                  )";

            try
            {
                using (DbCommand command = this.Db.GetSqlStringCommand(sql))
                {
                    this.Db.AddInParameter(command, "INTERFACE_ID", DbType.String
                        , InterfaceLogDAO.ConvertStringToDb2Char(this.Instance.InterfaceId
                            , InterfaceLogDAO.InterfaceIdentifierLength));
                    this.Db.AddInParameter(command, "INTERFCE_FILE_NAME", DbType.String
                        , InterfaceLogDAO.ConvertStringToDb2Char(interfaceFileName
                            , InterfaceLogDAO.InterfaceFileNameLength));
                    this.Db.AddInParameter(command, "CRTE_PGM_NAME", DbType.String
                        , InterfaceLogDAO.ConvertStringToDb2Char(creatingProgramName
                            , InterfaceLogDAO.CreatingProgramNameLength));
                    this.Db.AddInParameter(command, "INTERFCE_FILE_DESC", DbType.String
                        , InterfaceLogDAO.ConvertStringToDb2Char(interfaceFileDescription
                            , InterfaceLogDAO.InterfaceFileDescLength));
                    this.Db.AddInParameter(command, "SERVER_REC_COUNT", DbType.Decimal, serverRecordCount);
                    this.Db.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                throw new DataException(
                    String.Format(
                        "An exception occurred while inserting an interface log row (interface identifier: {0}, interface file name: {1}, creating program name: {2}, interface file description: {3}, server record count {4})."
                        , this.Instance.InterfaceId, interfaceFileName, creatingProgramName, interfaceFileDescription
                        , serverRecordCount)
                    , ex);
            }
        }

        /// <summary>
        /// Update an interface log.
        /// </summary>
        /// <param name="file">The interface file of the interface log to be updated.</param>
        /// <param name="updatingProgram">THe updating program of the interface log to be updated.</param>
        /// <param name="serverRecordCount">The server record count of the interface log to be updated.</param>
        public void Update(InterfaceFile file, string updatingProgram, decimal serverRecordCount)
        {
            this.Update(file.Name, updatingProgram, serverRecordCount);
        }

        /// <summary>
        /// Update an interface log.
        /// </summary>
        /// <param name="interfaceFileName">The interface file name of the interface log to be updated.</param>
        /// <param name="updatingProgram">The updating program of the interface log to be updated.</param>
        /// <param name="serverRecordCount">The server record count of the interface log to be updated.</param>
        /// <exception cref="DataException">Thrown when an exception occurs while updating the interface log.</exception>
        public void Update(string interfaceFileName, string updatingProgram, decimal serverRecordCount)
        {
            string sql = @"UPDATE   TCQTM002
                          SET      UPDATING_PGM = ?
                                 , SERVER_REC_COUNT = ?
                                 , LAST_DTTM_UPDATE = CURRENT_TIMESTAMP
                          WHERE    INTERFACE_ID = ?
                                       AND INTERFCE_FILE_NAME = ?
                                       AND LAST_DTTM_UPDATE = '9999-12-31-23:59:59.999999'";

            try
            {
                using (DbCommand command = this.Db.GetSqlStringCommand(sql))
                {
                    this.Db.AddInParameter(command, "UPDATING_PGM", DbType.String
                        , InterfaceLogDAO.ConvertStringToDb2Char(updatingProgram
                            , InterfaceLogDAO.UpdatingProgramLength));
                    this.Db.AddInParameter(command, "SERVER_REC_COUNT", DbType.Decimal, serverRecordCount);
                    this.Db.AddInParameter(command, "INTERFACE_ID", DbType.String
                        , InterfaceLogDAO.ConvertStringToDb2Char(this.Instance.InterfaceId
                            , InterfaceLogDAO.InterfaceIdentifierLength));
                    this.Db.AddInParameter(command, "INTERFCE_FILE_NAME", DbType.String
                        , InterfaceLogDAO.ConvertStringToDb2Char(interfaceFileName
                            , InterfaceLogDAO.InterfaceFileNameLength));
                    this.Db.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                throw new DataException(
                    String.Format(
                        "An exception occurred while updating an interface log row (interface identifier: {0}, interface file name: {1}, updating program: {2}, server record count: {3})."
                        , this.Instance.InterfaceId, interfaceFileName, updatingProgram, serverRecordCount)
                    , ex);
            }
        }

        /// <summary>
        /// Get a host record count.
        /// </summary>
        /// <param name="file">The interface file for the host record count to get.</param>
        /// <returns>The host record count for the specified interface file name.</returns>
        public decimal GetHostRecordCount(InterfaceFile file)
        {
            return this.GetHostRecordCount(file.Name);
        }

        /// <summary>
        /// Get a host record count.
        /// </summary>
        /// <param name="interfaceFileName">The interface file name for the host record count to get.</param>
        /// <returns>The host record count for the specified interface file name.</returns>
        /// <exception cref="DataException">Thrown when an exception occurs while getting a host record count.</exception>
        public decimal GetHostRecordCount(string interfaceFileName)
        {
            decimal returnValue = Decimal.Zero;
            string sql = @"SELECT   HOST_REC_COUNT
                           FROM     TCQTM002
                           WHERE    INTERFACE_ID = ?
                                        AND INTERFCE_FILE_NAME = ?
                                        AND LAST_DTTM_UPDATE = '9999-12-31-23:59:59.999999'";
            object hostRecordCount = null;

            try
            {
                using (DbCommand command = this.Db.GetSqlStringCommand(sql))
                {
                    this.Db.AddInParameter(command, "INTERFACE_ID", DbType.String
                        , InterfaceLogDAO.ConvertStringToDb2Char(this.Instance.InterfaceId
                            , InterfaceLogDAO.InterfaceIdentifierLength));
                    this.Db.AddInParameter(command, "INTERFCE_FILE_NAME", DbType.String
                        , InterfaceLogDAO.ConvertStringToDb2Char(interfaceFileName
                            , InterfaceLogDAO.InterfaceFileNameLength));
                    hostRecordCount = this.Db.ExecuteScalar(command);
                    if (hostRecordCount == null)
                    {
                        throw new Exception("An interface log was not found.");
                    }
                    else
                    {
                        returnValue = Convert.ToDecimal(hostRecordCount);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DataException(
                    String.Format(
                        "An exception occurred while getting a host record count (interface identifier: {0}, interface file name: {1})."
                        , this.Instance.InterfaceId, interfaceFileName)
                    , ex);
            }
            return returnValue;
        }

        /// <summary>
        /// Validate the record counts.
        /// </summary>
        /// <param name="file">The interface file of the interface log with the host record count to be validated.</param>
        /// <param name="updatingProgram">The updating program of the validated interface log.</param>
        /// <param name="serverRecordCount">The server record count to be validated.</param>
        public void ValidateRecordCounts(InterfaceFile file, string updatingProgram, decimal serverRecordCount)
        {
            this.ValidateRecordCounts(file.Name, updatingProgram, serverRecordCount);
        }

        /// <summary>
        /// Validate the record counts.
        /// </summary>
        /// <param name="interfaceFileName">The interface file name of the interface log with the host record count to be validated.</param>
        /// <param name="updatingProgram">The updating program of the validated interface log.</param>
        /// <param name="serverRecordCount">The server record count to be validated.</param>
        /// <exception cref="Exception">Thrown when the host and server record counts are different.</exception>
        public void ValidateRecordCounts(string interfaceFileName, string updatingProgram, decimal serverRecordCount)
        {
            decimal hostRecordCount = this.GetHostRecordCount(interfaceFileName);

            if (hostRecordCount == serverRecordCount)
            {
                this.Update(interfaceFileName, updatingProgram, serverRecordCount);
            }
            else
            {
                throw new Exception(
                    String.Format(
                        "Host record count and server record count are different (interface identifier: {0}, interface file name: {1}, host record count: {2}, server record count: {3})."
                        , this.Instance.InterfaceId, interfaceFileName, hostRecordCount, serverRecordCount));
            }
        }

        /// <summary>
        /// Validate the file against the interface log.
        /// </summary>
        /// <param name="fileName">The name of the file to be validated.</param>
        /// <param name="file"The interface file of the file to be validated.></param>
        /// <param name="updatingProgram">The name of the updating program.</param>
        /// <returns>The number of records in the file.</returns>
        public decimal ValidateFile(string fileName, InterfaceFile file, string updatingProgram)
        {
            return this.ValidateFile(fileName, file.Name, updatingProgram);
        }

        /// <summary>
        /// Validate the file against the interface log.
        /// </summary>
        /// <param name="fileName">The name of the file to be validated.</param>
        /// <param name="interfaceFileName">The interface file name of the file to be validated.</param>
        /// <param name="updatingProgram">The name of the updating program.</param>
        /// <returns>The number of records in the file.</returns>
        /// <exception cref="Exception">Thrown when an exception occurs while validating the file.</exception>
        public decimal ValidateFile(string fileName, string interfaceFileName, string updatingProgram)
        {
            decimal returnValue = Decimal.Zero;
            bool readingFile = true;

            try
            {
                using (StreamReader reader = new StreamReader(fileName, UTF7Encoding.ASCII))
                {
                    while (readingFile)
                    {
                        if (reader.EndOfStream)
                        {
                            readingFile = false;
                        }
                        else
                        {
                            reader.ReadLine();
                            returnValue++;
                        }
                    }
                    reader.Close();
                }
                this.ValidateRecordCounts(interfaceFileName, updatingProgram, returnValue);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    String.Format("An exception occurred while validating a file (file name: {0}).", fileName)
                    , ex);
            }
            return returnValue;
        }
    }
}
