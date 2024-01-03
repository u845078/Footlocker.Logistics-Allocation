using Aspose.Cells;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class CrossdockLinkSpreadsheet : UploadSpreadsheet
    {
        public List<POCrossdockData> errorList = new List<POCrossdockData>();
        public List<POCrossdockData> validPOCrossdocks = new List<POCrossdockData>();

        private POCrossdockData ParseUploadRow(DataRow row)
        {
            POCrossdockData returnValue = new POCrossdockData
            {
                WarehouseID = Convert.ToString(row[0]).Trim(),
                Division = Convert.ToString(row[1]).Trim(),
                PO = Convert.ToString(row[2]).Trim(),
                ExpectedReceiptDateString = Convert.ToString(row[3]),
                CancelIndString = Convert.ToString(row[4]).ToUpper(),                
                LastModifiedDate = DateTime.Now,
                LastModifiedUser = config.currentUser.NetworkID
            };

            if (string.IsNullOrEmpty(returnValue.WarehouseID))
                returnValue.ErrorMessage = "Warehouse ID is a mandatory field";

            if (!string.IsNullOrEmpty(returnValue.CancelIndString))
                returnValue.CancelInd = returnValue.CancelIndString == "Y";
            else
                returnValue.ErrorMessage = "You must supply a value for the Cancel PO field";

            if (string.IsNullOrEmpty(returnValue.Division))
                returnValue.ErrorMessage = "Division is a mandatory field";

            if (string.IsNullOrEmpty(returnValue.PO))
                returnValue.ErrorMessage = "PO is a mandatory field";

            if (!string.IsNullOrEmpty(returnValue.ExpectedReceiptDateString))
                returnValue.ExpectedReceiptDate = Convert.ToDateTime(returnValue.ExpectedReceiptDateString);

            try
            {
                int instanceID;

                if (string.IsNullOrEmpty(returnValue.ErrorMessage))
                    instanceID = configService.GetInstance(returnValue.Division);
            }
            catch 
            {
                returnValue.ErrorMessage = "Division is not a valid value";
            }

            return returnValue;
        }

        private void ValidateList()
        {
            var duplicates = (from id in validPOCrossdocks
                              group id by new { f1 = id.Division, f2 = id.PO } into g
                              where g.Count() > 1
                              select g).ToList();

            foreach (var dup in duplicates)
            {
                foreach (var plDup in validPOCrossdocks.Where(x => x.Division == dup.Key.f1 && x.PO == dup.Key.f2))
                {
                    plDup.ErrorMessage = "Duplicate Crossdock Link record";
                    errorList.Add(plDup);
                }
            }

            foreach (POCrossdockData rec in validPOCrossdocks.Where(vp => string.IsNullOrEmpty(vp.ErrorMessage)))
            {
                if (config.db.POs.Where(p => p.Division == rec.Division && p.PO == rec.PO).Count() == 0)
                {
                    rec.ErrorMessage = "Division and PO combination was not found on PO table";
                    errorList.Add(rec);
                }

                if (string.IsNullOrEmpty(rec.ErrorMessage))
                {
                    if (config.db.POCrossdocks.Where(pc => pc.Division == rec.Division && pc.PO == rec.PO).Count() > 0)
                    {
                        rec.ErrorMessage = "Division and PO combination have already been submitted";
                        errorList.Add(rec);
                    }
                }
            }           

            validPOCrossdocks.RemoveAll(rfd => !string.IsNullOrEmpty(rfd.ErrorMessage));
        }

        public void Save(HttpPostedFileBase attachment)
        {
            LoadAttachment(attachment.InputStream);

            if (!HasValidHeaderRow())
                message = "Upload failed: Incorrect header - please use template.";
            else
            {
                int row = 1;

                try
                {
                    foreach (DataRow dataRow in excelData.Rows)
                    {
                        POCrossdockData newRec = ParseUploadRow(dataRow);

                        if (string.IsNullOrEmpty(newRec.ErrorMessage))
                            validPOCrossdocks.Add(newRec);
                        else
                            errorList.Add(newRec);
                        
                        row++;
                    }
                    
                    ValidateList();

                    if (validPOCrossdocks.Count > 0)
                    {
                        if (string.IsNullOrEmpty(message))
                        {
                            foreach (POCrossdockData rec in validPOCrossdocks)
                                config.db.POCrossdocks.Add(rec);

                            config.db.SaveChanges(config.currentUser.NetworkID);
                        }                            
                    }
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public Workbook GetErrors(List<POCrossdockData> errorList)
        {
            if (errorList != null)
            {
                Workbook excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (POCrossdockData p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.WarehouseID);
                    mySheet.Cells[row, 1].PutValue(p.Division);
                    mySheet.Cells[row, 2].PutValue(p.PO);
                    mySheet.Cells[row, 3].PutValue(p.ExpectedReceiptDateString);
                    mySheet.Cells[row, 4].PutValue(p.CancelIndString);

                    mySheet.Cells[row, 5].PutValue(p.ErrorMessage);
                    mySheet.Cells[row, 5].SetStyle(errorStyle);
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

        public CrossdockLinkSpreadsheet(AppConfig config, ConfigService configService) : base(config, configService)
        {
            maxColumns = 5;

            columns.Add(0, "Warehouse ID");
            columns.Add(1, "Division");
            columns.Add(2, "PO Number");
            columns.Add(3, "Expected Receipt Date");
            columns.Add(4, "Cancel PO?");

            templateFilename = config.CrossdockLinkTemplate;
        }
    }
}