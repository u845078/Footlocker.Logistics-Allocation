using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System.Collections.Generic;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class StoreNSSExport : ExportExcelSpreadsheet
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

            currentSheet = excelDocument.Worksheets[worksheetNum];

            WriteHeaderRecord();

            foreach (NetworkZone zone in networkZoneList)
            {
                storeList = config.db.NetworkZoneStores.Where(nzs => nzs.ZoneID == zone.ID).ToList();
                foreach (NetworkZoneStore s in storeList)
                {
                    if (s.Division == division)
                    {
                        currentSheet.Cells[currentRow, 0].PutValue(s.Division);
                        currentSheet.Cells[currentRow, 1].PutValue(s.Store);
                        currentSheet.Cells[currentRow, 22].PutValue(zone.Name);

                        foreach (StoreLeadTime slt in storeLeadTimeList.Where(slt => slt.Division == s.Division && 
                                                                                     slt.Store == s.Store && 
                                                                                     slt.Active == true && 
                                                                                     slt.Rank > 0).ToList())
                        {
                            col = slt.Rank + 1;
                            dcName = dcList.Where(dc => dc.ID == slt.DCID).FirstOrDefault().Name;

                            currentSheet.Cells[currentRow, col].PutValue(dcName);
                            currentSheet.Cells[currentRow, col + 10].PutValue(slt.LeadTime);
                        }

                        currentRow++;
                        recordCount++;

                        if (currentRow >= 65535)
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
            maxColumns = 23;

            columns.Add(0, "Division");
            columns.Add(1, "Store");
            columns.Add(2, "Rank 1");
            columns.Add(3, "Rank 2");
            columns.Add(4, "Rank 3");
            columns.Add(5, "Rank 4");
            columns.Add(6, "Rank 5");
            columns.Add(7, "Rank 6");
            columns.Add(8, "Rank 7");
            columns.Add(9, "Rank 8");
            columns.Add(10, "Rank 9");
            columns.Add(11, "Rank 10");
            columns.Add(12, "Leadtime 1");
            columns.Add(13, "Leadtime 2");
            columns.Add(14, "Leadtime 3");
            columns.Add(15, "Leadtime 4");
            columns.Add(16, "Leadtime 5");
            columns.Add(17, "Leadtime 6");
            columns.Add(18, "Leadtime 7");
            columns.Add(19, "Leadtime 8");
            columns.Add(20, "Leadtime 9");
            columns.Add(21, "Leadtime 10");
            columns.Add(22, "Zone");

            this.configService = configService;
            networkZoneStoreDAO = nssStoreDAO;
        }
    }
}