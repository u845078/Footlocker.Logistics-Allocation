using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System.Collections.Generic;
using System.Linq;
using Telerik.Web.Mvc.Extensions;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class StoreNSSExport : ExportSpreadsheet
    {
        readonly private NetworkZoneStoreDAO networkZoneStoreDAO;
        private int instanceID;

        public void WriteData(string division)
        {
            List<NetworkZoneStore> storeList;
            int col;
            string dcName;

            instanceID = configService.GetInstance(division);
            List<NetworkZone> networkZoneList = networkZoneStoreDAO.GetStoreLeadTimes(instanceID);
            List<DistributionCenter> dcList = config.db.DistributionCenters.ToList();
            List<StoreLeadTime> storeLeadTimeList = config.db.StoreLeadTimes.Where(slt => slt.Division == division).ToList();
            List<StoreLookup> storeLookupList = config.db.StoreLookups.Where(sl => sl.Division == division).ToList();

            currentSheet = excelDocument.Worksheets[worksheetNum];

            WriteHeaderRecord();

            foreach (NetworkZone zone in networkZoneList)
            {
                storeList = config.db.NetworkZoneStores.Where(nzs => nzs.ZoneID == zone.ID).ToList();
                foreach (NetworkZoneStore s in storeList)
                {
                    if (s.Division == division)
                    {
                        StoreLookup store = storeLookupList.Where(sl => sl.Store == s.Store).FirstOrDefault();

                        currentSheet.Cells[currentRow, 0].PutValue(s.Division);
                        currentSheet.Cells[currentRow, 1].PutValue(s.Store);
                        currentSheet.Cells[currentRow, 2].PutValue(store.City);
                        currentSheet.Cells[currentRow, 3].PutValue(store.State);
                        currentSheet.Cells[currentRow, 24].PutValue(zone.Name);

                        foreach (StoreLeadTime slt in storeLeadTimeList.Where(slt => slt.Division == s.Division && 
                                                                                     slt.Store == s.Store && 
                                                                                     slt.Active == true && 
                                                                                     slt.Rank > 0).ToList())
                        {
                            col = slt.Rank + 3;
                            DistributionCenter distCenter = dcList.Where(dc => dc.ID == slt.DCID).FirstOrDefault();

                            dcName = string.Format("{0} - {1}", distCenter.MFCode, distCenter.Name);

                            currentSheet.Cells[currentRow, col].PutValue(dcName);
                            currentSheet.Cells[currentRow, col + 10].PutValue(slt.LeadTime);
                        }

                        currentRow++;
                        recordCount++;

                        if (currentRow >= maxSpreadsheetRows)
                        {
                            AutofitColumns();
                            worksheetNum++;
                            WriteHeaderRecord();
                        }
                    }
                }
            }

            AutofitColumns();
        }

        public StoreNSSExport(AppConfig config, ConfigService configService, NetworkZoneStoreDAO nssStoreDAO) : base(config)
        {
            maxColumns = 25;

            columns.Add(0, "Division");
            columns.Add(1, "Store");
            columns.Add(2, "City");
            columns.Add(3, "State");
            columns.Add(4, "Rank 1");
            columns.Add(5, "Rank 2");
            columns.Add(6, "Rank 3");
            columns.Add(7, "Rank 4");
            columns.Add(8, "Rank 5");
            columns.Add(9, "Rank 6");
            columns.Add(10, "Rank 7");
            columns.Add(11, "Rank 8");
            columns.Add(12, "Rank 9");
            columns.Add(13, "Rank 10");
            columns.Add(14, "Leadtime 1");
            columns.Add(15, "Leadtime 2");
            columns.Add(16, "Leadtime 3");
            columns.Add(17, "Leadtime 4");
            columns.Add(18, "Leadtime 5");
            columns.Add(19, "Leadtime 6");
            columns.Add(20, "Leadtime 7");
            columns.Add(21, "Leadtime 8");
            columns.Add(22, "Leadtime 9");
            columns.Add(23, "Leadtime 10");
            columns.Add(24, "Zone");

            this.configService = configService;
            networkZoneStoreDAO = nssStoreDAO;
        }
    }
}