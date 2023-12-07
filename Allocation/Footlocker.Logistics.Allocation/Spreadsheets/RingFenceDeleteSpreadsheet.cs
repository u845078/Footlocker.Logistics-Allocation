using Aspose.Excel;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class RingFenceDeleteSpreadsheet : UploadExcelSpreadsheet
    {
        public List<RingFenceUploadModel> errorList = new List<RingFenceUploadModel>();
        public List<RingFenceUploadModel> validRingFenceDeletes = new List<RingFenceUploadModel>();
        public List<RingFenceUploadModel> parsedRingFenceDeletes = new List<RingFenceUploadModel>();

        private RingFenceUploadModel ParseRow(int row)
        {
            RingFenceUploadModel returnValue = new RingFenceUploadModel()
            {
                Store = Convert.ToString(worksheet.Cells[row, 0].Value),
                SKU = Convert.ToString(worksheet.Cells[row, 1].Value),                
                Division = Convert.ToString(worksheet.Cells[row, 1].Value).Substring(0, 2)
            };

            return returnValue;
        }

        private bool ValidateRow(RingFenceUploadModel inputData, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!config.currentUser.HasDivision(config.AppName, inputData.Division))
                errorMessage = string.Format("You are not authorized to update division {0}", inputData.Division);
            else
            {
                if (!config.db.RingFences.Any(rf => rf.Sku == inputData.SKU))
                    errorMessage = "There are no ring fences for this SKU";
                else
                {
                    if (!config.db.RingFences.Any(rf => rf.Sku == inputData.SKU && rf.Store == inputData.Store))
                        errorMessage = "There are no ring fences for this SKU/Store combination";
                }
            }

            return string.IsNullOrEmpty(errorMessage);
        }

        private void ValidateList()
        {
            var duplicates = (from id in validRingFenceDeletes
                              group id by new { f1 = id.SKU, f2 = id.Store } into g
                              where g.Count() > 1
                              select g).ToList();

            foreach (var dup in duplicates)
            {
                foreach (var plDup in validRingFenceDeletes.Where(x => x.SKU == dup.Key.f1 && x.Store == dup.Key.f2))
                {
                    plDup.ErrorMessage = "Duplicate ring fence record";
                    errorList.Add(plDup);
                }
            }

            validRingFenceDeletes.RemoveAll(rfd => !string.IsNullOrEmpty(rfd.ErrorMessage));
        }

        public void Save(HttpPostedFileBase attachment)
        {
            RingFenceUploadModel uploadRec;
            List<RingFenceUploadModel> skuOnlyList;
            List<RingFenceUploadModel> skuStoreList;

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
                        parsedRingFenceDeletes.Add(uploadRec);

                        if (!ValidateRow(uploadRec, out errorMessage))
                            uploadRec.ErrorMessage = errorMessage;

                        if (string.IsNullOrEmpty(uploadRec.ErrorMessage))
                            validRingFenceDeletes.Add(uploadRec);
                        else
                            errorList.Add(uploadRec);

                        row++;
                    }

                    ValidateList();

                    if (validRingFenceDeletes.Count > 0)
                    {
                        config.db.Configuration.AutoDetectChangesEnabled = false;
                        config.db.Configuration.LazyLoadingEnabled = false;
                        config.db.Configuration.ProxyCreationEnabled = false;
                        config.db.Configuration.ValidateOnSaveEnabled = false;

                        skuOnlyList = validRingFenceDeletes.Where(id => string.IsNullOrEmpty(id.Store)).ToList();
                        skuStoreList = validRingFenceDeletes.Where(id => !string.IsNullOrEmpty(id.Store)).ToList();

                        var rfToDelete = (from pl in skuStoreList
                                          join r in config.db.RingFences
                                          on new { sku = pl.SKU, store = pl.Store } equals new { sku = r.Sku, store = r.Store }
                                          select r.ID).ToList();

                        if (skuOnlyList.Count() > 0)
                        {
                            rfToDelete.AddRange((from pl in skuOnlyList
                                                 join r in config.db.RingFences
                                                 on pl.SKU equals r.Sku
                                                 select r.ID).Except(rfToDelete).ToList());
                        }

                        var rfdToDelete = config.db.RingFenceDetails.Where(rfd => rfToDelete.Contains(rfd.RingFenceID)).ToList();

                        foreach (var rfd in rfdToDelete)
                            config.db.RingFenceDetails.Remove(rfd);

                        var rfhToDelete = config.db.RingFenceHistory.Where(rfh => rfToDelete.Contains(rfh.RingFenceID)).ToList();

                        foreach (var rfh in rfhToDelete)
                            config.db.RingFenceHistory.Remove(rfh);

                        var deleteRF = config.db.RingFences.Where(rf => rfToDelete.Contains(rf.ID)).ToList();

                        foreach (var rf in deleteRF)                        
                            config.db.RingFences.Remove(rf);

                        config.db.SaveChanges(config.currentUser.NetworkID);                        
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

        public Excel GetErrors(List<RingFenceUploadModel> errorList)
        {
            if (errorList != null)
            {
                Excel excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (RingFenceUploadModel p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Store);
                    mySheet.Cells[row, 1].PutValue(p.SKU);
                    mySheet.Cells[row, 2].PutValue(p.ErrorMessage);
                    mySheet.Cells[row, 2].Style.Font.Color = Color.Red;
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

        public RingFenceDeleteSpreadsheet(AppConfig config, ConfigService configService) : base(config, configService)
        {
            maxColumns = 2;

            columns.Add(0, "Store");
            columns.Add(1, "SKU");

            templateFilename = config.RingFenceDeleteTemplate;
        }
    }
}