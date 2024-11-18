using System.Collections.Generic;
using System.Linq;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class RDQRestrictionsExport : ExportSpreadsheet
    {
        public void WriteData(IList<IFilterDescriptor> filterDescriptors)
        {
            excelDocument = GetTemplate();

            IQueryable<RDQRestriction> rdqRestrictions = (from rr in config.db.RDQRestrictions.AsEnumerable()
                                                          join d in config.currentUser.GetUserDivisions()
                                                            on rr.Division equals d.DivCode
                                                          orderby rr.Division, rr.Department, rr.Category, rr.Brand
                                                          select rr).AsQueryable();

            if (filterDescriptors != null)
                rdqRestrictions = rdqRestrictions.ApplyFilters(filterDescriptors);

            currentRow = 1;
            currentSheet = excelDocument.Worksheets[worksheetNum];

            foreach (RDQRestriction r in rdqRestrictions)
            {
                currentSheet.Cells[currentRow, 0].PutValue(r.Division);
                currentSheet.Cells[currentRow, 1].PutValue(r.Department);
                currentSheet.Cells[currentRow, 2].PutValue(r.Category);
                currentSheet.Cells[currentRow, 3].PutValue(r.Brand);
                currentSheet.Cells[currentRow, 4].PutValue(r.Vendor);
                currentSheet.Cells[currentRow, 5].PutValue(r.SKU);
                currentSheet.Cells[currentRow, 6].PutValue(r.RDQType);
                currentSheet.Cells[currentRow, 7].PutValue(r.FromDate);
                currentSheet.Cells[currentRow, 8].PutValue(r.ToDate);
                currentSheet.Cells[currentRow, 9].PutValue(r.FromDCCode);
                currentSheet.Cells[currentRow, 10].PutValue(r.ToLeague);
                currentSheet.Cells[currentRow, 11].PutValue(r.ToRegion);
                currentSheet.Cells[currentRow, 12].PutValue(r.ToStore);
                currentSheet.Cells[currentRow, 13].PutValue(r.ToDCCode);

                currentRow++;

                if (currentRow >= maxSpreadsheetRows)
                {
                    AutofitColumns();

                    worksheetNum++;
                    WriteHeaderRecord();
                    currentRow = 1;
                    currentSheet = excelDocument.Worksheets[worksheetNum];
                }
            }

            AutofitColumns();
        }

        public RDQRestrictionsExport(AppConfig config) : base(config)
        {
            maxColumns = 14;

            columns.Add(0, "Division");
            columns.Add(1, "Department");
            columns.Add(2, "Category");
            columns.Add(3, "Brand");
            columns.Add(4, "Vendor");
            columns.Add(5, "SKU");
            columns.Add(6, "RDQ Type");
            columns.Add(7, "From Date");
            columns.Add(8, "To Date");
            columns.Add(9, "From DC Code");
            columns.Add(10, "To League");
            columns.Add(11, "To Region");
            columns.Add(12, "To Store");
            columns.Add(13, "To DC Code");

            templateFilename = config.RDQRestrictionsExportTemplate;
        }
    }
}