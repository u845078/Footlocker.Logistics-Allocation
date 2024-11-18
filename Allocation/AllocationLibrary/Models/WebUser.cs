using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Footlocker.Common;


namespace Footlocker.Logistics.Allocation.Models
{
    public class WebUser
    {
        public string UserDomain { get; set; }

        public string FullName { get; set; }

        public string NetworkID { get; set; }

        public string AppName { get; set; }

        public string ActiveDirectoryEntry
        {
            get
            {
                return string.Format("WinNT://{0}/{1}", UserDomain, NetworkID);
            }
        }

        public string FullNetworkID
        {
            get
            {
                return string.Format("{0}\\{1}", UserDomain, NetworkID);
            }
        }

        private List<Division> UserDivisions { get; set; }
        private List<Department> UserDepartments { get; set; }
        private List<string> UserRoles { get; set; }

        public bool HasUserRole(List<string> roles)
        {
            return roles.Intersect(GetUserRoles()).Count() > 0;
        }

        /// <summary>
        /// Checks to see if a user has a specific role
        /// </summary>
        /// <param name="app">The app name</param>
        /// <param name="role">The role you're looking for</param>
        /// <returns>boolean if it is found</returns>
        public bool HasUserRole(string role)
        {
            return GetUserRoles().Contains(role);
        }

        public bool HasDivisionRole(string role, string division)
        {
            if (HasUserRole(role))            
                return WebSecurityService.UserHasDivisionRole(NetworkID, AppName, division, role);            
            else 
                return false;
        }

        /// <summary>
        /// Gets a list of roles that the current user is a part of
        /// </summary>
        /// <param name="app">Application name</param>
        /// <returns>List of role strings</returns>
        public List<string> GetUserRoles()
        {
            if (UserRoles == null)
                UserRoles = WebSecurityService.ListUserRoles(NetworkID, AppName);

            return UserRoles;
        }

        /// <summary>
        /// Returns all the user divisions concatenated into one string
        /// </summary>
        /// <param name="app">The application name</param>
        /// <returns></returns>
        public string GetUserDivisionsString()
        {
            List<string> userDivCodes = GetUserDivList();
            return string.Join("", userDivCodes);
        }

        /// <summary>
        /// A quick check to see if the user has a division in their security
        /// </summary>
        /// <param name="app">The Application Name</param>
        /// <param name="division">The division you're looking for</param>
        /// <returns>boolean: true, they got it; false, they don't</returns>
        public bool HasDivision(string division)
        {
            return GetUserDivisionsString().Contains(division);
        }

        /// <summary>
        /// Checks to see if the user has a both the division and department for an app
        /// </summary>
        /// <param name="app">The websecurity application name string</param>
        /// <param name="div">Division</param>
        /// <param name="dept">Division</param>
        /// <returns></returns>
        public bool HasDivDept(string div, string dept)
        {
            return GetUserDivDept().Contains(string.Format("{0}-{1}", div, dept));
        }

        /// <summary>
        /// This will return a list of user allowed divisions and departments
        /// </summary>
        /// <param name="app">The websecurity application name string</param>
        /// <returns>list of Department objects</returns>
        public List<Department> GetUserDepartments()
        {
            if (UserDepartments == null)
            {
                UserDepartments = new List<Department>();

                foreach (string div in GetUserDivList())
                {
                    UserDepartments.AddRange(WebSecurityService.ListUserDepartments(NetworkID, AppName, div));
                }
            }

            return UserDepartments;
        }

        /// <summary>
        /// This will return a list of user allowed divisions and departments
        /// </summary>
        /// <param name="app">The websecurity application name string</param>
        /// <returns>list of div-dept strings</returns>
        public List<string> GetUserDivDept()
        {
            List<string> temp = new List<string>();
            UserDepartments = GetUserDepartments();

            temp.AddRange(UserDepartments.Select(d => string.Format("{0}-{1}", d.DivCode, d.DeptNumber)).ToList());

            return temp;
        }

        /// <summary>
        /// This will return a list of div codes
        /// </summary>
        /// <param name="app">The websecurity application name string</param>
        /// <returns>list of div code strings</returns>
        public List<string> GetUserDivList()
        {
            UserDivisions = GetUserDivisions();

            return UserDivisions.Select(d => d.DivCode).ToList();
        }

        public List<Division> GetUserDivisions()
        {
            if (UserDivisions == null)
                UserDivisions = WebSecurityService.ListUserDivisions(NetworkID, AppName);

            return UserDivisions;
        }

        public WebUser(string userDomain, string networkID, string appName)
        {
            UserDomain = userDomain;
            NetworkID = networkID;
            AppName = appName;
        }
    }
}
