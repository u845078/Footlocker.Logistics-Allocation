using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;
using System.Collections.Generic;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class ZoneStoreExport : ExportSpreadsheet
    {
        public void WriteData(int instanceID)
        {
            WriteHeaderRecord();

            List<NetworkZone> list = (from a in config.db.NetworkZones
                                      join b in config.db.NetworkZoneStores
                                        on a.ID equals b.ZoneID
                                      join c in config.db.InstanceDivisions
                                        on b.Division equals c.Division
                                      where c.InstanceID == instanceID
                                      select a).Distinct().ToList();

            List<NetworkZoneStore> storeList;

            foreach (NetworkZone z in list)
            {
                storeList = (from a in config.db.NetworkZoneStores
                             join b in config.db.InstanceDivisions
                               on a.Division equals b.Division
                             where a.ZoneID == z.ID && 
                                   b.InstanceID == instanceID
                             select a).ToList();

                foreach (NetworkZoneStore s in storeList)
                {
                    currentSheet = excelDocument.Worksheets[worksheetNum];

                    currentSheet.Cells[currentRow, 0].PutValue(z.Name);
                    currentSheet.Cells[currentRow, 1].PutValue(s.Division);
                    currentSheet.Cells[currentRow, 2].PutValue(s.Store);

                    currentRow++;
                    recordCount++;
                }
            }

            AutofitColumns();
        }

        public ZoneStoreExport(AppConfig config) : base(config)
        {
            maxColumns = 3;

            columns.Add(0, "Zone");
            columns.Add(1, "Division");
            columns.Add(2, "Store");
        }
    }
}