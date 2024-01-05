using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using Footlocker.Logistics.Allocation.Common;
using System.Data;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class StoreSpreadsheet : UploadSpreadsheet
    {
        readonly RangePlanDAO rangeDAO;   
        readonly RuleDAO ruleDAO;
        public List<StoreBase> validStores = new List<StoreBase>();
        RangePlan range;

        private StoreBase ParseRow(DataRow row)
        {
            message = string.Empty;
            string rangeType;

            StoreBase returnValue = new StoreBase()
            {
                Division = Convert.ToString(row[0]).PadLeft(2, '0'),
                Store = Convert.ToString(row[1]).PadLeft(5, '0')
            };

            if (row[2] != null)
                rangeType = row[2].ToString();
            else
                rangeType = "ALR";

            if (rangeType == "ALR" || rangeType == "OP")
                returnValue.RangeType = rangeType;

            //always default to "Both"
            if (string.IsNullOrEmpty(returnValue.RangeType))
                returnValue.RangeType = "Both";

            return returnValue;
        }

        private string ValidateUploadValues(StoreBase parsedRec)
        {
            string errorMessage = string.Empty;
            Regex validStoreNumber = new Regex("^[0-9][0-9][0-9][0-9][0-9]$");

            if (parsedRec.Division == "00")
                errorMessage = "Division cannot be empty or 00";
            else if (range.Division != parsedRec.Division)
                errorMessage = "The division does not match the division for the range plan";
            else
            {
                if (parsedRec.Store == "00000")
                    errorMessage = "Store cannot be empty or 00000";
                else
                    if (!validStoreNumber.IsMatch(parsedRec.Store))
                    errorMessage = "The store format does not look valid. It must be a five digit numeric number";
            }

            return errorMessage;
        }

        public void Save(HttpPostedFileBase attachment, long planID)
        {
            StoreBase uploadRec;

            LoadAttachment(attachment.InputStream);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 1;
                range = rangeDAO.GetRangePlan(planID);

                try
                {
                    foreach (DataRow dataRow in excelData.Rows)
                    {
                        uploadRec = ParseRow(dataRow);
                      
                        errorMessage = ValidateUploadValues(uploadRec);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            message = string.Format("Row {0}: {1}", row, errorMessage);
                            return;
                        }
                        else                        
                            validStores.Add(uploadRec);                        

                        row++;
                    }

                    if (validStores.Count > 0)                    
                        ruleDAO.AddStoresToPlan(validStores, planID);                    

                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public StoreSpreadsheet(AppConfig config, ConfigService configService, RangePlanDAO rangePlanDAO, RuleDAO ruleDAO) : base(config, configService)
        {
            maxColumns = 2;
            headerRowNumber = 0;

            columns.Add(0, "Division");
            columns.Add(1, "Store");

            rangeDAO = rangePlanDAO;

            templateFilename = config.StoreTemplate;
            this.ruleDAO = ruleDAO;
        }
    }
}