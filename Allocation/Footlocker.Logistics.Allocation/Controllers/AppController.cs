using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices;
using System.Web.Routing;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Controllers
{
    /// <summary>
    /// Inherit from your controller from this, and you will get sitewide security/etc.
    /// </summary>
    /// TODO:  Set the authorization to your webpages Active Directory Group
    //[CheckAuthorization(Roles = "allocation-gs")]
    public class AppController : Controller
    {
        Footlocker.Logistics.Allocation.Services.FootLockerCommonContext flCommon = new Footlocker.Logistics.Allocation.Services.FootLockerCommonContext();
        public AppConfig appConfig = new AppConfig();
        public AllocationLibraryContext allocDB = new AllocationLibraryContext();

        public WebUser currentUser;
        public const string AppName = "Allocation";

        public string GetFullUserNameFromDatabase(string fullUserID)
        {
            string fullName = string.Empty;

            try
            {
                if (!fullUserID.Contains(" ") && !string.IsNullOrEmpty(fullUserID))
                {
                    string lookupUserID = fullUserID.Replace("CORP/", "");

                    if (lookupUserID.Substring(0, 1) == "u")
                    {
                        fullName = (from au in flCommon.ApplicationUsers
                                    where au.UserName == lookupUserID
                                    select au.FullName).Distinct().FirstOrDefault();
                    }
                }

                if (string.IsNullOrEmpty(fullName))
                    return HttpUtility.HtmlEncode(fullUserID);
                else
                    return fullName;
            }
            catch
            {
                return HttpUtility.HtmlEncode(fullUserID);
            }
        }

        public List<ApplicationUser> GetAllUserNamesFromDatabase()
        {
            return flCommon.ApplicationUsers.ToList();
        }

        private void LoadCurrentUser()
        {
            DirectoryEntry de;

            currentUser = new WebUser(Environment.UserDomainName, User.Identity.Name.Replace("CORP\\", ""), AppName);
            de = new DirectoryEntry(currentUser.ActiveDirectoryEntry);

            if (de.Guid != null)
                currentUser.FullName = de.Properties["fullname"].Value.ToString();
        }

        public Dictionary<string, string> LoadUserNames(List<string> uniqueUserIDs)
        {
            Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();
            List<ApplicationUser> allUserNames = GetAllUserNamesFromDatabase();

            foreach (var item in uniqueUserIDs)
            {
                if (!item.Contains(" ") && !string.IsNullOrEmpty(item))
                {
                    string userLookup = item.Replace('\\', '/');
                    userLookup = userLookup.Replace("CORP/", "");

                    if (userLookup.Substring(0, 1) == "u")
                    {
                        string lookupName = allUserNames.Where(aun => aun.UserName == userLookup).Select(aun => aun.FullName).FirstOrDefault();

                        if (!string.IsNullOrEmpty(lookupName))
                            fullNamePairs.Add(item, lookupName);
                        else
                            fullNamePairs.Add(item, item);
                    }                        
                    else
                        fullNamePairs.Add(item, item);
                }
                else
                    fullNamePairs.Add(item, item);
            }

            return fullNamePairs;
        }

        public AppController()
        {
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            LoadCurrentUser();

            ViewBag.FullUserName = currentUser.FullName;
            ViewBag.NetworkID = currentUser.FullNetworkID;
            ViewBag.CurrentDate = DateTime.Now;

            appConfig.currentUser = currentUser;
            appConfig.AppName = AppName;
            appConfig.db = new DAO.AllocationContext();
            appConfig.allocDB = new AllocationLibraryContext();
            appConfig.AppPath = Server.MapPath("~/"); 
        }
    }
}
