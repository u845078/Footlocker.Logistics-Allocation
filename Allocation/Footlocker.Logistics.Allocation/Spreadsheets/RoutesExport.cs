using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class RoutesExport : ExportSpreadsheet
    {
        public void WriteData(int instanceID)
        {
            WriteHeaderRecord();

            List<Route> list = config.db.Routes.Where(r => r.InstanceID == instanceID).ToList();

            foreach (Route r in list)
            {
                var rdList = (from a in config.db.RouteDetails
                              join b in config.db.NetworkZones on a.ZoneID equals b.ID
                              join c in config.db.DistributionCenters on a.DCID equals c.ID
                              where a.RouteID == r.ID
                              select new { det = a, zone = b, dc = c }).ToList();

                foreach (var d in rdList)
                {
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 0].PutValue(r.Name);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 1].PutValue(r.Perspective);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 2].PutValue(r.Pass);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 3].PutValue(d.dc.Name);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 4].PutValue(d.zone.Name);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 5].PutValue(d.det.Days);
                    currentRow++;
                }                
            }
            
            AutofitColumns();
        }

        public RoutesExport(AppConfig config) : base(config)
        {
            maxColumns = 6;
            columns.Add(0, "Route");
            columns.Add(1, "Perspective");
            columns.Add(2, "Pass");
            columns.Add(3, "DC");
            columns.Add(4, "Zone");
            columns.Add(5, "Days");
        }
    }
}