using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Common;
using Telerik.Web.Mvc;
using System.DirectoryServices;
using System.Web.Routing;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Controllers
{
    /// <summary>
    /// Inherit from your controller from this, and you will get sitewide security/etc.
    /// </summary>
    /// TODO:  Set the authorization to your webpages Active Directory Group
    [CheckAuthorization(Roles = "allocation-gs")]
    public class AppController : Controller
    {
        Footlocker.Logistics.Allocation.Services.FootLockerCommonContext flCommon = new Footlocker.Logistics.Allocation.Services.FootLockerCommonContext();

        public WebUser currentUser;
        public const string AppName = "Allocation";

        public string getFullUserNameFromDatabase(string fullUserID)
        {
            string fullName = "";

            try
            {
                string lookupUserID = fullUserID.Replace("CORP/", "");
                fullName = (from au in flCommon.ApplicationUsers
                            where au.UserName == lookupUserID
                            select au.FullName).Distinct().FirstOrDefault();

                if (string.IsNullOrEmpty(fullName))
                    return fullUserID;
                else
                    return fullName;
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
                return currentUser.NetworkID;
            }
        }

        private void LoadCurrentUser()
        {
            DirectoryEntry de;

            currentUser = new WebUser(Environment.UserDomainName, Environment.UserName);
            de = new DirectoryEntry(currentUser.ActiveDirectoryEntry);

            if (de.Guid != null)
                currentUser.FullName = de.Properties["fullname"].Value.ToString();
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

        public AppController()
        {
        }

        protected override void Initialize(RequestContext requestContext)
        {
            
            base.Initialize(requestContext);
            LoadCurrentUser();

            ViewBag.FullUserName = currentUser.FullName;
            ViewBag.CurrentDate = DateTime.Now;
        }
    }
}
