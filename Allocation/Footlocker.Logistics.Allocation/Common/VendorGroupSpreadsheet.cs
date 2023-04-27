using Aspose.Excel;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Common
{
    public class VendorGroupSpreadsheet : UploadSpreadsheet
    {
        readonly VendorGroupDetailDAO vendorGroupDAO; 
        public List<VendorGroupDetail> errorList = new List<VendorGroupDetail>();
        public List<VendorGroupDetail> validList = new List<VendorGroupDetail>();

        private VendorGroupDetail ParseRow(int rowNum, int groupID)
        {
            VendorGroupDetail returnValue = new VendorGroupDetail()
            {
                GroupID = groupID,
                VendorNumber = Convert.ToString(worksheet.Cells[rowNum, 0].Value).PadLeft(5, '0'), 
                CreateDate = DateTime.Now,
                CreatedBy = config.currentUser.NetworkID
            };

            return returnValue;
        }

        private string ValidateUploadValues(VendorGroupDetail rec)
        {
            string errorMessage = string.Empty;

            VendorGroupDetail existing = config.db.VendorGroupDetails.Where(vgd => vgd.VendorNumber == rec.VendorNumber).FirstOrDefault();
            if (existing != null)                            
                errorMessage = string.Format("Already in group {0}", existing.GroupID);
            else            
                if (!vendorGroupDAO.IsVendorSetupForEDI(rec.VendorNumber))
                    errorMessage = "Vendor must be setup for EDI before it can be added to a group.  Please email EDI.Support.";            

            return errorMessage;
        }

        public void Save(HttpPostedFileBase attachment, int ID)
        {
            VendorGroupDetail uploadRec;

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
                        uploadRec = ParseRow(row, ID);

                        errorMessage = ValidateUploadValues(uploadRec);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            uploadRec.ErrorMessage = errorMessage;
                            errorList.Add(uploadRec);
                        }
                        else
                            validList.Add(uploadRec);

                        row++;
                    }

                    if (validList.Count > 0)
                    {
                        foreach (VendorGroupDetail rec in validList)
                        {
                            config.db.VendorGroupDetails.Add(rec);
                        }

                        VendorGroup vendorGroup = config.db.VendorGroups.Where(vg => vg.ID == ID).FirstOrDefault();

                        if (vendorGroup != null)                        
                            vendorGroup.Count += validList.Count();                        

                        config.db.SaveChanges();
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

        public Excel GetErrors(List<VendorGroupDetail> errorList)
        {
            if (errorList != null)
            {
                Excel excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (VendorGroupDetail p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.VendorNumber);
                    mySheet.Cells[row, 1].PutValue(p.ErrorMessage);
                    mySheet.Cells[row, 1].Style.Font.Color = Color.Red;
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

        public VendorGroupSpreadsheet(AppConfig config, ConfigService configService, VendorGroupDetailDAO vendorGroupDetailDAO) : base(config, configService)
        {
            maxColumns = 1;
            headerRowNumber = 0;

            columns.Add(0, "Vendor");

            templateFilename = config.VendorGroupTemplate;

            vendorGroupDAO = vendorGroupDetailDAO;
        }
    }
}