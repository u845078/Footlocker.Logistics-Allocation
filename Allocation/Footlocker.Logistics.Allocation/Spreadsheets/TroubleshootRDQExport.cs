using System;
using System.Collections.Generic;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class TroubleshootRDQExport : ExportExcelSpreadsheet
    {
        readonly private RDQDAO rdqDAO;

        public void WriteData(string sku, DateTime controldate)
        {
            WriteHeaderRecord();

            List<RDQ> rdqList = rdqDAO.GetRDQExtractForSkuDate(sku, controldate);

            foreach (RDQ rdq in rdqList)
            {
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 0].PutValue(rdq.Sku);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 1].PutValue(rdq.Division);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 2].PutValue(rdq.Size);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 3].PutValue(rdq.DestinationType);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 4].PutValue(rdq.Type);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 5].PutValue(rdq.Store);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 6].PutValue(rdq.Status);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 7].PutValue(rdq.DC);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 8].PutValue(rdq.WarehouseName);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 9].PutValue(rdq.PO);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 10].PutValue(rdq.Qty);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 11].PutValue(rdq.UnitQty);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 12].PutValue(rdq.RecordType);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 13].PutValue(rdq.QuantumRecordType.RecordTypeDesc);
                excelDocument.Worksheets[worksheetNum].Cells[currentRow, 14].PutValue(rdq.RDQRejectedReason.Description);

                currentRow++;
            }

            AutofitColumns();
        }

        public TroubleshootRDQExport(AppConfig config, RDQDAO rdqDAO) : base(config)
        {
            maxColumns = 15;

            columns.Add(0, "SKU");
            columns.Add(1, "Division");
            columns.Add(2, "Size/Caselot");
            columns.Add(3, "Destination");
            columns.Add(4, "Type");
            columns.Add(5, "Store");
            columns.Add(6, "Status");
            columns.Add(7, "DC");
            columns.Add(8, "Warehouse");
            columns.Add(9, "PO");
            columns.Add(10, "Pick Qty");
            columns.Add(11, "Unit Qty");
            columns.Add(12, "Record Type");
            columns.Add(13, "Record Type Description");
            columns.Add(14, "Rejected Reason");

            this.rdqDAO = rdqDAO;
        }
    }
}