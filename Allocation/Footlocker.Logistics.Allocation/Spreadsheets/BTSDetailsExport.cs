using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using System.Collections.Generic;
using System.Linq;
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class BTSDetailsExport : ExportSpreadsheet
    {
        public void WriteData(GridCommand settings, int ID)
        {
            WriteHeaderRecord();
            List<StoreBTSGridExtract> storeBTS = (from a in config.db.StoreBTS
                                                  where a.ID == ID
                                                  select new StoreBTSGridExtract
                                                  {
                                                      Year = a.Year,
                                                      Name = a.Name,
                                                      ID = a.ID,
                                                      Division = a.Division,
                                                      CreateDate = a.CreateDate,
                                                      CreatedBy = a.CreatedBy
                                                  }).ToList();

            foreach (var storeBTSRec in storeBTS)
            {
                storeBTSRec.ClusterID = (from a in config.db.StoreClusters
                                         where a.Division == storeBTSRec.Division &&
                                               a.Name == storeBTSRec.Name
                                         select a.ID).FirstOrDefault();

                IQueryable<StoreLookup> listToFilter = (from a in config.db.StoreBTSDetails
                                                        join b in config.db.StoreLookups on new { a.Division, a.Store } equals new { b.Division, b.Store }
                                                        where a.GroupID == ID
                                                        select b).ToList().AsQueryable();

                if (settings.FilterDescriptors.Any())
                    listToFilter = listToFilter.ApplyFilters(settings.FilterDescriptors);

                storeBTSRec.StoreLookups = listToFilter.ToList();
            }

            foreach (StoreBTSGridExtract rec in storeBTS)
            {
                currentSheet = excelDocument.Worksheets[worksheetNum];

                if (rec.StoreLookups.Count() == 0)
                {
                    currentSheet.Cells[currentRow, 0].PutValue(rec.ClusterID);
                    currentSheet.Cells[currentRow, 1].PutValue(rec.Year);
                    currentSheet.Cells[currentRow, 2].PutValue(rec.Name);
                    currentSheet.Cells[currentRow, 3].PutValue(rec.Division);
                    currentSheet.Cells[currentRow, 10].PutValue(rec.CreatedBy);
                    currentSheet.Cells[currentRow, 11].PutValue(rec.CreateDate);
                    currentSheet.Cells[currentRow, 11].SetStyle(dateStyle);

                    currentRow++;
                    recordCount++;
                }
                else
                {
                    foreach (var detail in rec.StoreLookups)
                    {
                        currentSheet.Cells[currentRow, 0].PutValue(rec.ClusterID);
                        currentSheet.Cells[currentRow, 1].PutValue(rec.Year);
                        currentSheet.Cells[currentRow, 2].PutValue(rec.Name);
                        currentSheet.Cells[currentRow, 3].PutValue(rec.Division);
                        currentSheet.Cells[currentRow, 4].PutValue(detail.League);
                        currentSheet.Cells[currentRow, 5].PutValue(detail.Store);
                        currentSheet.Cells[currentRow, 6].PutValue(detail.DBA);
                        currentSheet.Cells[currentRow, 7].PutValue(detail.Mall);
                        currentSheet.Cells[currentRow, 8].PutValue(detail.City);
                        currentSheet.Cells[currentRow, 9].PutValue(detail.State);
                        currentSheet.Cells[currentRow, 10].PutValue(rec.CreatedBy);
                        currentSheet.Cells[currentRow, 11].PutValue(rec.CreateDate);
                        currentSheet.Cells[currentRow, 11].SetStyle(dateStyle);

                        currentRow++;
                        recordCount++;
                    }
                }

                if (currentRow >= maxSpreadsheetRows)
                {
                    AutofitColumns();

                    worksheetNum++;
                    WriteHeaderRecord();
                }
            }

            AutofitColumns();
        }

        public BTSDetailsExport(AppConfig config) : base(config)
        {
            maxColumns = 12;

            columns.Add(0, "Cluster ID");
            columns.Add(1, "Year");
            columns.Add(2, "Name");
            columns.Add(3, "Division");
            columns.Add(4, "League");
            columns.Add(5, "Store");
            columns.Add(6, "DBA");
            columns.Add(7, "Mall");
            columns.Add(8, "City");
            columns.Add(9, "State");
            columns.Add(10, "Created By");
            columns.Add(11, "Create Date");
        }
    }
}