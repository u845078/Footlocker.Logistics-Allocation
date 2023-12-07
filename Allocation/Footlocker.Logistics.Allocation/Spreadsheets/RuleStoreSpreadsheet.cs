using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class RuleStoreSpreadsheet : UploadExcelSpreadsheet
    {
        public List<StoreBase> LoadedStores = new List<StoreBase>();
        public string MainDivision;

        private StoreBase ParseRow(int row)
        {
            StoreBase returnValue = new StoreBase()
            {
                Division = Convert.ToString(worksheet.Cells[row, 0].Value).PadLeft(2, '0'),
                Store = Convert.ToString(worksheet.Cells[row, 1].Value).PadLeft(5, '0')
            };

            return returnValue;
        }

        private bool ValidateUploadValues(StoreBase storeRec)
        {
            return storeRec.Store != "00000";
        }

        /// <summary>
        /// This is going to load the stores into the loaded
        /// </summary>
        /// <param name="attachment"></param>
        public void Save(HttpPostedFileBase attachment)
        {
            StoreBase uploadRec;

            LoadAttachment(attachment);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 1;

                try
                {
                    while (HasDataOnRow(row))
                    {
                        uploadRec = ParseRow(row);

                        if (ValidateUploadValues(uploadRec))
                            LoadedStores.Add(uploadRec);

                        row++;
                    }

                    if (LoadedStores.Count > 0)
                        MainDivision = LoadedStores[0].Division;
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public RuleStoreSpreadsheet(AppConfig config, ConfigService configService) : base(config, configService)
        {
            maxColumns = 2;
            headerRowNumber = 0;

            columns.Add(0, "Division");
            columns.Add(1, "Store");

            templateFilename = config.StoreTemplate;
        }
    }
}