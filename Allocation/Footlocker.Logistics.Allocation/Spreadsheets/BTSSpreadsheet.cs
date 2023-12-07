using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Web;
using Footlocker.Logistics.Allocation.Common;
using System.Linq;
using Aspose.Excel;
using System.Drawing;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class BTSSpreadsheet : UploadExcelSpreadsheet
    {
        public List<StoreBTSDetail> errorList = new List<StoreBTSDetail>();
        public List<StoreBTSDetail> validData = new List<StoreBTSDetail>();
        int groupID;
        StoreBTS group;
        int year;

        private StoreBTSDetail ParseRow(int row)
        {
            StoreBTSDetail returnValue = new StoreBTSDetail()
            {
                Division = Convert.ToString(worksheet.Cells[row, 0].Value).PadLeft(2, '0'),
                Store = Convert.ToString(worksheet.Cells[row, 1].Value).PadLeft(5, '0'),
                GroupID = groupID,
                Year = year,
                CreateDate = DateTime.Now,
                CreatedBy = config.currentUser.NetworkID
            };

            return returnValue;
        }

        private void ValidateUploadValues(StoreBTSDetail uploadRec)
        {
            bool foundStore = config.db.vValidStores.Where(vs => vs.Division == uploadRec.Division && vs.Store == uploadRec.Store).Count() > 0;

            if (!foundStore)
                uploadRec.errorMessage = string.Format("Store '{0}-{1}' was not found to be a valid store. Please only enter existing, valid stores.", uploadRec.Division, uploadRec.Store);
        }

        public void Save(HttpPostedFileBase attachment, int groupID)
        {
            StoreBTSDetail uploadRec;            

            LoadAttachment(attachment);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 1;
                this.groupID = groupID;
                group = config.db.StoreBTS.Where(sb => sb.ID == groupID).First();
                year = group.Year;

                try
                {
                    while (HasDataOnRow(row))
                    {
                        uploadRec = ParseRow(row);
                        ValidateUploadValues(uploadRec);

                        if (!string.IsNullOrEmpty(uploadRec.errorMessage))
                            errorList.Add(uploadRec);
                        else
                            validData.Add(uploadRec);

                        row++;
                    }

                    if (validData.Count > 0)
                    {
                        foreach (StoreBTSDetail rec in validData)
                            config.db.StoreBTSDetails.Add(rec);

                        config.db.SaveChanges();
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

        public Excel GetErrors(List<StoreBTSDetail> errorList)
        {
            if (errorList != null)
            {
                Excel excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (StoreBTSDetail p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Division);
                    mySheet.Cells[row, 1].PutValue(p.Store);
                    mySheet.Cells[row, 2].PutValue(p.errorMessage);
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

        public BTSSpreadsheet(AppConfig config, ConfigService configService) : base(config, configService)
        {
            maxColumns = 2;
            headerRowNumber = 0;

            columns.Add(0, "Division");
            columns.Add(1, "Store");

            templateFilename = config.SeasonalityTemplate;
        }
    }
}