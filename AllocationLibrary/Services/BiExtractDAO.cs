using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Practices.EnterpriseLibrary.Data;

using Footlocker.Common;
using Footlocker.Common.Utilities;
using Footlocker.Common.Utilities.File;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Services
{
    /// <summary>
    /// Provides an object representation of a business intelligence extract data access object.
    /// </summary>
    public static class BiExtractDAO
    {
        /// <summary>
        /// Gets the log count.
        /// </summary>
        private static long LogCount
        {
            get { return Convert.ToInt64(ConfigurationManager.AppSettings["logcount"]); }
        }

        /// <summary>
        /// Extract business intellegence extract items.
        /// </summary>
        /// <param name="request">The business intelligence extract request.</param>
        /// <param name="extractPath">The path to which the business intelligence extract items will be written.</param>
        /// <param name="log">The current logger.</param>
        /// <param name="instance">The Quantum instance.</param>
        public static void Extract(BiExtractRequest request, string extractPath, LogService log, QuantumInstance instance)
        {
            long count = 0;
            Database db = null;
            DbCommand SqlCommand = null;
            IDataReader reader = null;
            IBiExtractFactory<BiExtract> factory = null;
            BiExtract item = null;
            List<ValidStoreLookup> allStores = null;

            log.Log(
                String.Format("Processing {0}.  Quantum Instance is {1}.", request.Request.ToString()
                    , instance.ToString()));
            db = DatabaseService.GetSqlDatabase("AllocationContext");
            SqlCommand = db.GetStoredProcCommand(request.GetStoredProcedureName());
            SqlCommand.CommandTimeout = 3600;
            db.AddInParameter(SqlCommand, "@instanceId", DbType.Int32, Convert.ToInt32(instance.Id));
            reader = db.ExecuteReader(SqlCommand);
            using (StreamWriter writer = new StreamWriter(extractPath))
            {
                factory = request.GetFactory();
                while (reader.Read())
                {
                    item = factory.Create(reader);
                    if (item.CopyForAllStoresInDivision())
                    {
                        if (allStores == null)
                        {
                            AllocationLibraryContext ac = new AllocationLibraryContext();
                            int instanceid = Convert.ToInt32(instance.Id);
                            allStores = (from a in ac.vValidStores join b in ac.InstanceDivisions on a.Division equals b.Division where (b.InstanceID == instanceid) select a).ToList();
                        }
                        foreach (ValidStoreLookup store in allStores)
                        {
                            if (item.SubstituteStore(store.Division, store.Store))
                            {
                                writer.WriteLine(item.ToString('|'));
                                count++;
                                if ((count % LogCount) == 0)
                                {
                                    log.Log(String.Format("Processed {0} items.", count));
                                    writer.Flush();
                                }
                            }
                        }
                    }
                    else
                    {
                        writer.WriteLine(item.ToString('|'));
                        count++;
                        if ((count % LogCount) == 0)
                        {
                            log.Log(String.Format("Processed {0} items.", count));
                            writer.Flush();
                        }
                    }
                }
                writer.Flush();
                writer.Close();
                log.Log(String.Format("Processed {0} items for {1}.", count, request.Request.ToString()));
            }

            /*------------------------------FTP SERVICE------------------------------*/

            //if (System.Configuration.ConfigurationManager.AppSettings["BIExtracts_FTPToProd"].Equals("False"))
            //{
            //    FTPService ftpDev = new FTPService(System.Configuration.ConfigurationManager.AppSettings["BIExtracts_FTPTargetServer"], System.Configuration.ConfigurationManager.AppSettings["BIExtracts_FTPDevUserName"], System.Configuration.ConfigurationManager.AppSettings["BIExtracts_FTPDevPassword"]);
            //    ftpDev.Connect(0, 0);
            //    ftpDev.ChangeDirectory(System.Configuration.ConfigurationManager.AppSettings["BIExtracts_FTPDevRemotePath"]);
            //    ftpDev.SendFile(extractPath, Path.GetFileName(extractPath));
            //    ftpDev.Quit();
            //}
            //else
            //{
            //    FTPService ftpProd = new FTPService(System.Configuration.ConfigurationManager.AppSettings["BIExtracts_FTPTargetServer"], System.Configuration.ConfigurationManager.AppSettings["BIExtracts_FTPProdUserName"], System.Configuration.ConfigurationManager.AppSettings["BIExtracts_FTPProdPassword"]);
            //    ftpProd.Connect(0, 0);
            //    ftpProd.ChangeDirectory(System.Configuration.ConfigurationManager.AppSettings["BIExtracts_FTPProdRemotePath"]);
            //    ftpProd.SendFile(extractPath, Path.GetFileName(extractPath));
            //    ftpProd.Quit();
            //}
        }

        /// <summary>
        /// Archive the business intelligence extract.
        /// </summary>
        /// <param name="extractPath">The business intelligence extract path.</param>
        /// <param name="archivePath">The archive path.</param>
        /// <param name="archivePeriod">The archive period in days.</param>
        public static void Archive(string extractPath, string archivePath, int archivePeriod)
        {
            FileService.ArchiveFile(extractPath, archivePath, true);
            FileService.PurgeFolder(archivePath, archivePeriod);
        }
    }
}
