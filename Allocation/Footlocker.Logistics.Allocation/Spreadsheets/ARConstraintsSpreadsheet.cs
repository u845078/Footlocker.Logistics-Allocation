using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Web;
using System.Data;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class ARConstraintsSpreadsheet : UploadSpreadsheet
    {
        public List<DirectToStoreConstraint> parsedARConstraints = new List<DirectToStoreConstraint>();

        private DirectToStoreConstraint ParseUploadRow(DataRow row)
        {
            string startDate = row[2].ToString();
            string endDate = row[3].ToString();

            DirectToStoreConstraint returnValue = new DirectToStoreConstraint
            {
                Sku = Convert.ToString(row[0]).Trim(),
                Size = Convert.ToString(row[1]).Trim(),
                MaxQty = Convert.ToInt32(row[4]),
                CreateDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                CreatedBy = config.currentUser.NetworkID
            };

            if (!string.IsNullOrEmpty(startDate))
                returnValue.StartDate = Convert.ToDateTime(startDate);

            if (!string.IsNullOrEmpty(endDate))
                returnValue.EndDate = Convert.ToDateTime(endDate);

            return returnValue;
        }

        public void Save(HttpPostedFileBase attachment)
        {
            var dao = new DirectToStoreDAO(config.EuropeDivisions);

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
                        parsedARConstraints.Add(ParseUploadRow(dataRow));
                        row++;
                    }

                    if (parsedARConstraints.Count > 0)
                    {
                        foreach (DirectToStoreConstraint rec in parsedARConstraints)
                        {
                            if (!config.currentUser.GetUserDivList(config.AppName).Contains(rec.Division))
                            {
                                message = "Upload failed: One ore more Skus are in a division you do not have permissions for.";
                            }
                        }

                        if (string.IsNullOrEmpty(message))
                            dao.SaveARConstraintsUpload(parsedARConstraints);
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

        public ARConstraintsSpreadsheet(AppConfig config, ConfigService configService) : base(config, configService)
        {
            maxColumns = 5;

            columns.Add(0, "SKU");
            columns.Add(1, "Size");
            columns.Add(2, "Start Date");
            columns.Add(3, "End Date");
            columns.Add(4, "Max Qty");

            templateFilename = config.ARConstraintsUploadTemplate;
        }
    }
}