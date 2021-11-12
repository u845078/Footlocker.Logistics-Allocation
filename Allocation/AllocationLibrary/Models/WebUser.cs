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

        public string GetUserDivisionsString(string app)
        {
            List<string> userDivCodes = GetUserDivList(app);
            return string.Join("", userDivCodes);
        }

        /// <summary>
        /// A quick check to see if the user has a division in their security
        /// </summary>
        /// <param name="app">The Application Name</param>
        /// <param name="division">The division you're looking for</param>
        /// <returns>boolean: true, they got it; false, they don't</returns>
        public bool HasDivision(string app, string division)
        {
            return GetUserDivisionsString(app).Contains(division);
        }

        /// <summary>
        /// This will return a list of user allowed divisions and departments
        /// </summary>
        /// <param name="app">The websecurity application name string</param>
        /// <returns>list of Department objects</returns>
        public List<Department> GetUserDepartments(string app)
        {
            if (UserDepartments == null)
            {
                UserDepartments = new List<Department>();

                foreach (string div in GetUserDivList(app))
                {
                    UserDepartments.AddRange(WebSecurityService.ListUserDepartments(NetworkID, app, div));
                }
            }

            return UserDepartments;
        }

        /// <summary>
        /// This will return a list of user allowed divisions and departments
        /// </summary>
        /// <param name="app">The websecurity application name string</param>
        /// <returns>list of div-dept strings</returns>
        public List<string> GetUserDevDept(string app)
        {
            List<string> temp = new List<string>();
            UserDepartments = GetUserDepartments(app);

            temp.AddRange(UserDepartments.Select(d => string.Format("{0}-{1}", d.DivCode, d.DeptNumber)).ToList());

            return temp;
        }

        /// <summary>
        /// This will return a list of div codes
        /// </summary>
        /// <param name="app">The websecurity application name string</param>
        /// <returns>list of div code strings</returns>
        public List<string> GetUserDivList(string app)
        {
            UserDivisions = GetUserDivisions(app);

            return UserDivisions.Select(d => d.DivCode).ToList();
        }

        public List<Division> GetUserDivisions(string app)
        {
            if (UserDivisions == null)
                UserDivisions = WebSecurityService.ListUserDivisions(NetworkID, app);

            return UserDivisions;
        }

        public WebUser(string userDomain, string networkID)
        {
            UserDomain = userDomain;
            NetworkID = networkID;
        }
    }
}
