using Aspose.Excel;
using Footlocker.Common.Entities;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Footlocker.Logistics.Allocation.Common
{
    public class HoldsUploadSpreadsheet : UploadSpreadsheet
    {
        public List<Hold> validHolds = new List<Hold>();
        public List<Hold> errorList = new List<Hold>();
        readonly HoldService holdService;

        private Hold ParseRow(int row)
        {
            bool deptExists;
            bool brandExists;
            bool teamExists;
            bool categoryExists;
            bool vendorExists;
            bool skuExists;

            Hold returnValue = new Hold()
            {
                Division = Convert.ToString(worksheet.Cells[row, 0].Value).Trim(),
                Store = Convert.ToString(worksheet.Cells[row, 1].Value).Trim(),
                Duration = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Convert.ToString(worksheet.Cells[row, 2].Value).Trim().ToLower()),
                Department = Convert.ToString(worksheet.Cells[row, 3].Value).Trim(),
                Brand = Convert.ToString(worksheet.Cells[row, 4].Value).Trim(),
                Team = Convert.ToString(worksheet.Cells[row, 5].Value).Trim(),
                Category = Convert.ToString(worksheet.Cells[row, 6].Value).Trim(),
                Vendor = Convert.ToString(worksheet.Cells[row, 7].Value).Trim(),
                SKU = Convert.ToString(worksheet.Cells[row, 8].Value).Trim(),
                StartDate = Convert.ToDateTime(worksheet.Cells[row, 9].Value),
                EndDate = Convert.ToDateTime(worksheet.Cells[row, 10].Value),
                HoldType = Convert.ToString(worksheet.Cells[row, 11].Value).Trim().ToLower(),
                Comments = string.Format("(Upload) - {0}", Convert.ToString(worksheet.Cells[row, 12].Value).Trim()),
                CreateDate = DateTime.Now,
                CreatedBy = config.currentUser.NetworkID                
            };

            deptExists = !string.IsNullOrEmpty(returnValue.Department);
            brandExists = !string.IsNullOrEmpty(returnValue.Brand);
            teamExists = !string.IsNullOrEmpty(returnValue.Team);
            categoryExists = !string.IsNullOrEmpty(returnValue.Category);
            vendorExists = !string.IsNullOrEmpty(returnValue.Vendor);
            skuExists = !string.IsNullOrEmpty(returnValue.SKU);

            // sku
            if (skuExists && !deptExists && !brandExists && !teamExists && !categoryExists && !vendorExists)
            {
                returnValue.Level = "Sku";
                returnValue.Value = returnValue.SKU;
            }
            // VendorDeptCategory
            else if (deptExists && categoryExists && vendorExists && !brandExists && !teamExists && !skuExists)
            {
                returnValue.Level = "VendorDeptCategory";
                returnValue.Value = string.Format("{0}-{1}-{2}", returnValue.Vendor, returnValue.Department, returnValue.Category);
            }
            // VendorDept
            else if (deptExists && vendorExists && !brandExists && !teamExists && !categoryExists && !skuExists)
            {
                returnValue.Level = "VendorDept";
                returnValue.Value = string.Format("{0}-{1}", returnValue.Vendor, returnValue.Department);
            }
            // DeptCatTeam
            else if (deptExists && categoryExists && teamExists && !brandExists && !vendorExists && !skuExists)
            {
                returnValue.Level = "DeptCatTeam";
                returnValue.Value = string.Format("{0}-{1}-{2}", returnValue.Department, returnValue.Category, returnValue.Team);
            }
            // DeptCatBrand
            else if (deptExists && categoryExists && brandExists && !teamExists && !vendorExists && !skuExists)
            {
                returnValue.Level = "DeptCatBrand";
                returnValue.Value = string.Format("{0}-{1}-{2}", returnValue.Department, returnValue.Category, returnValue.Brand);
            }
            // DeptCat
            else if (deptExists && categoryExists && !brandExists && !teamExists && !vendorExists && !skuExists)
            {
                returnValue.Level = "Category";
                returnValue.Value = string.Format("{0}-{1}", returnValue.Department, returnValue.Category);
            }
            // DeptTeam
            else if (deptExists && teamExists && !brandExists && !categoryExists && !vendorExists && !skuExists)
            {
                returnValue.Level = "DeptTeam";
                returnValue.Value = string.Format("{0}-{1}", returnValue.Department, returnValue.Team);
            }
            // DeptBrand
            else if (deptExists && brandExists && !teamExists && !categoryExists && !vendorExists && !skuExists)
            {
                returnValue.Level = "DeptBrand";
                returnValue.Value = string.Format("{0}-{1}", returnValue.Department, returnValue.Brand);
            }
            // Dept
            else if (deptExists && !brandExists && !teamExists && !categoryExists && !vendorExists && !skuExists)
            {
                returnValue.Level = "Dept";
                returnValue.Value = returnValue.Department;
            }
            // Store
            else if (!deptExists && !brandExists && !teamExists && !categoryExists && !vendorExists && !skuExists)            
                returnValue.Level = "All";            
            else            
                returnValue.Level = "";

            returnValue.ReserveInventoryBool = returnValue.HoldType == "reserve inventory";

            // check end date - original parsing will set the EndDate to the MinValue if not present, which will conflict with sql server
            if (returnValue.EndDate.Equals(DateTime.MinValue))
                returnValue.EndDate = null;

            return returnValue;
        }

        private bool ValidateRow(Hold hold, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (hold.Level == "")
                errorMessage += "Invalid combination for hold. ";

            // check duration
            if (!(hold.Duration == "Temporary" || hold.Duration == "Permanent"))           
                errorMessage += "Invalid input for 'Duration'. ";            

            // check hold type
            if (hold.HoldType == "reserve inventory" || hold.HoldType == "cancel inventory")
            {
                // if duration is permanent, the user should not be able to reserve inventory
                if (hold.ReserveInventoryBool && hold.Duration == "Permanent")                
                    errorMessage += "You cannot reserve inventory if you have a permanent duration. ";                
            }
            else            
                errorMessage += "Invalid input for 'Hold Type'. ";            

            // check start date
            if (hold.StartDate.Equals(DateTime.MinValue))            
                errorMessage += "Invalid input for 'Start Date'. This value cannot be empty. ";

            return string.IsNullOrEmpty(errorMessage);
        }

        public void Save(HttpPostedFileBase attachment)
        {
            Hold item;
            string rowErrorMessage;

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
                        item = ParseRow(row);

                        if (!ValidateRow(item, out rowErrorMessage))
                        {
                            item.ErrorMessage = rowErrorMessage;
                            errorList.Add(item);
                        }
                        else
                            validHolds.Add(item);

                        row++;
                    }    

                    foreach (Hold hold in validHolds)
                    {
                        int count;

                        count = validHolds.Where(h => h.Division == hold.Division &&
                                                      h.Store == hold.Store &&
                                                      h.Duration == hold.Duration &&
                                                      h.Level == hold.Level &&
                                                      h.Value == (hold.Value == "" ? "N/A" : hold.Value) &&
                                                      h.StartDate == hold.StartDate &&
                                                      h.EndDate == hold.EndDate &&
                                                      h.ReserveInventoryBool == hold.ReserveInventoryBool).Count();
                        if (count > 1)
                            hold.ErrorMessage += "Identical hold already found within spreadsheet.";
                    }

                    List<Hold> dupHolds = validHolds.Where(vh => !string.IsNullOrEmpty(vh.ErrorMessage)).ToList();
                    errorList.AddRange(dupHolds);

                    foreach (Hold dup in dupHolds)
                        validHolds.Remove(dup);
                    
                    foreach (Hold hold in validHolds)
                    {
                        //validate values dependent on business logic and sql server data type restrictions
                        holdService.Hold = hold;
                        hold.ErrorMessage = holdService.ValidateHold(false, false, true);
                    }

                    List<Hold> erroredHolds = validHolds.Where(vh => !string.IsNullOrEmpty(vh.ErrorMessage)).ToList();
                    errorList.AddRange(erroredHolds);

                    foreach (Hold errorHold in erroredHolds)
                        validHolds.Remove(errorHold);

                    foreach (Hold hold in validHolds)
                    {
                        config.db.Holds.Add(hold);
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

        public Excel GetErrors(List<Hold> errorList)
        {
            if (errorList != null)
            {
                Excel excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (Hold p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Division);
                    mySheet.Cells[row, 1].PutValue(p.Store);
                    mySheet.Cells[row, 2].PutValue(p.Duration);
                    mySheet.Cells[row, 3].PutValue(p.Department);
                    mySheet.Cells[row, 4].PutValue(p.Brand);
                    mySheet.Cells[row, 5].PutValue(p.Team);
                    mySheet.Cells[row, 6].PutValue(p.Category);
                    mySheet.Cells[row, 7].PutValue(p.Vendor);
                    mySheet.Cells[row, 8].PutValue(p.SKU);
                    mySheet.Cells[row, 9].PutValue(p.StartDate);
                    mySheet.Cells[row, 10].PutValue(p.EndDate);
                    mySheet.Cells[row, 11].PutValue(p.HoldType);
                    mySheet.Cells[row, 12].PutValue(p.Comments);

                    mySheet.Cells[row, 13].PutValue(p.ErrorMessage);
                    mySheet.Cells[row, 13].Style.Font.Color = Color.Red;
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

        public HoldsUploadSpreadsheet(AppConfig config, ConfigService configService, HoldService holdService) : base(config, configService)
        {
            maxColumns = 13;
            headerRowNumber = 1;

            columns.Add(0, "Division");
            columns.Add(1, "Store");
            columns.Add(2, "Duration");
            columns.Add(3, "Department");
            columns.Add(4, "Brand");
            columns.Add(5, "Team");
            columns.Add(6, "Category");
            columns.Add(7, "Vendor");
            columns.Add(8, "Sku");
            columns.Add(9, "Start Date");
            columns.Add(10, "End Date");
            columns.Add(11, "Hold Type");
            columns.Add(12, "Comment");

            //this.itemDAO = itemDAO;
            templateFilename = config.HoldsUploadTemplate;
            this.holdService = holdService;
        }
    }
}