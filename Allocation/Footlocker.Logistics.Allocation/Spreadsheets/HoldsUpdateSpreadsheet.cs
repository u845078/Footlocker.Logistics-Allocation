using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class HoldsUpdateSpreadsheet : UploadSpreadsheet
    {
        public HoldsUpdateSpreadsheet(AppConfig config, ConfigService configService) : base(config, configService)
        {
            maxColumns = 9;
            headerRowNumber = 0;

            columns.Add(0, "Division");
            columns.Add(1, "Store");
            columns.Add(2, "Level");
            columns.Add(3, "Value");
            columns.Add(4, "Start Date");
            columns.Add(5, "End Date");
            columns.Add(6, "Duration");
            columns.Add(7, "Hold Type");
            columns.Add(8, "Comments");

            templateFilename = config.HoldsUploadTemplate;
        }
    }
}