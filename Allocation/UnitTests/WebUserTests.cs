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
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            Assert.AreEqual("CORP", testUser.UserDomain);
            Assert.AreEqual("u695130", testUser.NetworkID);
            Assert.AreEqual("CORP\\u695130", testUser.FullNetworkID);
            Assert.AreEqual("WinNT://CORP/u695130", testUser.ActiveDirectoryEntry);
        }

        [TestMethod]
        public void ADLookupTest()
        {
            DirectoryEntry de;

            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            de = new DirectoryEntry(testUser.ActiveDirectoryEntry);

            if (de.Guid != null)
                testUser.FullName = de.Properties["fullname"].Value.ToString();

            Assert.AreEqual("Christopher Dick", testUser.FullName);
        }

        [TestMethod]
        public void UserDivListTest()
        {
            List<string> userDivisions;
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            userDivisions = testUser.GetUserDivList();
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
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            userDivisions = testUser.GetUserDivisions();
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
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            userDivisions = testUser.GetUserDivisionsString();
            Assert.AreEqual("03040608161718242829314759737677818590", userDivisions);
        }

        [TestMethod]
        public void UserDivDeptsTest()
        {
            List<string> userDepartments;
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            userDepartments = testUser.GetUserDivDept();
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
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            userDepartments = testUser.GetUserDepartments();
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
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            bool dataCheck = testUser.HasDivision("31");
            Assert.IsTrue(dataCheck);
            dataCheck = testUser.HasDivision("03");
            Assert.IsTrue(dataCheck);
            dataCheck = testUser.HasDivision("99");
            Assert.IsFalse(dataCheck);
        }

        [TestMethod]
        public void UserHasDivDeptTest()
        {
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            Assert.IsTrue(testUser.HasDivDept("03", "17"));
            Assert.IsTrue(testUser.HasDivDept("18", "05"));
            Assert.IsTrue(testUser.HasDivDept("16", "05"));
            Assert.IsTrue(testUser.HasDivDept("31", "55"));
            Assert.IsFalse(testUser.HasDivDept("99", "99"));
        }

        [TestMethod]
        public void UserRoleListTest()
        {
            List<string> userRoles;
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            userRoles = testUser.GetUserRoles();
            Assert.IsTrue(userRoles.Contains("IT"));
            Assert.IsTrue(userRoles.Contains("Support"));
            Assert.IsTrue(userRoles.Contains("EPick"));
            Assert.IsFalse(userRoles.Contains("Director"));
        }

        [TestMethod]
        public void UserRoleTests()
        {
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            Assert.IsTrue(testUser.HasUserRole("IT"));
            Assert.IsTrue(testUser.HasUserRole("Support"));
            Assert.IsTrue(testUser.HasUserRole("EPick"));
            Assert.IsFalse(testUser.HasUserRole("Director"));
        }

        [TestMethod]
        public void UserRoleList2Test()
        {
            List<string> rolesToFind = new List<string>() { "IT", "ABC" };
            WebUser testUser = new WebUser("CORP", "u695130", "Allocation");
            Assert.IsTrue(testUser.HasUserRole(rolesToFind));

            rolesToFind = new List<string>() { "NOT", "FOUND" };
            Assert.IsFalse(testUser.HasUserRole(rolesToFind));

            rolesToFind = new List<string>() { "IT", "Support", "EPick" };
            Assert.IsTrue(testUser.HasUserRole(rolesToFind));
        }
    }
}
