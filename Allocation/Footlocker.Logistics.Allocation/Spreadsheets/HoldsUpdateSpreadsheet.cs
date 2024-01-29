using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Aspose.Cells;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class HoldsUpdateSpreadsheet : UploadSpreadsheet
    {
        public List<HoldsUploadUpdateModel> validHolds = new List<HoldsUploadUpdateModel>();
        public List<HoldsUploadUpdateModel> errorList = new List<HoldsUploadUpdateModel>();

        private HoldsUploadUpdateModel ParseRow(DataRow row)
        {
            HoldsUploadUpdateModel newItem = new HoldsUploadUpdateModel() 
            {
                Division = row[0].ToString().Trim(),
                Store = row[1].ToString().Trim(),
                Level = row[2].ToString().Trim(),
                Value = row[3].ToString().Trim(),
                StartDate = DateTime.Parse(row[4].ToString().Trim()),
                Duration = row[6].ToString().Trim(),
                HoldType = row[7].ToString().Trim(),
                Comments = row[8].ToString().Trim()                
            };

            if (!string.IsNullOrEmpty(row[5].ToString().Trim()))
                newItem.EndDate = DateTime.Parse(row[5].ToString().Trim());

            if (string.IsNullOrEmpty(newItem.Store))
                newItem.Store = "";

            return newItem;
        }

        private Hold GetHoldForItem(HoldsUploadUpdateModel item)
        {
            Hold holdRec;

            holdRec = config.db.Holds.Where(h => h.Division == item.Division &&
                                                 h.Store == item.Store &&
                                                 h.Level == item.Level &&
                                                 h.Value == item.Value &&
                                                 h.StartDate == item.StartDate).FirstOrDefault();
            return holdRec;
        }

        private void ValidateRow(HoldsUploadUpdateModel item)
        {
            int holdCount;

            if (string.IsNullOrEmpty(item.Division))
                item.ErrorMessage = "Division is a required field";

            if (string.IsNullOrEmpty(item.Level) && string.IsNullOrEmpty(item.ErrorMessage))
                item.ErrorMessage = "Level is a required field";

            if (string.IsNullOrEmpty(item.Value) && string.IsNullOrEmpty(item.ErrorMessage))
                item.ErrorMessage = "Value is a required field";

            if (item.StartDate == DateTime.MinValue && string.IsNullOrEmpty(item.ErrorMessage))
                item.ErrorMessage = "Start Date is not a valid value - it is required";

            if (item.EndDate.HasValue)
            {
                if (item.EndDate.Value.Date < DateTime.Now.Date && string.IsNullOrEmpty(item.ErrorMessage))
                    item.ErrorMessage = "You can't back-date the End Date. It can be today if you want to expire the hold.";
            }

            if (item.Duration != "Permanent" && item.Duration != "Temporary" && string.IsNullOrEmpty(item.ErrorMessage))
                item.ErrorMessage = "Duration must be either Permanent or Temporary";

            if (item.HoldType != "Reserve Inventory" && item.HoldType != "Cancel Inventory" && string.IsNullOrEmpty(item.ErrorMessage))
                item.ErrorMessage = "Hold Type must be either 'Reserve Inventory' or 'Cancel Inventory'"; 
            
            if (string.IsNullOrEmpty(item.ErrorMessage))
            {
                holdCount = config.db.Holds.Where(h => h.Division == item.Division &&
                                                       h.Store == item.Store &&
                                                       h.Level == item.Level &&
                                                       h.Value == item.Value &&
                                                       h.StartDate == item.StartDate).Count();

                if (holdCount == 0)
                    item.ErrorMessage = string.Format("Could not find existing holds to update for Div: {0}, Store: {1}, Level: {2}, Value: {3}, Start Date: {4}",
                        item.Division, item.Store, item.Level, item.Value, item.StartDate.ToShortDateString());
            }
        }        

        public void Save(HttpPostedFileBase file)
        {
            HoldsUploadUpdateModel item;

            LoadAttachment(file.InputStream);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 1;

                try
                {
                    foreach (DataRow dataRow in excelData.Rows)
                    {
                        item = ParseRow(dataRow);
                        ValidateRow(item);

                        if (!string.IsNullOrEmpty(item.ErrorMessage))
                            errorList.Add(item);
                        else                        
                            validHolds.Add(item);                        

                        row++;
                    }

                    foreach (HoldsUploadUpdateModel hold in validHolds)
                    {
                        Hold changeHold = GetHoldForItem(hold);

                        if (hold.EndDate.HasValue)
                            changeHold.EndDate = hold.EndDate.Value;

                        changeHold.Duration = hold.Duration;
                        changeHold.HoldType = hold.HoldType;
                        changeHold.Comments = hold.Comments;
                        changeHold.CreateDate = DateTime.Now;
                        changeHold.CreatedBy = config.currentUser.NetworkID;

                        config.db.Entry(changeHold).State = EntityState.Modified;
                    }

                    config.db.SaveChanges();
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public Workbook GetErrors(List<HoldsUploadUpdateModel> errorList)
        {
            if (errorList != null)
            {
                Workbook excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (HoldsUploadUpdateModel p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Division);
                    mySheet.Cells[row, 1].PutValue(p.Store);
                    mySheet.Cells[row, 2].PutValue(p.Level);
                    mySheet.Cells[row, 3].PutValue(p.Value);
                    mySheet.Cells[row, 4].PutValue(p.StartDate);
                    mySheet.Cells[row, 5].PutValue(p.EndDate);
                    mySheet.Cells[row, 6].PutValue(p.Duration);
                    mySheet.Cells[row, 7].PutValue(p.HoldType);
                    mySheet.Cells[row, 8].PutValue(p.Comments);

                    mySheet.Cells[row, maxColumns].PutValue(p.ErrorMessage);
                    mySheet.Cells[row, maxColumns].SetStyle(errorStyle);
                    row++;
                }

                mySheet.AutoFitColumn(maxColumns);

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

        public HoldsUpdateSpreadsheet(AppConfig config, ConfigService configService) : base(config, configService)
        {
            maxColumns = 9;
            headerRowNumber = 0;

            columns.Add(0, "Division");
            columns.Add(1, "Store");
            columns.Add(2, "Level");
            columns.Add(3, "Value");
            columns.Add(4, "Start Date");
            columns.Add(5, "End Date");
            columns.Add(6, "Duration");
            columns.Add(7, "Hold Type");
            columns.Add(8, "Comments");

            templateFilename = config.HoldUpdateTemplate;
        }
    }
}