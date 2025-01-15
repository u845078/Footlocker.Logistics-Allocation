using Aspose.Cells;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models.Services;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Globalization;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class EcomRFRestrictionSpreadsheet : UploadSpreadsheet
    {
        readonly ItemDAO itemDAO;
        public List<EcomRFRestriction> validRecs = new List<EcomRFRestriction>();
        public List<EcomRFRestriction> errorList = new List<EcomRFRestriction>();

        private EcomRFRestriction ParseRow(DataRow row)
        {
            EcomRFRestriction newEcomRFRestriction = new EcomRFRestriction()
            {
                SKU = Convert.ToString(row[0]).Trim(),
                StartDateString = Convert.ToString(row[1]).Trim(),
                EndDateString = Convert.ToString(row[2]).Trim(),
                LastModifiedDate = DateTime.Now,
                LastModifiedUser = config.currentUser.NetworkID
            };

            return newEcomRFRestriction;
        }

        private bool ValidateRow(EcomRFRestriction item)
        { 
            DateTime parsedDate;

            if (string.IsNullOrEmpty(item.SKU))
                item.ErrorMessage = "SKU is missing and is required";
            else
            {
                item.ItemID = itemDAO.GetItemID(item.SKU);
                if (item.ItemID == 0)
                    item.ErrorMessage = "Invalid SKU: it was not found in the database";
                else
                {
                    if (string.IsNullOrEmpty(item.StartDateString))
                        item.ErrorMessage = "Start Date is missing and is required";
                    else
                    {
                        if (!DateTime.TryParseExact(item.StartDateString, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                            errorMessage += "Start Date is not in a mm/dd/yyyy format ";
                        else
                        {
                            item.StartDate = parsedDate;

                            if (!string.IsNullOrEmpty(item.EndDateString))
                            {
                                if (!DateTime.TryParseExact(item.EndDateString, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                                    errorMessage += "If you provide End Date, it must be in a mm/dd/yyyy format ";
                                else
                                    item.EndDate = parsedDate;
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(item.ErrorMessage))
            {
                if (item.EndDate.HasValue)
                {
                    if (item.EndDate < item.StartDate)
                        item.ErrorMessage = "Your End Date cannot be before your Start Date.";
                }
            }

            if (string.IsNullOrEmpty(item.ErrorMessage))
            {
                int recCount = config.allocDB.EcomRFRestictions.Where(erfr => erfr.ItemID == item.ItemID).Count();
                if (recCount > 0)
                    item.ErrorMessage = "There is a restriction for this SKU already.";
            }

            return string.IsNullOrEmpty(item.ErrorMessage);
        }

        public void Save(HttpPostedFileBase attachment)
        {
            EcomRFRestriction item;

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
                        item = ParseRow(dataRow);

                        if (!ValidateRow(item))
                            errorList.Add(item);
                        else
                            validRecs.Add(item);

                        row++;
                    }

                    foreach (EcomRFRestriction rec in validRecs)
                    {
                        config.allocDB.EcomRFRestictions.Add(rec);
                    }

                    config.allocDB.SaveChanges();
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public Workbook GetErrors(List<EcomRFRestriction> errorList)
        {
            if (errorList != null)
            {
                excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (EcomRFRestriction p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.SKU);
                    mySheet.Cells[row, 1].PutValue(p.StartDate);
                    mySheet.Cells[row, 2].PutValue(p.EndDate);
                    mySheet.Cells[row, maxColumns].PutValue(p.ErrorMessage);
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

        public EcomRFRestrictionSpreadsheet(AppConfig config, ConfigService configService, ItemDAO itemDAO) : base(config, configService)
        {
            maxColumns = 3;
            headerRowNumber = 0;

            columns.Add(0, "SKU");
            columns.Add(1, "Start Date");
            columns.Add(2, "End Date");

            templateFilename = config.EcomRFRestrictionsTemplate;

            this.itemDAO = itemDAO;
        }
    }
}