using Footlocker.Common;
using Footlocker.Logistics.Allocation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace Footlocker.Logistics.Allocation.Services
{
    public class HoldService
    {
        private readonly WebUser currentUser;
        private readonly ConfigService configService;
        private const string AppName = "Allocation";
        readonly AllocationLibraryContext db = new AllocationLibraryContext();
        public Hold Hold { get; set; }

        public string GetHoldTypePermissions()
        {
            string roles;

            switch (Hold.HoldType)
            {
                case "Dept":
                    roles = "Head Merchandiser,Space Planning,Director of Allocation,Admin,Support";
                    break;
                case "All":
                    roles = "Space Planning,Director of Allocation,Admin,Support,Advanced Merchandiser Processes";
                    break;
                default:
                    roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support";
                    break;
            }

            return roles;
        }

        public string CheckHoldPermission(WebUser currentUser)
        {
            string errorMessage;
            //check permission
            if (Hold.Level == "Dept")
                errorMessage = "You are not authorized to create Dept level holds.";
            else if (Hold.Level == "All")
                errorMessage = "You are not authorized to create Store level holds.";
            else
                errorMessage = "You are not authorized to create this hold.";

            if (!currentUser.HasUserRole(GetHoldTypePermissions().Split(',').ToList()))
                return errorMessage;
            else
                return "";
        }

        /// <summary>
        /// Performs various validations for a hold
        /// </summary>
        /// <param name="usesRuleSet">Only for store/all level holds. If this is true, store can be empty as it points to a rule set</param>
        /// <param name="edit">True if you are editing an existing hold. If false, the start date must be 2 days after control date</param>
        /// <param name="fromUpload">True if from a mass upload, false if not</param>
        /// <returns></returns>
        public string ValidateHold(bool edit, bool fromUpload = false)
        {
            string returnMessage = "";
            string value = Hold.Value + "";
            DateTime controlDate;
            Regex levelExpression;
            string levelError;

            controlDate = configService.GetControlDate(Hold.Division);

            returnMessage = CheckHoldPermission(currentUser);

            if (string.IsNullOrEmpty(returnMessage) && !edit)
            {
                if (Hold.StartDate <= controlDate.AddDays(2))
                    returnMessage = string.Format("Start date must be after {0}", controlDate.AddDays(2).ToShortDateString());
            }

            if (string.IsNullOrEmpty(returnMessage) && Hold.EndDate != null)
            {
                if (Hold.EndDate < Hold.StartDate)
                    returnMessage = "End Date must be after Start date.";
            }

            switch (Hold.Level)
            {
                case "Sku":
                    levelExpression = new Regex(@"^\d{2}-\d{2}-\d{5}-\d{2}$");
                    levelError = "Invalid Sku, format should be ##-##-#####-##";
                    break;

                case "Dept":
                    levelExpression = new Regex(@"^\d{2}$");
                    levelError = "Invalid Department, format should be ##";
                    break;

                case "VendorDept":
                    levelExpression = new Regex(@"^\d{5}-\d{2}$");
                    levelError = "Invalid Vendor-Dept, format should be #####-##";
                    break;

                case "VendorDeptCategory":
                    levelExpression = new Regex(@"^\d{5}-\d{2}-\d{3}$");
                    levelError = "Invalid Vendor-Dept-Category, format should be #####-##-###";
                    break;

                case "DeptBrand":
                    levelExpression = new Regex(@"^\d{2}-\d{3}$");
                    levelError = "Invalid Dept-Brand, format should be ##-###";
                    break;

                case "Category":
                    levelExpression = new Regex(@"^\d{2}-\d{3}$");
                    levelError = "Invalid Category, format should be ##-###";
                    break;

                case "DeptTeam":
                    levelExpression = new Regex(@"^\d{2}-\d{3}$");
                    levelError = "Invalid Dept-Team, format should be ##-###";
                    break;

                case "DeptCatTeam":
                    levelExpression = new Regex(@"^\d{2}-\d{3}-\d{3}$");
                    levelError = "Invalid Dept-Cat-Team, format should be ##-###-###";
                    break;

                case "DeptCatBrand":
                    levelExpression = new Regex(@"^\d{2}-\d{3}-\d{3}$");
                    levelError = "Invalid Dept-Cat-Brand, format should be ##-###-###";
                    break;

                default:
                    levelExpression = new Regex(@"^\d{5}$");
                    levelError = "Invalid Store, format should be #####";
                    break;
            }

            if (Hold.Level == "All")
                value = Hold.Store;

            if (string.IsNullOrEmpty(returnMessage))
            {
                if (!levelExpression.IsMatch(value))
                    returnMessage = levelError;

                if (Hold.Level == "Sku")
                {
                    if (!currentUser.HasDivision(Hold.Value.Substring(0, 2)))
                        returnMessage = "You do not have authority to create this hold. You need division level access.";
                    else if (!currentUser.HasDivDept(Hold.Value.Substring(0, 2), Hold.Value.Substring(3, 2)))
                        returnMessage = "You do not have authority to create this hold. You need dept level access.";
                    else if (Hold.Level == "Sku" && Hold.Division != Hold.Value.Substring(0, 2))
                        returnMessage = "Invalid Sku, division does not match selection.";
                }
                else if (Hold.Level == "Dept")
                {
                    if (!currentUser.HasDivDept(Hold.Division, Hold.Value))
                        returnMessage = "You do not have authority to create this hold. You need dept level access.";
                }
                else if (Hold.Level == "VendorDept")
                {
                    if (!currentUser.HasDivDept(Hold.Division, Hold.Value.Substring(6, 2)))
                        returnMessage = "You do not have authority to create this hold. You need dept level access.";
                }
                else if (Hold.Level == "VendorDeptCategory")
                {
                    if (!currentUser.HasDivDept(Hold.Division, Hold.Value.Substring(6, 2)))
                        returnMessage = "You do not have authority to create this hold. You need dept level access.";
                }
                else if (Hold.Level == "DeptBrand")
                {
                    if (!currentUser.HasDivDept(Hold.Division, Hold.Value.Substring(0, 2)))
                        returnMessage = "You do not have authority to create this hold. You need dept level access.";
                }
                else if (Hold.Level == "Category")
                {
                    if (!currentUser.HasDivDept(Hold.Division, Hold.Value.Substring(0, 2)))
                        returnMessage = "You do not have authority to create this hold.  You need dept level access.";
                }
                else if (Hold.Level == "DeptTeam")
                {
                    if (!currentUser.HasDivDept(Hold.Division, Hold.Value.Substring(0, 2)))
                        returnMessage = "You do not have authority to create this hold.  You need dept level access.";
                }
                else if (Hold.Level == "DeptCatTeam")
                {
                    if (!currentUser.HasDivDept(Hold.Division, Hold.Value.Substring(0, 2)))
                        returnMessage = "You do not have authority to create this hold.  You need dept level access.";
                }
                else if (string.Equals(Hold.Level, "DeptCatBrand"))
                {
                    if (!currentUser.HasDivDept(Hold.Division, Hold.Value.Substring(0, 2)))
                        returnMessage = "You do not have authority to create this hold.  You need dept level access.";
                }
                else
                {
                    //store hold
                    if (string.IsNullOrEmpty(Hold.Store))                                            
                        returnMessage = "For Store level, you must specify store.";
                    
                    Hold.Value = "N/A";
                }
            }

            if (string.IsNullOrEmpty(returnMessage))
            {
                if (Hold.Level == "All")
                {
                    if (db.Holds.Where(h => h.Division == Hold.Division &&
                                            h.Store == Hold.Store &&
                                            h.Level == Hold.Level &&
                                            h.ID != Hold.ID).Count() > 0)
                    {
                        returnMessage = string.Format("There is already a hold for {0}", Hold.Store);
                    }
                    else
                    {
                        int divDepts = DepartmentService.ListDepartments(Hold.Division).Count();
                        int enabledDepts = currentUser.GetUserDepartments().Where(m => m.DivCode == Hold.Division).Count();

                        if (divDepts != enabledDepts)
                            returnMessage = "You do not have authority to create this hold. Store level holds must have dept level access for ALL departments in the division.";
                    }
                }
                else
                {
                    // all the holds that are not this hold, but have the same level and value
                    var holds = db.Holds.Where(h => h.ID != Hold.ID && h.Level == Hold.Level && h.Value == Hold.Value).ToList();

                    // if not an upload, any existing holds matching division and store OR if an upload, any existing holds matching division where there is no
                    // end date or the new start date is after existing end dates
                    if (holds.Any(a => (!fromUpload &&
                                        a.Division == Hold.Division &&
                                         ((a.Store == Hold.Store) || (a.Store == null) &&
                                          (Hold.Store == null))) ||
                                       (fromUpload &&
                                       a.Division == Hold.Division &&
                                       a.Store == Hold.Store &&
                                         ((a.EndDate == null) || (a.EndDate > Hold.StartDate)))))
                    {
                        returnMessage = string.Format("There is already a hold for {0} {1} {2}", Hold.Store, Hold.Level, Hold.Value);
                    }
                }
            }

            if (string.IsNullOrEmpty(returnMessage) && !string.IsNullOrEmpty(Hold.Store))
            {
                if (Hold.Store.Length > 5)
                    returnMessage = "Store must be in format #####";
                else
                {
                    Hold.Store = Hold.Store.PadLeft(5, '0');

                    StoreLookup lookupStore = db.StoreLookups.Where(sl => sl.Store == Hold.Store && sl.Division == Hold.Division).FirstOrDefault();
                    if (lookupStore == null)
                        returnMessage = "Store ID is not valid or it is not under the selected division";
                }
            }

            if (string.IsNullOrEmpty(returnMessage) && Hold.ReserveInventory == 0)
            {
                RDQDAO dao = new RDQDAO();
                if (dao.GetRDQsForHold(Hold.ID).Count > 0)
                {
                    //if it was cancel inventory before, then let it be
                    var reserveinventory = (from a in db.Holds
                                            where a.ID == Hold.ID
                                            select a.ReserveInventory).First();

                    if (reserveinventory > 0)
                        returnMessage = "RDQs already assigned to this hold, you must release the RDQs before setting this to Cancel Inventory";
                }
            }

            return returnMessage;
        }

        public HoldService(WebUser currentUser, ConfigService configService) 
        { 
            this.currentUser = currentUser;
            this.configService = configService;
        }
    }
}
