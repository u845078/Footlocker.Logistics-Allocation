using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Footlocker.Logistics.Allocation.Models;
using System.Data;
using System.Collections.Generic;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Factories;

namespace AllocationTests
{
    [TestClass]
    public class RDQFactoryTests
    {
        DataSet testData = new DataSet();
        DataTable heldRDQs = new DataTable();

        [TestMethod]
        public void RDQConversionTest()
        {
            heldRDQs.Columns.Add("ID");
            heldRDQs.Columns.Add("Division");
            heldRDQs.Columns.Add("Store");
            heldRDQs.Columns.Add("DCID");
            heldRDQs.Columns.Add("Name");
            heldRDQs.Columns.Add("PO");
            heldRDQs.Columns.Add("ItemID");
            heldRDQs.Columns.Add("SKU");
            heldRDQs.Columns.Add("Size");
            heldRDQs.Columns.Add("Qty");
            heldRDQs.Columns.Add("UnitQty");
            heldRDQs.Columns.Add("CreatedBy");
            heldRDQs.Columns.Add("CreateDate");
            heldRDQs.Columns.Add("type");
            heldRDQs.Columns.Add("DestinationType");
            heldRDQs.Columns.Add("Status");
            heldRDQs.Columns.Add("TargetQty");
            heldRDQs.Columns.Add("ForecastQty");
            heldRDQs.Columns.Add("NeedQty");
            heldRDQs.Columns.Add("ExpectedShipDate");
            heldRDQs.Columns.Add("ExpectedDeliveryDate");
            heldRDQs.Columns.Add("OptimalQty");
            heldRDQs.Columns.Add("RequestedQty");
            heldRDQs.Columns.Add("UserRequestedQty");
            heldRDQs.Columns.Add("RDQRejectReasonCode");
            heldRDQs.Columns.Add("QuantumRecordTypeCode");
            heldRDQs.Columns.Add("LastModifiedDate");
            heldRDQs.Columns.Add("LastModifiedUser");
            heldRDQs.Columns.Add("TransmitControlDate");
            heldRDQs.Columns.Add("TransmittedToKafka");
            heldRDQs.Columns.Add("Dept");
            heldRDQs.Columns.Add("InstanceID");

            DataRow testRow = heldRDQs.NewRow();
            testRow["ID"] = "4201050924";
            testRow["Division"] = "03";
            testRow["Store"] = "07029";
            testRow["DCID"] = "1";
            testRow["Name"] = "Junction City";
            testRow["PO"] = "";
            testRow["ItemID"] = "4815207";
            testRow["SKU"] = "03-41-03914-04";
            testRow["Size"] = "110";
            testRow["Qty"] = "1";
            testRow["UnitQty"] = "1";
            testRow["CreatedBy"] = "batch";
            testRow["CreateDate"] = "2022-08-12 23:58:37.100";
            testRow["type"] = "Q";
            testRow["DestinationType"] = "WAREHOUSE";
            testRow["Status"] = "HOLD-STORE";
            testRow["TargetQty"] = null;
            testRow["ForecastQty"] = null;
            testRow["NeedQty"] = "0";
            testRow["ExpectedShipDate"] = "20220812";
            testRow["ExpectedDeliveryDate"] = "20220814";
            testRow["OptimalQty"] = "1";
            testRow["RequestedQty"] = null;
            testRow["UserRequestedQty"] = "1";
            testRow["RDQRejectReasonCode"] = null;
            testRow["QuantumRecordTypeCode"] = "2";
            testRow["LastModifiedDate"] = "2022-09-15 15:48:56.190";
            testRow["LastModifiedUser"] = "CORP\\u695130";
            testRow["TransmitControlDate"] = null;
            testRow["TransmittedToKafka"] = "0";
            testRow["Dept"] = "41";
            testRow["InstanceID"] = "1";
            heldRDQs.Rows.Add(testRow);

            testRow = heldRDQs.NewRow();
            testRow["ID"] = "3304943335";
            testRow["Division"] = "03";
            testRow["Store"] = "07010";
            testRow["DCID"] = "1";
            testRow["Name"] = "Junction City";
            testRow["PO"] = "7520101";
            testRow["ItemID"] = "5738442";
            testRow["SKU"] = "03-46-00532-04";
            testRow["Size"] = "120";
            testRow["Qty"] = "1";
            testRow["UnitQty"] = "1";
            testRow["CreatedBy"] = "CORP\\u488839";
            testRow["CreateDate"] = "2021-10-25 17:28:47.367";
            testRow["type"] = "user";
            testRow["DestinationType"] = "CROSSDOCK";
            testRow["Status"] = "HOLD-XDC";
            testRow["TargetQty"] = null;
            testRow["ForecastQty"] = null;
            testRow["NeedQty"] = null;
            testRow["ExpectedShipDate"] = null;
            testRow["ExpectedDeliveryDate"] = null;
            testRow["OptimalQty"] = null;
            testRow["RequestedQty"] = null;
            testRow["UserRequestedQty"] = null;
            testRow["RDQRejectReasonCode"] = null;
            testRow["QuantumRecordTypeCode"] = "3";
            testRow["LastModifiedDate"] = "2022-09-15 15:48:56.190";
            testRow["LastModifiedUser"] = "CORP\\u695130";
            testRow["TransmitControlDate"] = null;
            testRow["TransmittedToKafka"] = "0";
            testRow["Dept"] = "46";
            testRow["InstanceID"] = "1";
            heldRDQs.Rows.Add(testRow);

            testData.Tables.Add(heldRDQs);

            List<RDQ> testRDQs = new List<RDQ>();

            foreach (DataRow row in testData.Tables[0].Rows)
            {
                testRDQs.Add(RDQFactory.CreateFromHeldRDQRow(row));
            }

            Assert.AreEqual(2, testRDQs.Count);

            Assert.AreEqual(4201050924, testRDQs[0].ID);
            Assert.AreEqual("03", testRDQs[0].Division);
            Assert.AreEqual("07029", testRDQs[0].Store);
            Assert.AreEqual("Junction City", testRDQs[0].WarehouseName);
            Assert.AreEqual(1, testRDQs[0].DCID.Value);
            Assert.AreEqual("", testRDQs[0].PO);
            Assert.AreEqual(4815207, testRDQs[0].ItemID.Value);
            Assert.AreEqual("03-41-03914-04", testRDQs[0].Sku);
            Assert.AreEqual("110", testRDQs[0].Size);
            Assert.AreEqual(1, testRDQs[0].Qty);
            Assert.AreEqual(1, testRDQs[0].UnitQty);
            Assert.AreEqual("batch", testRDQs[0].CreatedBy);
            Assert.AreEqual("2022-08-12 23:58:37.1", testRDQs[0].CreateDate.Value.ToString("yyyy-MM-dd HH:mm:ss.F"));
            Assert.AreEqual("Q", testRDQs[0].Type);
            Assert.AreEqual("WAREHOUSE", testRDQs[0].DestinationType);
            Assert.AreEqual("HOLD-STORE", testRDQs[0].Status);
            Assert.AreEqual("41", testRDQs[0].Department);
            Assert.IsNull(testRDQs[0].RDQRejectedReasonCode);
            Assert.AreEqual(1, testRDQs[0].InstanceID);

            Assert.AreEqual(3304943335, testRDQs[1].ID);
            Assert.AreEqual("03", testRDQs[1].Division);
            Assert.AreEqual("07010", testRDQs[1].Store);
            Assert.AreEqual("Junction City", testRDQs[1].WarehouseName);
            Assert.AreEqual(1, testRDQs[1].DCID.Value);
            Assert.AreEqual("7520101", testRDQs[1].PO);
            Assert.AreEqual(5738442, testRDQs[1].ItemID.Value);
            Assert.AreEqual("03-46-00532-04", testRDQs[1].Sku);
            Assert.AreEqual("120", testRDQs[1].Size);
            Assert.AreEqual(1, testRDQs[1].Qty);
            Assert.AreEqual(1, testRDQs[1].UnitQty);
            Assert.AreEqual("CORP\\u488839", testRDQs[1].CreatedBy);
            Assert.AreEqual("2021-10-25 17:28:47.367", testRDQs[1].CreateDate.Value.ToString("yyyy-MM-dd HH:mm:ss.FFF"));
            Assert.AreEqual("user", testRDQs[1].Type);
            Assert.AreEqual("CROSSDOCK", testRDQs[1].DestinationType);
            Assert.AreEqual("HOLD-XDC", testRDQs[1].Status);
            Assert.AreEqual("46", testRDQs[1].Department);
            Assert.IsNull(testRDQs[1].RDQRejectedReasonCode);
            Assert.AreEqual(1, testRDQs[1].InstanceID);
        }
    }
}
