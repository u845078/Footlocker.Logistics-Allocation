using System.Collections.Generic;
using System.Linq;
using System;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Common;
using System.Globalization;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class BTSUnassignedExport : ExportExcelSpreadsheet
    {
        readonly MainframeStoreDAO mainframeStoreDAO;

        public void WriteData(string div, int year)
        {
            excelDocument = GetTemplate();

            List<StoreLookup> stores = (from a in config.db.StoreLookups.Include("StoreExtension")
                                        join b in config.db.StoreBTSDetails.Where(sbd => sbd.Year == year)
                                        on new { a.Division, a.Store } equals new { b.Division, b.Store } into subset
                                        from sc in subset.DefaultIfEmpty()
                                        where sc == null && a.Division == div
                                        select a).ToList();
            
            List<StoreExtension> excludedstores = config.db.StoreExtensions.Where(se => se.ExcludeStore == true).ToList();
            List<MainframeStore> ClosingDates = new List<MainframeStore>();
            List<string> ClosedStores = new List<string>();

            foreach (StoreLookup s in stores)
            {
                if (s.status == "C")
                {
                    ClosedStores.Add(s.Store);
                    if (ClosedStores.Count == 15)
                    {
                        ClosingDates.AddRange(mainframeStoreDAO.GetClosingDates(ClosedStores, div));
                        ClosedStores.Clear();
                    }
                }
            }
            if (ClosedStores.Count > 0)
            {
                ClosingDates.AddRange(mainframeStoreDAO.GetClosingDates(ClosedStores, div));
                ClosedStores.Clear();
            }
            
            currentRow = 1;
            currentSheet = excelDocument.Worksheets[worksheetNum];

            foreach (StoreLookup s in stores)
            {
                if (excludedstores.Where(es => es.Store == s.Store && es.Division == s.Division).Count() == 0)
                {                    
                    currentSheet.Cells[currentRow, 0].PutValue(s.Division);
                    currentSheet.Cells[currentRow, 1].PutValue(s.Region);
                    currentSheet.Cells[currentRow, 2].PutValue(s.League);
                    currentSheet.Cells[currentRow, 3].PutValue(s.Store);
                    currentSheet.Cells[currentRow, 4].PutValue(s.Mall);
                    currentSheet.Cells[currentRow, 5].PutValue(s.State);
                    currentSheet.Cells[currentRow, 6].PutValue(s.City);
                    currentSheet.Cells[currentRow, 7].PutValue(s.DBA);
                    currentSheet.Cells[currentRow, 8].PutValue(s.status);

                    if (s.status == "C")
                    {
                        var query = from a in ClosingDates 
                                    where a.Store == s.Store 
                                    select a.ClosedDate;

                        if (query.Count() > 0)
                        {
                            currentSheet.Cells[currentRow, 9].PutValue(DateTime.ParseExact(query.First(), "yymmdd", CultureInfo.InvariantCulture));
                        }
                    }

                    currentRow++;
                }
            }

            AutofitColumns();
        }

        public BTSUnassignedExport(AppConfig config, MainframeStoreDAO mfDAO) : base(config)
        {
            maxColumns = 10;

            templateFilename = config.BTSUnassignedTemplate;
            mainframeStoreDAO = mfDAO;
        }
    }
}