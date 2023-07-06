using Aspose.Excel;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Spreadsheet
{
    public class ARSKUSpreadsheet : UploadSpreadsheet
    {
        public List<DirectToStoreSku> parsedARSKUs = new List<DirectToStoreSku>();

        private DirectToStoreSku ParseUploadRow(int row)
        {
            DirectToStoreSku returnValue = new DirectToStoreSku
            {
                Sku = Convert.ToString(worksheet.Cells[row, 0].Value).Trim(),
                StartDate = Convert.ToDateTime(worksheet.Cells[row, 1].Value),
                EndDate = Convert.ToDateTime(worksheet.Cells[row, 2].Value),
                VendorPackQty = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                OrderSun = Convert.ToBoolean(worksheet.Cells[row, 4].Value),
                OrderMon = Convert.ToBoolean(worksheet.Cells[row, 5].Value),
                OrderTue = Convert.ToBoolean(worksheet.Cells[row, 6].Value),
                OrderWed = Convert.ToBoolean(worksheet.Cells[row, 7].Value),
                OrderThur = Convert.ToBoolean(worksheet.Cells[row, 8].Value),
                CreateDate = DateTime.Now,
                CreatedBy = config.currentUser.NetworkID
            };

            return returnValue;
        }

        public void Save(HttpPostedFileBase attachment)
        {
            var dao = new DirectToStoreDAO();

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
                        parsedARSKUs.Add(ParseUploadRow(row));
                        row++;
                    }

                    if (parsedARSKUs.Count > 0)
                    {
                        foreach (DirectToStoreSku rec in parsedARSKUs)
                        {
                            if (!config.currentUser.GetUserDivList(config.AppName).Contains(rec.Division))
                            {
                                message = "Upload failed: One ore more Skus are in a division you do not have permissions for.";
                            }
                        }

                        if (string.IsNullOrEmpty(message))                           
                            dao.SaveARSkusUpload(parsedARSKUs);                        
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

        public ARSKUSpreadsheet(AppConfig config, ConfigService configService) : base(config, configService)
        {
            maxColumns = 9;

            columns.Add(0, "SKU");
            columns.Add(1, "Start Date");
            columns.Add(2, "End Date");
            columns.Add(3, "Buying Multiple");
            columns.Add(4, "Order Sunday");
            columns.Add(5, "Order Monday");
            columns.Add(6, "Order Tuesday");
            columns.Add(7, "Order Wednesday");
            columns.Add(8, "Order Thursday");

            templateFilename = config.ARSKUsUploadTemplate;
        }
    }
}