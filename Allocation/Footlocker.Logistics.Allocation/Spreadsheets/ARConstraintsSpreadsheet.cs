using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Web;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class ARConstraintsSpreadsheet : UploadSpreadsheet
    {
        public List<DirectToStoreConstraint> parsedARConstraints = new List<DirectToStoreConstraint>();

        private DirectToStoreConstraint ParseUploadRow(int row)
        {
            DirectToStoreConstraint returnValue = new DirectToStoreConstraint
            {
                Sku = Convert.ToString(worksheet.Cells[row, 0].Value).Trim(),
                Size = Convert.ToString(worksheet.Cells[row, 1].Value).Trim(),
                StartDate = Convert.ToDateTime(worksheet.Cells[row, 2].Value),
                EndDate = Convert.ToDateTime(worksheet.Cells[row, 3].Value),
                MaxQty = Convert.ToInt32(worksheet.Cells[row, 4].Value),
                CreateDate = DateTime.Now,
                CreatedBy = config.currentUser.NetworkID
            };

            return returnValue;
        }

        public void Save(HttpPostedFileBase attachment)
        {
            var dao = new DirectToStoreDAO(config.EuropeDivisions);

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
                        parsedARConstraints.Add(ParseUploadRow(row));
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