using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Web;
using Footlocker.Logistics.Allocation.Common;
using System.Linq;
using System.Data;
using Aspose.Cells;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class ProductTypeSpreadsheet : UploadSpreadsheet
    {
        readonly ProductTypeDAO productTypeDAO;
        public List<ProductType> validRows = new List<ProductType>();
        public List<ProductType> errorRows = new List<ProductType>();
        List<ProductType> validProductTypes;
        string authDivs;

        private ProductType ParseRow(DataRow row)
        {
            ProductType returnValue = new ProductType()
            {
                Division = Convert.ToString(row[0]).PadLeft(2, '0'),
                Dept = Convert.ToString(row[1]).PadLeft(2, '0'),
                StockNumber = Convert.ToString(row[2]).PadLeft(5, '0'),
                ProductTypeCode = Convert.ToString(row[3])
            };

            return returnValue;
        }

        private string ValidateUploadValues(ProductType parsedRec)
        {
            string errorMessage = string.Empty;
            int count;

            if (!authDivs.Split(',').Contains(parsedRec.Division))
                errorMessage = string.Format("Unauthorized division specified in spreadsheet, Division {0}. Please read instructions above for the authorized divisions.", parsedRec.Division);
            else
            {
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
            }

            return errorMessage;
        }

        public void Save(HttpPostedFileBase attachment)
        {
            ProductType uploadRec;
            ProductType lookupRec;

            try
            {
                authDivs = configService.GetValue(1, "SKUID_UPLOAD_AUTHORIZED_DIVS");
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

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
                        uploadRec = ParseRow(dataRow);

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

                        if (config.UpdateMF)
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

        public Workbook GetErrors(List<ProductType> errorList)
        {
            if (errorList != null)
            {
                Workbook excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (ProductType p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Division);
                    mySheet.Cells[row, 1].PutValue(p.Dept);
                    mySheet.Cells[row, 2].PutValue(p.StockNumber);
                    mySheet.Cells[row, 3].PutValue(p.ProductTypeCode);
                    mySheet.Cells[row, 4].PutValue(p.ErrorMessage);
                    mySheet.Cells[row, 4].SetStyle(errorStyle);

                    row++;
                }

                mySheet.AutoFitColumn(maxColumns);

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