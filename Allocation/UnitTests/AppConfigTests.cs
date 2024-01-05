using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Footlocker.Logistics.Allocation.Common;
using System.DirectoryServices;
using System.Collections.Generic;
using Footlocker.Common;

namespace AllocationTests
{
    [TestClass]
    public class AppConfigTests
    {
        [TestMethod]
        public void BasicValuesTest()
        {
            AppConfig appConfig = new AppConfig();
            Assert.AreEqual("C:\\Log\\allocation", appConfig.LogFile);
            Assert.AreEqual("\\Content\\Templates\\WebPickUploadTemplate.xls", appConfig.WebPickTemplate);
            Assert.AreEqual("C:\\Aspose\\Aspose.Cells.lic", appConfig.AsposeCellsLicenseFile);
        }
    }
}
