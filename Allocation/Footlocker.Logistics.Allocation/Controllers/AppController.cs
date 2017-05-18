using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Common;
using Telerik.Web.Mvc;
using System.DirectoryServices;
using System.Web.Routing;

namespace Footlocker.Logistics.Allocation.Controllers
{

    /// <summary>
    /// Inherit from your controller from this, and you will get sitewide security/etc.
    /// </summary>
    /// TODO:  Set the authorization to your webpages Active Directory Group
    [CheckAuthorization(Roles = "allocation-gs")]
    public class AppController : Controller
    {
        public string getCurrentUserFullUserName()
        {
            try
            {
                DirectoryEntry de = new DirectoryEntry("WinNT://" + Environment.UserDomainName + "/" + Environment.UserName);
                return de.Properties["fullname"].Value.ToString();
            }
            catch
            {
                return null;
            }
        }

        public string getFullUserName(string fullUserID)
        {
            try
            {
                DirectoryEntry de = new DirectoryEntry("WinNT://" + fullUserID);
                return de.Properties["fullname"].Value.ToString();
            }
            catch
            {
                return fullUserID;
            }
        }

        public string UserName
        {
            get
            {
                return System.Web.HttpContext.Current.User.Identity.Name.ToLower().Replace("corp\\", "");
            }
        }

        public Alert UserAlert
        {
            get
            {
                Alert alert = null;

                if (Session["UserAlert"] != null)
                {
                    alert = (Alert)Session["UserAlert"];
                    Session["UserAlert"] = null;
                }

                return alert;
            }
            set
            {
                Session["UserAlert"] = value;
            }
        }

        private List<Division> _divisions;

        public List<Division> Divisions()
        {
            if (_divisions == null)
            {
                _divisions = WebSecurityService.ListUserDivisions(UserName, "Allocation");
            }
            return _divisions;
        }

        private List<Department> _departments;

        public List<Department> Departments()
        {
            if (_departments == null)
            {
                _departments = new List<Department>();
                foreach (Division d in Divisions())
                {
                    _departments.AddRange(WebSecurityService.ListUserDepartments(UserName, "Allocation", d.DivCode));
                }
            }
            return _departments;
        }

        public string DivisionList(string user)
        {
            string returnVal = "";
            foreach (Division d in Divisions())
            {
                returnVal = returnVal + d.DivCode;
            }
            return returnVal;
        }

        public AppController()
        {
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ViewBag.FullUserName = getCurrentUserFullUserName();
            ViewBag.CurrentDate = DateTime.Now;
        }
    }
}
