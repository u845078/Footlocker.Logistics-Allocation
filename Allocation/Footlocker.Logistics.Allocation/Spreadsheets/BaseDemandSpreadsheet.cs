using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Aspose.Cells;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class BaseDemandSpreadsheet : UploadSpreadsheet
    {
        public List<ReinitializeBaseDemand> validBaseDemand = new List<ReinitializeBaseDemand>();
        public List<ReinitializeBaseDemand> errorList = new List<ReinitializeBaseDemand>();
        private ItemDAO itemDAO;
        private StoreDAO storeDAO;

        private ReinitializeBaseDemand ParseRow(DataRow row)
        {         
            ReinitializeBaseDemand returnValue = new ReinitializeBaseDemand()
            {
                Division = Convert.ToString(row[0]).Trim(),
                SKU = Convert.ToString(row[1]).Trim(),
                Size = Convert.ToString(row[2]).Trim(),
                Store = Convert.ToString(row[3]).Trim(),
                BaseDemandString = Convert.ToString(row[4]),
                CreateDateTime = DateTime.Now,
                CreateUser = config.currentUser.NetworkID,
                LastModifiedDate = DateTime.Now,
                LastModifiedUser = config.currentUser.NetworkID
            };

            return returnValue;
        }

        private bool ValidateRow(ReinitializeBaseDemand baseDemand)
        {
            if (string.IsNullOrEmpty(baseDemand.Division))
                baseDemand.ErrorMessage = "Division is required and is empty";
            else
            {
                if (string.IsNullOrEmpty(baseDemand.SKU))
                    baseDemand.ErrorMessage = "SKU is required and is empty";
                else
                {
                    if (string.IsNullOrEmpty(baseDemand.Size))                        
                        baseDemand.ErrorMessage = "Size is required and is empty";
                    else
                    {
                        if (string.IsNullOrEmpty(baseDemand.Store))
                            baseDemand.ErrorMessage = "Store is required and is empty";
                        else
                        {
                            if (string.IsNullOrEmpty(baseDemand.BaseDemandString))
                                baseDemand.ErrorMessage = "Base Demand is required and is empty";
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(baseDemand.ErrorMessage))
            {
                baseDemand.ItemID = itemDAO.GetItemID(baseDemand.SKU);
                ValidStoreLookup validStoreLookup;
                decimal convertDecimal;

                if (baseDemand.ItemID == 0)
                    baseDemand.ErrorMessage = "Invalid SKU: it was not found in the database";
                else
                {
                    if (!itemDAO.DoValidSizesExist(baseDemand.SKU, baseDemand.Size))
                        baseDemand.ErrorMessage = "Invalid Size: it was not found in the database for this SKU";
                }

                validStoreLookup = storeDAO.GetValidStore(baseDemand.Division, baseDemand.Store);
                if (validStoreLookup == null)
                    baseDemand.ErrorMessage = "Invalid Store: it was not found in the database";

                if (!decimal.TryParse(baseDemand.BaseDemandString, out convertDecimal))
                    baseDemand.ErrorMessage = "Invalid Base Demand: value does not look like a valid decimal number";
                else
                {
                    baseDemand.BaseDemand = convertDecimal;

                    if (baseDemand.BaseDemand <= 0)
                        baseDemand.ErrorMessage = "Invalid Base Demand: it must be greater than 0";
                }                    
            }

            return string.IsNullOrEmpty(baseDemand.ErrorMessage);
        }

        private void ValidateList()
        {
            var duplicates = (from id in validBaseDemand
                              group id by new { f1 = id.Division, f2 = id.SKU, f3 = id.Size, f4 = id.Store } into g
                              where g.Count() > 1
                              select g).ToList();

            foreach (var dup in duplicates)
            {
                foreach (var plDup in validBaseDemand.Where(x => x.Division == dup.Key.f1 && x.SKU == dup.Key.f2 && x.Size == dup.Key.f3 && x.Store == dup.Key.f4))
                {
                    plDup.ErrorMessage = "Duplicate Base Demand record";
                    errorList.Add(plDup);
                }
            }
        }

        public void Save(HttpPostedFileBase attachment)
        {
            ReinitializeBaseDemand item;            

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
                        {
                            if (!ValidateRow(item))                            
                                errorList.Add(item);                            
                            else
                                validBaseDemand.Add(item);
                        }

                        row++;
                    }

                    ValidateList();
                    validBaseDemand.RemoveAll(x => !string.IsNullOrEmpty(x.ErrorMessage));

                    ReinitializeBaseDemand tempBaseDemand;
                    foreach (ReinitializeBaseDemand bd in validBaseDemand)
                    {
                        tempBaseDemand = config.allocDB.ReinitializeBaseDemand.Where(rbd => rbd.ItemID == bd.ItemID &&
                                                                                            rbd.Size == bd.Size &&
                                                                                            rbd.Store == bd.Store).FirstOrDefault();
                        if (tempBaseDemand == null)                        
                            config.allocDB.ReinitializeBaseDemand.Add(bd);
                        else                        
                            tempBaseDemand.BaseDemand = bd.BaseDemand;                                                    
                    }

                    config.allocDB.SaveChanges();
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public Workbook GetErrors(List<ReinitializeBaseDemand> errorList)
        {
            if (errorList != null)
            {
                Workbook excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (ReinitializeBaseDemand p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Division);
                    mySheet.Cells[row, 1].PutValue(p.SKU);
                    mySheet.Cells[row, 2].PutValue(p.Size);
                    mySheet.Cells[row, 3].PutValue(p.Store);
                    mySheet.Cells[row, 4].PutValue(p.BaseDemandString);

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

        public BaseDemandSpreadsheet(AppConfig config, ConfigService configService, ItemDAO itemDAO, StoreDAO storeDAO) : base(config, configService)
        {
            maxColumns = 5;
            headerRowNumber = 0;

            columns.Add(0, "Division");
            columns.Add(1, "SKU");
            columns.Add(2, "Size");
            columns.Add(3, "Store");
            columns.Add(4, "Base Demand");

            templateFilename = config.BaseDemandTemplate;

            this.itemDAO = itemDAO;
            this.storeDAO = storeDAO;
        }
    }
}