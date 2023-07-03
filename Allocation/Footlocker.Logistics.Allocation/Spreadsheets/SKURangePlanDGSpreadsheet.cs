using Aspose.Excel;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using Footlocker.Logistics.Allocation.Common;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Spreadsheet
{
    public class SKURangePlanDGSpreadsheet : UploadSpreadsheet
    {
        public List<DeliveryGroupUploadModel> parsedDeliveryGroups = new List<DeliveryGroupUploadModel>();
        public List<DeliveryGroupUploadModel> errorList = new List<DeliveryGroupUploadModel>();
        readonly RangePlanDAO rangeDAO;
        private List<DeliveryGroup> updatedDeliveryGroups = new List<DeliveryGroup>();

        private DeliveryGroupUploadModel ParseUploadRow(int row)
        {
            DeliveryGroupUploadModel returnValue = new DeliveryGroupUploadModel
            {
                SKU = Convert.ToString(worksheet.Cells[row, 0].Value).Trim(),
                DeliveryGroupName = Convert.ToString(worksheet.Cells[row, 1].Value).Trim()                
            };

            if (!string.IsNullOrEmpty(worksheet.Cells[row, 2].Value.ToString()))
                returnValue.StartDate = Convert.ToDateTime(worksheet.Cells[row, 2].Value);

            if (!string.IsNullOrEmpty(worksheet.Cells[row, 3].Value.ToString()))
                returnValue.EndDate = Convert.ToDateTime(worksheet.Cells[row, 3].Value);

            if (!string.IsNullOrEmpty(worksheet.Cells[row, 4].Value.ToString()))
                returnValue.MinEndDays = Convert.ToInt32(worksheet.Cells[row, 4].Value);

            returnValue.RangePlanID = rangeDAO.GetRangePlanID(returnValue.SKU);
            returnValue.DeliveryGroup = config.db.DeliveryGroups.Where(dg => dg.PlanID == returnValue.RangePlanID && 
                                                                             dg.Name == returnValue.DeliveryGroupName).FirstOrDefault();

            if (string.IsNullOrEmpty(worksheet.Cells[row, 2].Value.ToString()) &&
                string.IsNullOrEmpty(worksheet.Cells[row, 3].Value.ToString()) &&
                string.IsNullOrEmpty(worksheet.Cells[row, 4].Value.ToString()))
                returnValue.ErrorMessage = "Missing required fields";

            return returnValue;
        }

        private void ValidateRow(DeliveryGroupUploadModel row)
        {
            if (!config.currentUser.HasDivDept(config.AppName, row.Division, row.Department))
                row.ErrorMessage = string.Format("You do not have permission for the division/department {0}/{1}.", row.Division, row.Department);

            if (row.DeliveryGroup == null)
                row.ErrorMessage = "Range plan for this SKU and/or Delivery Group Name was not found";
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
                        DeliveryGroupUploadModel newRow = ParseUploadRow(row);
                        ValidateRow(newRow);

                        if (string.IsNullOrEmpty(newRow.ErrorMessage))
                            parsedDeliveryGroups.Add(newRow);
                        else
                            errorList.Add(newRow);

                        row++;
                    }

                    if (parsedDeliveryGroups.Count > 0)
                    {
                        foreach (DeliveryGroupUploadModel rec in parsedDeliveryGroups)
                        {
                            rec.ApplyValues();
                            updatedDeliveryGroups.Add(rec.DeliveryGroup);
                        }

                        rangeDAO.UpdateDeliveryGroups(updatedDeliveryGroups, config.currentUser.NetworkID);
                    }

                    // if errors occured, allow user to download them
                    if (errorList.Count > 0)
                    {
                        errorMessage = string.Format("{0} errors were found and {1} lines were processed successfully. ", errorList.Count, parsedDeliveryGroups.Count);
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

        public SKURangePlanDGSpreadsheet(AppConfig config, ConfigService configService, RangePlanDAO rangePlanDAO) : base(config, configService)
        {
            maxColumns = 5;

            columns.Add(0, "Sku");
            columns.Add(1, "Delivery Group Name");
            columns.Add(2, "Start Date");
            columns.Add(3, "End Date");
            columns.Add(4, "Min End Days");

            templateFilename = config.SKURangePlanDGUploadTemplate;
            rangeDAO = rangePlanDAO;
        }
    }
}