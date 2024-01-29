using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class SkuAttributeSpreadsheet : UploadSpreadsheet
    {
        public List<SkuAttributeHeader> parsedSKUAttributes = new List<SkuAttributeHeader>();
        public List<SkuAttributeHeader> validSKUAttributes = new List<SkuAttributeHeader>();
        readonly ItemDAO itemDAO;

        private string CreateDetailRecord(SkuAttributeHeader header, string attributeType, string value)
        {            
            SkuAttributeDetail detail = new SkuAttributeDetail()
            {
                AttributeType = attributeType
            };
            
            if (value.ToLower().Equals("m"))
                detail.Mandatory = true;            
            else if (string.IsNullOrEmpty(value))            
                detail.WeightInt = 0;            
            else
            {
                try
                {
                    detail.WeightInt = Convert.ToInt32(value);
                }
                catch (FormatException)
                {
                    return string.Format("Attribute type {0} has an invalid supplied value.\n", attributeType);                    
                }
            }

            header.SkuAttributeDetails.Add(detail);

            return "";
        }

        private SkuAttributeHeader ParseRow(DataRow row)
        {
            message = string.Empty;

            SkuAttributeHeader returnValue = new SkuAttributeHeader()
            {
                Division = Convert.ToString(row[0]).Trim(),
                Dept = Convert.ToString(row[1]).Trim(),
                Category = Convert.ToString(row[2]).Trim(),
                Brand = Convert.ToString(row[3]).Trim(),
                SKU = Convert.ToString(row[4]).Trim(),
                CreateDate = Convert.ToDateTime(row[5]),
                WeightActiveInt = Convert.ToInt32(row[6])
            };

            message += CreateDetailRecord(returnValue, "Department", Convert.ToString(row[7]).Trim());
            message += CreateDetailRecord(returnValue, "Category", Convert.ToString(row[8]).Trim());
            message += CreateDetailRecord(returnValue, "VendorNumber", Convert.ToString(row[9]).Trim());
            message += CreateDetailRecord(returnValue, "BrandID", Convert.ToString(row[10]).Trim());
            message += CreateDetailRecord(returnValue, "Size", Convert.ToString(row[11]).Trim());
            message += CreateDetailRecord(returnValue, "SizeRange", Convert.ToString(row[12]).Trim());
            message += CreateDetailRecord(returnValue, "Color1", Convert.ToString(row[13]).Trim());
            message += CreateDetailRecord(returnValue, "Color2", Convert.ToString(row[14]).Trim());
            message += CreateDetailRecord(returnValue, "Color3", Convert.ToString(row[15]).Trim());
            message += CreateDetailRecord(returnValue, "Gender", Convert.ToString(row[16]).Trim());
            message += CreateDetailRecord(returnValue, "LifeOfSku", Convert.ToString(row[17]).Trim());
            message += CreateDetailRecord(returnValue, "Material", Convert.ToString(row[18]).Trim());
            message += CreateDetailRecord(returnValue, "PlayerID", Convert.ToString(row[19]).Trim());
            message += CreateDetailRecord(returnValue, "SkuID1", Convert.ToString(row[20]).Trim());
            message += CreateDetailRecord(returnValue, "SkuID2", Convert.ToString(row[21]).Trim());
            message += CreateDetailRecord(returnValue, "SkuID3", Convert.ToString(row[22]).Trim());
            message += CreateDetailRecord(returnValue, "SkuID4", Convert.ToString(row[23]).Trim());
            message += CreateDetailRecord(returnValue, "SkuID5", Convert.ToString(row[24]).Trim());
            message += CreateDetailRecord(returnValue, "TeamCode", Convert.ToString(row[25]).Trim());

            if (!string.IsNullOrEmpty(message))
                message = string.Format("Row #{0}: {1}", row, message);

            return returnValue;
        }

        private string ValidateUploadValues(SkuAttributeHeader header)
        {
            string errorsFound = "";

            // take out display for category and brand
            if (header.Category.ToLower().Equals("default") || header.Category.Equals(""))            
                header.Category = null;
            
            if (header.Brand.ToLower().Equals("default") || header.Brand.Equals(""))            
                header.Brand = null;            

            bool divisionExists = !string.IsNullOrEmpty(header.Division);
            bool deptExists = !string.IsNullOrEmpty(header.Dept);
            bool categoryExists = !string.IsNullOrEmpty(header.Category);
            bool brandExists = !string.IsNullOrEmpty(header.Brand);
            bool skuExists = !string.IsNullOrEmpty(header.SKU);

            if (!divisionExists)            
                errorsFound += "Division is required. <br />";            

            if (!deptExists)            
                errorsFound += "Department is required. <br />";            

            if (!categoryExists && brandExists)            
                errorsFound += "Category is required if a Brand is supplied. <br />";            

            // division/department/category/brand combination must have skus.
            bool comboExists = true;
            if (!categoryExists && !brandExists)
            {
                comboExists = config.db.ItemMasters.Where(im => im.Div == header.Division && im.Dept == header.Dept).Any();

                if (!comboExists)
                    errorsFound += "The Division/Department combination is not associated with any Sku. <br />";
            }
            else if (categoryExists && !brandExists)
            {
                comboExists = config.db.ItemMasters.Where(im => im.Div == header.Division && 
                                                                im.Dept == header.Dept && 
                                                                im.Category == header.Category).Any();

                if (!comboExists)
                    errorsFound += "The Division/Department/Category combination is not associated with any Sku. <br />";
            }
            else if (brandExists)
            {
                comboExists = config.db.ItemMasters.Where(im => im.Div == header.Division &&
                                                                im.Dept == header.Dept &&
                                                                im.Category == header.Category &&
                                                                im.Brand == header.Brand).Any();

                if (!comboExists)                
                    errorsFound += "The Division/Department/Category/Brand combination is not associated with any Sku  <br />";                
            }

            if (skuExists)
            {
                string skuDivision = header.SKU.Substring(0, 2);
                string skuDepartment = header.SKU.Substring(3, 2);

                if (skuDivision != header.Division || skuDepartment != header.Dept)
                    errorsFound += "The Division and Department must match the SKU's division and department";

                long itemID = itemDAO.GetItemID(header.SKU);

                if (itemID == 0)
                    errorsFound += "This SKU is not found in the database";

                if (categoryExists || brandExists)                
                    errorsFound += "You can't provide a Category or Brand ID when providing a SKU.";                
            }        

            // department MUST be mandatory.
            var departmentAttributeDetail = header.SkuAttributeDetails.Where(sad => sad.AttributeType.ToLower().Equals("department")).SingleOrDefault();
            if (!departmentAttributeDetail.Mandatory)
                errorsFound += "The department attribute must be mandatory. <br />";

            // if category or brand is supplied then the attributes for category and brand MUST be mandatory.
            var categoryAttributeDetail = header.SkuAttributeDetails.Where(sad => sad.AttributeType.ToLower().Equals("category")).SingleOrDefault();
            if (categoryExists && !categoryAttributeDetail.Mandatory)
                errorsFound += "The category attribute must be mandatory. <br />";

            var brandAttributeDetail = header.SkuAttributeDetails.Where(sad => sad.AttributeType.ToLower().Equals("brandid")).SingleOrDefault();
            if (brandExists && !brandAttributeDetail.Mandatory)
                errorsFound += "The brand attribute must be mandatory. <br />";

            // all attributes must add up to 100
            if (header.SkuAttributeDetails.Sum(sad => sad.WeightInt) != 100)
                errorsFound += "The weight must add up to 100. <br />";

            return errorsFound;
        }

        public void Save(HttpPostedFileBase attachment)
        {
            SkuAttributeHeader uploadRec;

            LoadAttachment(attachment.InputStream);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 2;
                try
                {
                    foreach (DataRow dataRow in excelData.Rows)
                    {
                        uploadRec = ParseRow(dataRow);
                        
                        if (!string.IsNullOrEmpty(message))
                            return;

                        parsedSKUAttributes.Add(uploadRec);

                        errorMessage = ValidateUploadValues(uploadRec);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            message = string.Format("Row {0}: {1}", row, errorMessage);
                            return;
                        } 
                        else
                        {
                            validSKUAttributes.Add(uploadRec);

                            // determine if the header already exists
                            SkuAttributeHeader existentHeader = config.db.SkuAttributeHeaders.Where(sah => sah.Division == uploadRec.Division &&
                                                                                                           sah.Dept == uploadRec.Dept &&
                                                                                                           (sah.Category == null ? uploadRec.Category == null : sah.Category == uploadRec.Category) &&
                                                                                                           (sah.Brand == null ? uploadRec.Brand == null : sah.Brand == uploadRec.Brand) &&
                                                                                                           (sah.SKU == null ? uploadRec.SKU == null : sah.SKU == uploadRec.SKU))
                                                                                             .SingleOrDefault();

                            if (existentHeader != null)
                            {
                                existentHeader.WeightActiveInt = uploadRec.WeightActiveInt;
                                existentHeader.CreateDate = DateTime.Now;
                                existentHeader.CreatedBy = config.currentUser.NetworkID;
                                uploadRec.ID = existentHeader.ID;

                                // delete existing detail records.
                                List<SkuAttributeDetail> deleteDetailRecords = config.db.SkuAttributeDetails.Where(sad => sad.HeaderID == existentHeader.ID).ToList();

                                foreach (var detail in deleteDetailRecords)
                                {
                                    config.db.SkuAttributeDetails.Remove(detail);
                                }

                                config.db.Entry(existentHeader).State = System.Data.EntityState.Modified;

                                // populate detail records with header ID and add record
                                foreach (var detail in uploadRec.SkuAttributeDetails)
                                {
                                    detail.HeaderID = uploadRec.ID;
                                    config.db.SkuAttributeDetails.Add(detail);
                                }

                                config.db.SaveChanges();
                            }
                            else
                            {
                                // add header to get its ID
                                uploadRec.CreatedBy = config.currentUser.NetworkID;
                                uploadRec.CreateDate = DateTime.Now;
                                config.db.SkuAttributeHeaders.Add(uploadRec);
                                config.db.SaveChanges();
                            }
                        }

                        row++;
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

        public SkuAttributeSpreadsheet(AppConfig config, ConfigService configService, ItemDAO itemDAO) : base(config, configService)
        {
            maxColumns = 26;
            headerRowNumber = 1;

            columns.Add(0, "Division");
            columns.Add(1, "Department");
            columns.Add(2, "Category");
            columns.Add(3, "BrandID");
            columns.Add(4, "SKU");
            columns.Add(5, "Update Date");
            columns.Add(6, "Active");
            columns.Add(7, "Department");
            columns.Add(8, "Category");
            columns.Add(9, "VendorNumber");
            columns.Add(10, "BrandID");
            columns.Add(11, "Size");
            columns.Add(12, "SizeRange");
            columns.Add(13, "Color1");
            columns.Add(14, "Color2");
            columns.Add(15, "Color3");
            columns.Add(16, "Gender");
            columns.Add(17, "LifeOfSku");
            columns.Add(18, "Material");
            columns.Add(19, "PlayerID");
            columns.Add(20, "SkuID1");
            columns.Add(21, "SkuID2");
            columns.Add(22, "SkuID3");
            columns.Add(23, "SkuID4");
            columns.Add(24, "SkuID5");
            columns.Add(25, "Team Code");

            this.itemDAO = itemDAO;
            templateFilename = config.SKUAttributeTemplate;
        }
    }
}