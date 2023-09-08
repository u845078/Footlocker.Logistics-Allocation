using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Footlocker.Logistics.Allocation.Common;
using System.DirectoryServices;
using System.Collections.Generic;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Spreadsheets;
using Footlocker.Logistics.Allocation.Models;
using System.Data.Common.CommandTrees.ExpressionBuilder;

namespace AllocationTests
{
    [TestClass]
    public class ServiceTypeSpreadsheetTests
    {
        [TestMethod]
        public void ServiceTypeRecordFormatTest()
        {
            ServiceTypeUploadData uploadRec = new ServiceTypeUploadData()
            {
                SKU = "03-42-01512-04",
                ServiceType = "1",
                EffectiveDateString = Convert.ToDateTime("05/24/2023").ToString("yyyy-MM-dd"),
                Availability = "",
                UserID = "u695130", 
                currentDate = Convert.ToDateTime("2023-09-08T11:00:21.456189-04:00")
            };

            Assert.AreEqual("SRVTY03420151204                   2023-05-241                                                 2023-09-08-11.00.21.456189u695130                                ", uploadRec.GetServiceTypeDataString());
        }

        [TestMethod]
        public void AvailabilityRecordFormatTest()
        {
            ServiceTypeUploadData uploadRec = new ServiceTypeUploadData()
            {
                SKU = "03-42-01512-04",
                ServiceType = "1",
                EffectiveDateString = Convert.ToDateTime("05/24/2023").ToString("yyyy-MM-dd"),
                Availability = "A",
                UserID = "u695130",
                currentDate = Convert.ToDateTime("2023-09-08T11:00:21.456189-04:00")
            };

            Assert.AreEqual("SKUAV03420151204                   A                                                           2023-09-08-11.00.21.456189u695130                                ", uploadRec.GetAvailabilityDataString());
        }
    }
}
