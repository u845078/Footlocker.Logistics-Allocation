using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using Footlocker.Common.Services;
using System.Text.RegularExpressions;
using System.Web;
using Footlocker.Logistics.Allocation.Common;
using System.IO;
using System.Linq;
using Aspose.Excel;
using System.Drawing;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class ProductTypeSpreadsheet : UploadExcelSpreadsheet
    {
        readonly ProductTypeDAO productTypeDAO;
        public List<ProductType> validRows = new List<ProductType>();
        public List<ProductType> errorRows = new List<ProductType>();
        List<ProductType> validProductTypes;

        private ProductType ParseRow(int row)
        {
            ProductType returnValue = new ProductType()
            {
                Division = Convert.ToString(worksheet.Cells[row, 0].Value).PadLeft(2, '0'),
                Dept = Convert.ToString(worksheet.Cells[row, 1].Value).PadLeft(2, '0'),
                StockNumber = Convert.ToString(worksheet.Cells[row, 2].Value).PadLeft(5, '0'),
                ProductTypeCode = Convert.ToString(worksheet.Cells[row, 3].Value)
            };

            return returnValue;
        }

        private string ValidateUploadValues(ProductType parsedRec)
        {
            string errorMessage = string.Empty;
            int count;

            if (!config.currentUser.HasDivision(config.AppName, parsedRec.Division))            
                errorMessage = string.Format("You are not authorized to update division {0}", parsedRec.Division);
            else
            {
                count = validProductTypes.Where(vpt => vpt.Division == parsedRec.Division &&
                                                       (vpt.Dept == parsedRec.Dept || vpt.Dept == "00") &&
                                                       vpt.ProductTypeCode == parsedRec.ProductTypeCode).Count();
                if (count == 0)
                    errorMessage = string.Format("Product Type {0} for Div/Dept {1}/{2} doesn't exist", parsedRec.ProductTypeCode, parsedRec.Division, parsedRec.Dept);
            }

            return errorMessage;
        }

        public void Save(HttpPostedFileBase attachment)
        {
            ProductType uploadRec;
            ProductType lookupRec;

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

                        if (row == 1)                        
                            validProductTypes = productTypeDAO.GetProductTypeList(uploadRec.Division);

                        errorMessage = ValidateUploadValues(uploadRec);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            uploadRec.ErrorMessage = errorMessage;
                            errorRows.Add(uploadRec);
                        }                            
                        else
                            validRows.Add(uploadRec);

                        row++;
                    }

                    if (validRows.Count > 0)
                    {
                        foreach (ProductType rec in validRows)
                        {
                            lookupRec = validProductTypes.Where(vpt => vpt.Division == rec.Division &&
                                                                (vpt.Dept == rec.Dept || vpt.Dept == "00") &&
                                                                vpt.ProductTypeCode == rec.ProductTypeCode).First();
                            
                            rec.ProductTypeID = lookupRec.ProductTypeID;
                            rec.ProductTypeName = lookupRec.ProductTypeName;
                        }

                        productTypeDAO.UpdateList(validRows);
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

        public Excel GetErrors(List<ProductType> errorList)
        {
            if (errorList != null)
            {
                Excel excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (ProductType p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Division);
                    mySheet.Cells[row, 1].PutValue(p.Dept);
                    mySheet.Cells[row, 2].PutValue(p.StockNumber);
                    mySheet.Cells[row, 3].PutValue(p.ProductTypeCode);
                    mySheet.Cells[row, 4].PutValue(p.ErrorMessage);
                    mySheet.Cells[row, 4].Style.Font.Color = Color.Red;
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

        public ProductTypeSpreadsheet(AppConfig config, ConfigService configService, ProductTypeDAO productTypeDAO) : base(config, configService)
        {
            maxColumns = 4;
            headerRowNumber = 0;

            columns.Add(0, "Div");
            columns.Add(1, "Dept");
            columns.Add(2, "Stock");
            columns.Add(3, "Product Type");

            templateFilename = config.ProductTypeTemplate;
            this.productTypeDAO = productTypeDAO;
        }
    }
}