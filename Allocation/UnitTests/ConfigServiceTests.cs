using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Footlocker.Logistics.Allocation.Services;
using System.Data.Entity;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

namespace AllocationTests
{
    [TestClass]
    public class ConfigServiceTests
    {
        ConfigService _configService = new ConfigService();
        
        [TestMethod]
        public void GetInstanceTest()
        {
            int instanceID;
            instanceID = _configService.GetInstance("03");
            Assert.AreEqual(1, instanceID);
        }

        [TestMethod]
        public void GetControlDateTest()
        {
            DateTime expectedControlDate;
            DateTime actualControlDate;

            AllocationLibraryContext db = new AllocationLibraryContext();
            expectedControlDate = db.ControlDates.Where(cd => cd.InstanceID == 1).Select(cd => cd.RunDate).FirstOrDefault();

            actualControlDate = _configService.GetControlDate(1);
            Assert.AreEqual(expectedControlDate, actualControlDate);
        }
    }
}
