using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Web;
using System.Data;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class ARSKUSpreadsheet : UploadSpreadsheet
    {
        public List<DirectToStoreSku> parsedARSKUs = new List<DirectToStoreSku>();

        private DirectToStoreSku ParseUploadRow(DataRow row)
        {
            DirectToStoreSku returnValue = new DirectToStoreSku
            {
                Sku = Convert.ToString(row[0]).Trim(),
                StartDate = Convert.ToDateTime(row[1]),                
                VendorPackQty = Convert.ToInt32(row[3]),
                OrderSun = Convert.ToString(row[4]) == "1",
                OrderMon = Convert.ToString(row[5]) == "1",
                OrderTue = Convert.ToString(row[6]) == "1",
                OrderWed = Convert.ToString(row[7]) == "1",
                OrderThur = Convert.ToString(row[8]) == "1",
                CreateDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                CreatedBy = config.currentUser.NetworkID
            };

            if (!string.IsNullOrEmpty(row[2].ToString().Trim()))
                returnValue.EndDate = DateTime.Parse(row[2].ToString().Trim());

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
                        parsedARSKUs.Add(ParseUploadRow(dataRow));
                        row++;
                    }

                    if (parsedARSKUs.Count > 0)
                    {
                        foreach (DirectToStoreSku rec in parsedARSKUs)
                        {
                            if (!config.currentUser.GetUserDivList(config.AppName).Contains(rec.Division))                            
                                message = "Upload failed: One ore more Skus are in a division you do not have permissions for.";                            
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