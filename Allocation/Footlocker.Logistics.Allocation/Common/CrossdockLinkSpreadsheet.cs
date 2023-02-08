using Aspose.Excel;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Common
{
    public class CrossdockLinkSpreadsheet : UploadSpreadsheet
    {
        public List<POCrossdockData> errorList = new List<POCrossdockData>();
        public List<POCrossdockData> validPOCrossdocks = new List<POCrossdockData>();

        private POCrossdockData ParseUploadRow(int row)
        {
            POCrossdockData returnValue = new POCrossdockData
            {
                WarehouseID = Convert.ToString(worksheet.Cells[row, 0].Value).Trim(),
                Division = Convert.ToString(worksheet.Cells[row, 1].Value).Trim(),
                PO = Convert.ToString(worksheet.Cells[row, 2].Value).Trim(),
                ExpectedReceiptDateString = Convert.ToString(worksheet.Cells[row, 3].Value),
                CancelIndString = Convert.ToString(worksheet.Cells[row, 4].Value).ToUpper(),                
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
                if (string.IsNullOrEmpty(returnValue.ErrorMessage))
                    returnValue.InstanceID = configService.GetInstance(returnValue.Division);
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
            LoadAttachment(attachment);

            if (!HasValidHeaderRow())
                message = "Upload failed: Incorrect header - please use template.";
            else
            {
                int row = 1;

                try
                {
                    while (HasDataOnRow(row))
                    {
                        POCrossdockData newRec = ParseUploadRow(row);

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

        public Excel GetErrors(List<POCrossdockData> errorList)
        {
            if (errorList != null)
            {
                Excel excelDocument = GetTemplate();
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
                    mySheet.Cells[row, 5].Style.Font.Color = Color.Red;
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