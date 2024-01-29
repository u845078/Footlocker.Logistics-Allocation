using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data;
using Aspose.Cells;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class SkuRangeSpreadsheet : UploadSpreadsheet
    {
        public List<BulkRange> errorList = new List<BulkRange>();
        public List<BulkRange> validRanges = new List<BulkRange>();
        public List<BulkRange> parsedRanges = new List<BulkRange>();

        private BulkRange ParseRow(DataRow row)
        {
            BulkRange returnValue = new BulkRange()
            {
                Range = "1",
                Division = Convert.ToString(row[0]).Trim().PadLeft(2, '0'),
                Store = Convert.ToString(row[3]).Trim().PadLeft(5, '0'),
                Sku = Convert.ToString(row[4]).Trim(),
                Size = Convert.ToString(row[5]).Trim().PadLeft(3, '0').ToUpper(),
                RangeStartDate = Convert.ToString(row[6]).Trim(),
                DeliveryGroupName = Convert.ToString(row[7]).Trim(),
                Min = Convert.ToString(row[8]).Trim(),
                Max = Convert.ToString(row[9]).Trim(),
                MinEndDaysOverride = Convert.ToString(row[11]).Trim(),
                EndDate = Convert.ToString(row[12]),
                OPStartSend = Convert.ToString(row[13]),
                OPStopSend = Convert.ToString(row[14]),
                OPRequestComments = Convert.ToString(row[15])
            };

            string baseDemand = Convert.ToString(row[10]).Trim();

            if (!string.IsNullOrEmpty(baseDemand))            
                returnValue.BaseDemand = Convert.ToDecimal(row[10]).ToString();
            else            
                returnValue.BaseDemand = "";            

            //doing this to preserve nulls for blank
            if (returnValue.Min == "")
                returnValue.Min = "-1";

            if (returnValue.Max == "")
                returnValue.Max = "-1";

            if (returnValue.BaseDemand == "")
                returnValue.BaseDemand = "-1";
            
            if (returnValue.MinEndDaysOverride == "")            
                returnValue.MinEndDaysOverride = "-1";
            
            if (returnValue.EndDate == "")            
                returnValue.EndDate = "-1";            

            return returnValue;
        }

        private bool ValidateRow(BulkRange range, out string errorMessage)
        {
            errorMessage= string.Empty;
            string[] validFormats = { "M/d/yyyy", "M/d/yyyy hh:mm:ss tt" };
            DateTime parsedDate;

            Regex regexSku = new Regex(@"^\d{2}-\d{2}-\d{5}-\d{2}$");
            if (!regexSku.IsMatch(range.Sku))
                errorMessage = "Invalid Sku, format should be ##-##-#####-##";

            if (!config.currentUser.HasDivision(config.AppName, range.Division))
                errorMessage = string.Format("You are not authorized to update division {0}", range.Division);

            //ensure the store is valid            
            if (!config.db.StoreLookups.Where(sl => sl.Division == range.Division && sl.Store == range.Store).Any())
                errorMessage = "The division and store combination does not exist within the system.";

            if (!string.IsNullOrEmpty(range.RangeStartDate))
                if (!DateTime.TryParseExact(range.RangeStartDate, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    errorMessage = "Delivery group start date is not in a mm/dd/yyyy format";

            if (range.EndDate != "-1")
                if (!DateTime.TryParseExact(range.EndDate, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    errorMessage = "Store End Date override is not in a mm/dd/yyyy format";

            if (!string.IsNullOrEmpty(range.OPStartSend))
                if (!DateTime.TryParseExact(range.OPStartSend, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    errorMessage = "OP Start Send Date is not in a mm/dd/yyyy format";

            if (!string.IsNullOrEmpty(range.OPStopSend))
                if (!DateTime.TryParseExact(range.OPStopSend, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    errorMessage = "OP End Send Date is not in a mm/dd/yyyy format";

            return string.IsNullOrEmpty(errorMessage);
        }

        private void ValidateList()
        {

        }

        public void Save(HttpPostedFileBase attachment, bool purgeFirst)
        {
            RangePlanDetailDAO dao = new RangePlanDetailDAO();
            List<BulkRange> bulkloadErrorList;
            BulkRange range;
            string errorMessage;

            LoadAttachment(attachment.InputStream);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 1;
                try
                {
                    foreach (DataRow dataRow in excelData.Rows)
                    {
                        range = ParseRow(dataRow);
                        
                        if (!ValidateRow(range, out errorMessage))
                            range.Error = errorMessage;

                        if (!string.IsNullOrEmpty(range.Error))
                            errorList.Add(range);
                        else
                            parsedRanges.Add(range);
                        
                        row++;
                    }

                    ValidateList();

                    foreach (BulkRange bulkRange in parsedRanges)
                    {
                        if (!string.IsNullOrEmpty(bulkRange.Error))
                            errorList.Add(bulkRange);
                        else
                            validRanges.Add(bulkRange);
                    }
                    
                    bulkloadErrorList = dao.BulkUpdateRange(validRanges, config.currentUser.NetworkID, purgeFirst);
                    errorList.AddRange(bulkloadErrorList);
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public Workbook GetErrors(List<BulkRange> errorList)
        {
            if (errorList != null)
            {                
                excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (BulkRange p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Division);
                    mySheet.Cells[row, 1].PutValue(p.League);
                    mySheet.Cells[row, 2].PutValue(p.Region);
                    mySheet.Cells[row, 3].PutValue(p.Store);
                    mySheet.Cells[row, 4].PutValue(p.Sku);
                    mySheet.Cells[row, 5].PutValue(p.Size);
                    mySheet.Cells[row, 6].PutValue(p.RangeStartDate);
                    mySheet.Cells[row, 7].PutValue(p.DeliveryGroupName);
                    mySheet.Cells[row, 8].PutValue(p.Min);
                    mySheet.Cells[row, 9].PutValue(p.Max);
                    mySheet.Cells[row, 10].PutValue(p.BaseDemand);
                    mySheet.Cells[row, 11].PutValue(p.MinEndDaysOverride);
                    mySheet.Cells[row, 12].PutValue(p.EndDate);
                    mySheet.Cells[row, 13].PutValue(p.OPStartSend);
                    mySheet.Cells[row, 14].PutValue(p.OPStopSend);
                    mySheet.Cells[row, 15].PutValue(p.OPRequestComments);
                    mySheet.Cells[row, maxColumns].PutValue(p.Error);
                    mySheet.Cells[row, maxColumns].SetStyle(errorStyle);
                    row++;
                }

                return excelDocument;
            }
            else
            {
                // if this message is hit that means there was an exception while processing that was not accounted for
                // check the log to see what the exception was
                message = "An unexpected error has occured.  Please try again or contact an administrator.";
                return null;
            }
        }

        public SkuRangeSpreadsheet(AppConfig config, ConfigService configService) : base(config, configService)
        {
            maxColumns = 16;

            columns.Add(0, "Division");
            columns.Add(1, "League");
            columns.Add(2, "Region");
            columns.Add(3, "Store");
            columns.Add(4, "Sku");
            columns.Add(5, "Size");
            columns.Add(6, "Delivery Group Start Date");
            columns.Add(7, "Delivery Group Name");
            columns.Add(8, "Min");
            columns.Add(9, "Max");
            columns.Add(10, "Base Demand");
            columns.Add(11, "Min End Days Override");
            columns.Add(12, "Store End Date Override");
            columns.Add(13, "OP Start Send Date");
            columns.Add(14, "OP End Send Date");
            columns.Add(15, "OP Comments");

            templateFilename = config.RangeTemplate;
        }
    }
}