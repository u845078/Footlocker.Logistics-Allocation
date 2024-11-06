using Aspose.Cells;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Services;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class POOverrideSpreadsheet : UploadSpreadsheet
    {
        readonly ExistingPODAO existingPODAO;
        public List<ExpeditePO> validRecs = new List<ExpeditePO>();
        public List<ExpeditePO> errorList = new List<ExpeditePO>();

        private ExpeditePO ParseRow(DataRow row)
        {
            ExpeditePO newExpeditePO = new ExpeditePO()
            {
                Division = Convert.ToString(row[0]).Trim(),
                PO = Convert.ToString(row[1]).Trim(),
                OverrideDate = Convert.ToDateTime(row[2]), 
                Departments = "",
                Sku = "",
                CreateDate = DateTime.Now, 
                CreatedBy = config.currentUser.NetworkID
            };

            return newExpeditePO; 
        }

        private bool ValidateRow(ExpeditePO item)
        {            
            List<ExistingPO> existingPOList;
            int skucount = 0;

            if (string.IsNullOrEmpty(item.Division))
                item.ErrorMessage = "Division is missing and is required";
            else
            {
                if (string.IsNullOrEmpty(item.PO))
                    item.ErrorMessage = "PO is missing and is required";
                else
                {
                    if (item.OverrideDate == DateTime.MinValue)
                        item.ErrorMessage = "Override Date is missing and is required";
                    else
                    {
                        existingPOList = existingPODAO.GetExistingPO(item.Division, item.PO);
                        if (existingPOList.Count == 0)
                            item.ErrorMessage = "PO not found";
                        else
                        {
                            foreach (ExistingPO po in existingPOList)
                            {
                                if (!config.currentUser.HasDivDept(config.AppName, po.Division, po.Sku.Substring(3, 2)))
                                    item.ErrorMessage = "Permission denied.";

                                if (!item.Departments.Contains(po.Sku.Substring(3, 2)))
                                {
                                    if (item.Departments.Length > 0)
                                        item.Departments += ",";

                                    item.Departments += po.Sku.Substring(3, 2);
                                }

                                item.Sku = po.Sku;
                                skucount++;

                                if (skucount > 1)
                                    item.Sku = "multi-sku-" + item.Sku;
                            }

                            if (existingPOList.Count > 0)
                                item.ExpectedDeliveryDate = existingPOList[0].ExpectedDeliveryDate;
                        }
                    }
                }
            }

            return string.IsNullOrEmpty(item.ErrorMessage);
        }

        public void Save(HttpPostedFileBase attachment)
        {
            ExpeditePO item;

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

                    foreach (ExpeditePO rec in validRecs)
                    {
                        if (rec.Sku.Length > 50)
                            rec.Sku = rec.Sku.Substring(0, 50);

                        bool alreadyExists = config.db.ExpeditePOs.Where(ep => ep.PO == rec.PO && ep.Division == rec.Division).Count() > 0;
                        
                        if (alreadyExists)                        
                            config.db.Entry(rec).State = System.Data.EntityState.Modified;                        
                        else                        
                            config.db.ExpeditePOs.Add(rec);
                        
                        if (string.IsNullOrEmpty(rec.Sku))                        
                            rec.Sku = "unknown";                        
                    }

                    config.db.SaveChanges();
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public Workbook GetErrors(List<ExpeditePO> errorList)
        {
            if (errorList != null)
            {
                excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (ExpeditePO p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Division);
                    mySheet.Cells[row, 1].PutValue(p.PO);
                    mySheet.Cells[row, 2].PutValue(p.OverrideDate);
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

        public POOverrideSpreadsheet(AppConfig config, ConfigService configService, ExistingPODAO poDAO) : base(config, configService)
        {
            maxColumns = 3;
            headerRowNumber = 0;

            columns.Add(0, "Division");
            columns.Add(1, "PO");
            columns.Add(2, "Override Date");

            templateFilename = config.POOverrideTemplate;
            existingPODAO = poDAO;
        }
    }
}