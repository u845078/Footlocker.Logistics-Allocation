using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using Footlocker.Logistics.Allocation.Common;
using System.Linq;
using System.Web;
using System.Data;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class SKURangePlanDGSpreadsheet : UploadSpreadsheet
    {
        public List<DeliveryGroupUploadModel> parsedDeliveryGroups = new List<DeliveryGroupUploadModel>();
        public List<DeliveryGroupUploadModel> errorList = new List<DeliveryGroupUploadModel>();
        readonly RangePlanDAO rangeDAO;
        private List<DeliveryGroup> updatedDeliveryGroups = new List<DeliveryGroup>();

        private DeliveryGroupUploadModel ParseUploadRow(DataRow row)
        {
            DeliveryGroupUploadModel returnValue = new DeliveryGroupUploadModel
            {
                SKU = Convert.ToString(row[0]).Trim(),
                DeliveryGroupName = Convert.ToString(row[1]).Trim()                
            };

            if (!string.IsNullOrEmpty(row[2].ToString()))
                returnValue.StartDate = Convert.ToDateTime(row[2]);

            if (!string.IsNullOrEmpty(row[3].ToString()))
                returnValue.EndDate = Convert.ToDateTime(row[3]);

            if (!string.IsNullOrEmpty(row[4].ToString()))
                returnValue.MinEndDays = Convert.ToInt32(row[4]);

            returnValue.RangePlanID = rangeDAO.GetRangePlanID(returnValue.SKU);
            returnValue.DeliveryGroup = config.db.DeliveryGroups.Where(dg => dg.PlanID == returnValue.RangePlanID && 
                                                                             dg.Name == returnValue.DeliveryGroupName).FirstOrDefault();

            if (string.IsNullOrEmpty(row[2].ToString()) &&
                string.IsNullOrEmpty(row[3].ToString()) &&
                string.IsNullOrEmpty(row[4].ToString()))
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
                        DeliveryGroupUploadModel newRow = ParseUploadRow(dataRow);
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