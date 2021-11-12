using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Footlocker.Logistics.Allocation.Models;
using System.DirectoryServices;
using System.Collections.Generic;
using Footlocker.Common;

namespace AllocationTests
{
    [TestClass]
    public class WebUserTests
    {
        [TestMethod]
        public void BasicValuesTest()
        {
            WebUser testUser = new WebUser("CORP", "u695130");
            Assert.AreEqual("CORP", testUser.UserDomain);
            Assert.AreEqual("u695130", testUser.NetworkID);
            Assert.AreEqual("CORP\\u695130", testUser.FullNetworkID);
            Assert.AreEqual("WinNT://CORP/u695130", testUser.ActiveDirectoryEntry);
        }

        [TestMethod]
        public void ADLookupTest()
        {
            DirectoryEntry de;

            WebUser testUser = new WebUser("CORP", "u695130");
            de = new DirectoryEntry(testUser.ActiveDirectoryEntry);

            if (de.Guid != null)
                testUser.FullName = de.Properties["fullname"].Value.ToString();

            Assert.AreEqual("Christopher Dick", testUser.FullName);
        }

        [TestMethod]
        public void UserDivListTest()
        {
            List<string> userDivisions;
            WebUser testUser = new WebUser("CORP", "u695130");
            userDivisions = testUser.GetUserDivList("Allocation");
            Assert.IsTrue(userDivisions.Contains("03"));
            Assert.IsTrue(userDivisions.Contains("18"));
            Assert.IsTrue(userDivisions.Contains("31"));
            Assert.IsTrue(userDivisions.Contains("76"));
            Assert.IsTrue(userDivisions.Contains("24"));
        }

        [TestMethod]
        public void UserDivisionsTest()
        {
            List<Division> userDivisions;
            WebUser testUser = new WebUser("CORP", "u695130");
            userDivisions = testUser.GetUserDivisions("Allocation");
            Assert.IsTrue(userDivisions.Exists(ud => ud.DivCode == "03"));
            Assert.IsTrue(userDivisions.Exists(ud => ud.DivCode == "18"));
            Assert.IsTrue(userDivisions.Exists(ud => ud.DivCode == "31"));
            Assert.IsTrue(userDivisions.Exists(ud => ud.DivCode == "76"));
            Assert.IsTrue(userDivisions.Exists(ud => ud.DivCode == "24"));
        }

        [TestMethod]
        public void UserDivisionsStringTest()
        {
            string userDivisions;
            WebUser testUser = new WebUser("CORP", "u695130");
            userDivisions = testUser.GetUserDivisionsString("Allocation");
            Assert.AreEqual("03040608161718242829314759737677818590", userDivisions);
        }

        [TestMethod]
        public void UserDivDeptsTest()
        {
            List<string> userDepartments;
            WebUser testUser = new WebUser("CORP", "u695130");
            userDepartments = testUser.GetUserDevDept("Allocation");
            Assert.IsTrue(userDepartments.Contains("03-17"));
            Assert.IsTrue(userDepartments.Contains("18-05"));
            Assert.IsTrue(userDepartments.Contains("16-05"));
            Assert.IsTrue(userDepartments.Contains("31-55"));
            Assert.IsTrue(userDepartments.Contains("76-55"));
            Assert.IsTrue(userDepartments.Contains("24-64"));
        }

        [TestMethod]
        public void UserDepartmentsTest()
        {
            List<Department> userDepartments;
            WebUser testUser = new WebUser("CORP", "u695130");
            userDepartments = testUser.GetUserDepartments("Allocation");
            Assert.IsTrue(userDepartments.Exists(ud => ud.DivCode == "03" && ud.DeptNumber == "17"));
            Assert.IsTrue(userDepartments.Exists(ud => ud.DivCode == "18" && ud.DeptNumber == "05"));
            Assert.IsTrue(userDepartments.Exists(ud => ud.DivCode == "16" && ud.DeptNumber == "05"));
            Assert.IsTrue(userDepartments.Exists(ud => ud.DivCode == "31" && ud.DeptNumber == "55"));
            Assert.IsTrue(userDepartments.Exists(ud => ud.DivCode == "76" && ud.DeptNumber == "55"));
            Assert.IsTrue(userDepartments.Exists(ud => ud.DivCode == "24" && ud.DeptNumber == "64"));
        }

        [TestMethod]
        public void UserHasDepartmentTest()
        {
            WebUser testUser = new WebUser("CORP", "u695130");
            bool dataCheck = testUser.HasDivision("Allocation", "31");
            Assert.IsTrue(dataCheck);
            dataCheck = testUser.HasDivision("Allocation", "03");
            Assert.IsTrue(dataCheck);
            dataCheck = testUser.HasDivision("Allocation", "99");
            Assert.IsFalse(dataCheck);
        }
    }
}
