using System;
using System.Linq;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using Aspose.Excel;
using System.Collections.Generic;
using Telerik.Web.Mvc;
using System.Runtime;
using System.Runtime.Remoting.Messaging;
//using Aspose.Cells;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class SkuAttributeExport : ExportSpreadsheet
    {
        public string headerFileName;

        public void ExtractGrid(IList<IFilterDescriptor> filters)
        {
            IQueryable<SkuAttributeHeader> headers = (from a in config.db.SkuAttributeHeaders.Include("SkuAttributeDetails").AsEnumerable()
                                                      join d in config.currentUser.GetUserDivisions(config.AppName)
                                                        on new { a.Division } equals new { Division = d.DivCode }
                                                      orderby a.Division, a.Dept, a.Category, a.SKU
                                                      select a).AsQueryable();

            if (filters != null)
                headers = headers.ApplyFilters(filters);

            if (headers.Count() == 0)
                errorMessage = string.Format("There was no data to export");
            else
                WriteData(headers);
        }

        public void ExtractHeader(int ID)
        {
            // retrieve data (return list even though only 1 should be returned in order to use general method)
            IQueryable<SkuAttributeHeader> headers = config.db.SkuAttributeHeaders.Include("SkuAttributeDetails")
                                                                                  .Where(sa => sa.ID == ID)
                                                                                  .AsQueryable();                                                                                  

            if (headers.Count() == 0)
                errorMessage = string.Format("There was no data to export");
            else
            {
                SkuAttributeHeader sah = headers.FirstOrDefault();

                headerFileName = string.Format("{0}-{1}", sah.Division, sah.Dept);
                if (sah.Category != null)
                    headerFileName += "-" + sah.Category;

                if (sah.Brand != null)
                    headerFileName += "-" + sah.Brand;

                if (!string.IsNullOrEmpty(sah.SKU))
                    headerFileName += "-" + sah.SKU;

                headerFileName += "-SkuAttributes.xls";

                WriteData(headers);
            }                
        }

        private void WriteData(IQueryable<SkuAttributeHeader> headers)
        {
            excelDocument = GetTemplate();

            currentRow = 2;
            currentSheet = excelDocument.Worksheets[worksheetNum];

            foreach (var header in headers)
            {
                // header values
                currentSheet.Cells[currentRow, 0].PutValue(header.Division);

                currentSheet.Cells[currentRow, 1].PutValue(header.Dept);
                currentSheet.Cells[currentRow, 2].PutValue(header.CategoryForDisplay);
                currentSheet.Cells[currentRow, 3].PutValue(header.BrandForDisplay);
                currentSheet.Cells[currentRow, 4].PutValue(header.SKU);
                currentSheet.Cells[currentRow, 5].PutValue(header.CreateDate);
                currentSheet.Cells[currentRow, 5].Style.Number = 14;
                currentSheet.Cells[currentRow, 6].PutValue(header.WeightActiveInt);
                AddBorder(currentRow, 6, currentSheet);

                // attribute weighting
                PopulateRowValue(currentRow, 7, header, currentSheet, "department");
                PopulateRowValue(currentRow, 8, header, currentSheet, "category");
                PopulateRowValue(currentRow, 9, header, currentSheet, "vendornumber");
                PopulateRowValue(currentRow, 10, header, currentSheet, "brandid");
                PopulateRowValue(currentRow, 11, header, currentSheet, "size");
                PopulateRowValue(currentRow, 12, header, currentSheet, "sizerange");
                PopulateRowValue(currentRow, 13, header, currentSheet, "color1");
                PopulateRowValue(currentRow, 14, header, currentSheet, "color2");
                PopulateRowValue(currentRow, 15, header, currentSheet, "color3");
                PopulateRowValue(currentRow, 16, header, currentSheet, "gender");
                PopulateRowValue(currentRow, 17, header, currentSheet, "lifeofsku");
                PopulateRowValue(currentRow, 18, header, currentSheet, "material");
                PopulateRowValue(currentRow, 19, header, currentSheet, "playerid");
                PopulateRowValue(currentRow, 20, header, currentSheet, "skuid1");
                PopulateRowValue(currentRow, 21, header, currentSheet, "skuid2");
                PopulateRowValue(currentRow, 22, header, currentSheet, "skuid3");
                PopulateRowValue(currentRow, 23, header, currentSheet, "skuid4");
                PopulateRowValue(currentRow, 24, header, currentSheet, "skuid5");
                PopulateRowValue(currentRow, 25, header, currentSheet, "teamcode");

                currentRow++;

                if (currentRow >= maxSpreadsheetRows)
                {
                    AutofitColumns();

                    worksheetNum++;
                    WriteHeaderRecord();
                    currentRow = 2;
                    currentSheet = excelDocument.Worksheets[worksheetNum];
                }
            }

            AutofitColumns();
        }

        private void AddBorder(int row, int col, Worksheet mySheet)
        {
            mySheet.Cells[row, col].Style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            mySheet.Cells[row, col].Style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            mySheet.Cells[row, col].Style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            mySheet.Cells[row, col].Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;

            mySheet.Cells[row, col].Style.Borders[BorderType.LeftBorder].Color = System.Drawing.Color.Black;
            mySheet.Cells[row, col].Style.Borders[BorderType.RightBorder].Color = System.Drawing.Color.Black;
            mySheet.Cells[row, col].Style.Borders[BorderType.BottomBorder].Color = System.Drawing.Color.Black;
            mySheet.Cells[row, col].Style.Borders[BorderType.TopBorder].Color = System.Drawing.Color.Black;
        }

        /// <summary>
        /// This was made so I didn't have to create an if/else for every attribute type since the PutValue method does
        /// not allow you to use a turing operator to determine what value to put (if they are of two different types i.e. int/string)
        /// </summary>
        private void PopulateRowValue(int row, int col, SkuAttributeHeader header, Worksheet mySheet, string attributeType)
        {
            SkuAttributeDetail attribute = header.SkuAttributeDetails.Where(sad => sad.AttributeType.ToLower().Equals(attributeType)).SingleOrDefault();
            if (attribute.Mandatory)            
                mySheet.Cells[row, col].PutValue("M");            
            // users don't want to have a 0 for the value. so if 0, return empty string.
            else if (attribute.WeightInt == 0)            
                mySheet.Cells[row, col].PutValue(string.Empty);            
            else            
                mySheet.Cells[row, col].PutValue(attribute.WeightInt);            

            AddBorder(row, col, mySheet);
        }

        public SkuAttributeExport(AppConfig config) : base(config)
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

            templateFilename = config.SKUAttributeTemplate;
        }
    }
}