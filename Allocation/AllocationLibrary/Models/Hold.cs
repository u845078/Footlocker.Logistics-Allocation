using Footlocker.Common;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;


namespace Footlocker.Logistics.Allocation.Models
{
    public class Hold 
    {
        private const string AppName = "Allocation";
        readonly AllocationLibraryContext db = new AllocationLibraryContext();

        [XmlAttribute]
        public Int64 ID { get; set; }

        [RegularExpression(@"^\d{2}$", ErrorMessage = "Division must be in the format ##")]
        [XmlAttribute]
        public string Division { get; set; }

        [XmlIgnore]
        public string Store { get; set; }

        private string _level;

        [XmlIgnore]
        public string Level 
        { 
            get 
            {
                if (_level == null)
                {
                    _level = "";
                }
                return _level;
            }

            set 
            { 
                _level = value; 
            }
        }

        [XmlIgnore]
        public string Value { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [XmlIgnore]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [XmlIgnore]
        public DateTime? EndDate { get; set; }

        [XmlIgnore]
        public Int16 ReserveInventory { get; set; }

        [NotMapped]
        [XmlIgnore]
        public Boolean ReserveInventoryBool
        {
            get 
            {
                return Convert.ToBoolean(ReserveInventory);
            }
            set 
            {
                ReserveInventory = Convert.ToInt16(value);
            }
        }

        [NotMapped]
        [XmlIgnore]
        public string HoldType
        {
            get 
            {
                if (Convert.ToBoolean(ReserveInventory))
                {
                    return "Reserve Inventory";
                }
                else
                {
                    return "Cancel Inventory";
               }
            }

            set 
            {
                if (value.Equals("Reserve Inventory"))
                {
                    ReserveInventory = 1;
                }
                else
                {
                    ReserveInventory = 0;
                }
            }
        }

        [NotMapped]
        [XmlIgnore]
        public string Department { get; set; }

        [NotMapped]
        [XmlIgnore]
        public string Brand { get; set; }

        [NotMapped]
        [XmlIgnore]
        public string Team { get; set; }

        [NotMapped]
        [XmlIgnore]
        public string Category { get; set; }

        [NotMapped]
        [XmlIgnore]
        public string Vendor { get; set; }

        [NotMapped]
        [XmlIgnore]
        public string SKU { get; set; }

        [NotMapped]
        [XmlIgnore]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the reserved inventory character.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a value other than 'Y' or 'N' is specified.</exception>
        [XmlIgnore]
        public char ReserveInventoryChar
        {
            get { return this.ReserveInventoryBool ? 'Y' : 'N'; }
            set
            {
                switch (value)
                {
                    case 'N':
                        this.ReserveInventoryBool = false;
                        break;
                    case 'Y':
                        this.ReserveInventoryBool = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value", value
                            , "An value other than 'Y' or 'N' was specified for the reserve inventory indicator.");
                }
            }
        }

        [NotMapped]
        [XmlIgnore]
        public string ReleaseButtonText
        {
            get
            {
                if (Convert.ToBoolean(ReserveInventory))
                {
                    return "Release";
                }
                else
                {
                    return "Delete";
                }
            }

            set
            {
            }
        }

        /// <summary>
        /// Temporary or Permanent, for easy filtering on screen
        /// </summary>
        [XmlIgnore]
        public string Duration { get; set; }

        [XmlIgnore]
        public string Comments { get; set; }

        [XmlIgnore]
        public string CreatedBy { get; set; }

        [XmlIgnore]
        public DateTime? CreateDate { get; set; }

        [NotMapped]
        [XmlIgnore]
        public string BISKU { get; set; }

        /// <summary>
        /// Initialize a new instance of the Hold class.
        /// </summary>
        public Hold()
        {
            this.ID = 0L;
            this.Division = String.Empty;
            this.BISKU = string.Empty;
            this.Store = String.Empty;
            this.Level = String.Empty;
            this.Value = String.Empty;
            this.StartDate = DateTime.MinValue;
            this.EndDate = new DateTime?();
            this.ReserveInventory = 0;
            this.Duration = String.Empty;
            this.Comments = String.Empty;
            this.CreatedBy = String.Empty;
            this.CreateDate = new DateTime?();
        }

        /// <summary>
        /// Overrides the Footlocker.Common 'StringLayoutDelimitedUtility' ToString for performance gain
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public string ToString(char delimiter)
        {
            var resultBuilder = new StringBuilder();
            var delimiterString = delimiter.ToString();

            resultBuilder.Append(ID.ToString()).Append(delimiterString);
            resultBuilder.Append(Division).Append(delimiterString);
            resultBuilder.Append(Store).Append(delimiterString);
            resultBuilder.Append(Level).Append(delimiterString);
            resultBuilder.Append(Value).Append(delimiterString);
            resultBuilder.Append(Convert.ToDateTime(StartDate).ToString("M/d/yyyy h:mm:ss tt")).Append(delimiterString);
            resultBuilder.Append(EndDate.HasValue ? Convert.ToDateTime(EndDate).ToString("M/d/yyyy h:mm:ss tt") : String.Empty).Append(delimiterString);
            resultBuilder.Append(this.ReserveInventoryChar.ToString()).Append(delimiterString);
            resultBuilder.Append(Duration).Append(delimiterString);
            resultBuilder.Append(Comments).Append(delimiterString);
            resultBuilder.Append(CreatedBy).Append(delimiterString);
            resultBuilder.Append(CreateDate.HasValue ? Convert.ToDateTime(CreateDate).ToString("M/d/yyyy h:mm:ss tt") : String.Empty).Append(delimiterString);
            resultBuilder.Append(BISKU).Append(delimiterString);

            return resultBuilder.ToString();
        }

        public static string ToXml(List<Hold> list)
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<Hold>));
            System.IO.StringWriter sw = new System.IO.StringWriter();
            ser.Serialize(sw, list);
            return sw.ToString();
        }

        public string GetHoldTypePermissions()
        {
            string roles;

            switch (HoldType)
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
            if (Level == "Dept")
                errorMessage = "You are not authorized to create Dept level holds.";
            else if (Level == "All")
                errorMessage = "You are not authorized to create Store level holds.";
            else
                errorMessage = "You are not authorized to create this hold.";

            if (!currentUser.HasUserRole(AppName, GetHoldTypePermissions().Split(',').ToList()))
                return errorMessage;
            else
                return "";
        }

        /// <summary>
        /// Performs various validations for a hold
        /// </summary>
        /// <param name="currentUser">The currently logged in user</param>
        /// <param name="configService">A service used for getting application configuration data</param>
        /// <param name="usesRuleSet">Only for store/all level holds. If this is true, store can be empty as it points to a rule set</param>
        /// <param name="edit">True if you are editing an existing hold. If false, the start date must be 2 days after control date</param>
        /// <param name="fromUpload">True if from a mass upload, false if not</param>
        /// <returns></returns>
        public string ValidateHold(WebUser currentUser, ConfigService configService, bool usesRuleSet, bool edit, bool fromUpload = false)
        {
            string returnMessage = "";
            string value = Value + "";
            int instanceID;
            DateTime controlDate;
            Regex levelExpression;
            string levelError;
            
            instanceID = configService.GetInstance(Division);
            controlDate = configService.GetControlDate(instanceID);

            returnMessage = CheckHoldPermission(currentUser);

            if (string.IsNullOrEmpty(returnMessage) && !edit)
            {
                if (StartDate <= controlDate.AddDays(2))                
                    returnMessage = string.Format("Start date must be after {0}", controlDate.AddDays(2).ToShortDateString());                
            }

            if (string.IsNullOrEmpty(returnMessage))
            {
                if (Level == "All")
                {
                    if (db.Holds.Where(h => h.Division == Division && 
                                            h.Store == Store && 
                                            h.Level == Level && 
                                            h.ID != ID).Count() > 0)
                    {
                        returnMessage = string.Format("There is already a hold for {0}", Store);
                    }
                    else
                    {
                        int divDepts = db.Departments.Where(d => d.divisionCode == Division).Count();
                        int enabledDepts = currentUser.GetUserDepartments(AppName).Where(m => m.DivCode == Division).Count();

                        if (divDepts != enabledDepts)                        
                            returnMessage = "You do not have authority to create this hold. Store level holds must have dept level access for ALL departments in the division.";                        
                    }
                }
                else
                {
                    // all the holds that are not this hold, but have the same level and value
                    var holds = db.Holds.Where(h => h.ID != ID && h.Level == Level && h.Value == Value).ToList();

                    // if not an upload, any existing holds matching division and store OR if an upload, any existing holds matching division where there is no
                    // end date or the new start date is after existing end dates
                    if (holds.Any(a => (!fromUpload &&
                                        a.Division == Division &&
                                         ((a.Store == Store) || (a.Store == null) &&
                                          (Store == null))) ||
                                       (fromUpload &&
                                       a.Division == Division &&
                                       a.Store == Store &&
                                         ((a.EndDate == null) || (a.EndDate > StartDate)))))
                    {
                        returnMessage = string.Format("There is already a hold for {0} {1} {2}", Store, Level, Value);
                    }
                }
            }

            if (string.IsNullOrEmpty(returnMessage) && EndDate != null)
            {
                if (EndDate < StartDate)                
                    returnMessage = "End Date must be after Start date.";                
            }

            if (string.IsNullOrEmpty(returnMessage) && !string.IsNullOrEmpty(Store))
            {
                if (Store.Length > 5)                
                    returnMessage = "Store must be in format #####";                
                else
                {
                    Store = Store.PadLeft(5, '0');

                    StoreLookup lookupStore = db.StoreLookups.Where(sl => sl.Store == Store && sl.Division == Division).FirstOrDefault();
                    if (lookupStore == null)                    
                        returnMessage = "Store ID is not valid or it is not under the selected division";                    
                }
            }

            switch (Level)
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

            if (string.IsNullOrEmpty(returnMessage))
            {
                if (!levelExpression.IsMatch(value))
                    returnMessage = levelError;

                if (Level == "Sku")
                {                   
                    if (!currentUser.HasDivision(AppName, Value.Substring(0, 2)))                    
                        returnMessage = "You do not have authority to create this hold. You need division level access.";                    
                    else if (!currentUser.HasDivDept(AppName, Value.Substring(0, 2), Value.Substring(3, 2)))                    
                        returnMessage = "You do not have authority to create this hold. You need dept level access.";                    
                    else if (Level == "Sku" && Division != Value.Substring(0, 2))                    
                        returnMessage = "Invalid Sku, division does not match selection.";                    
                }
                else if (Level == "Dept")
                {
                    if (!currentUser.HasDivDept(AppName, Division, Value))                    
                        returnMessage = "You do not have authority to create this hold. You need dept level access.";                    
                }
                else if (Level == "VendorDept")
                {
                    if (!currentUser.HasDivDept(AppName, Division, Value.Substring(6, 2)))                    
                        returnMessage = "You do not have authority to create this hold. You need dept level access.";                    
                }
                else if (Level == "VendorDeptCategory")
                {
                    if (!currentUser.HasDivDept(AppName, Division, Value.Substring(6, 2)))                    
                        returnMessage = "You do not have authority to create this hold. You need dept level access.";                    
                }
                else if (Level == "DeptBrand")
                {
                    if (!currentUser.HasDivDept(AppName, Division, Value.Substring(0, 2)))                    
                        returnMessage = "You do not have authority to create this hold. You need dept level access.";                    
                }
                else if (Level == "Category")
                {
                    if (!currentUser.HasDivDept(AppName, Division, Value.Substring(0, 2)))                    
                        returnMessage = "You do not have authority to create this hold.  You need dept level access.";                    
                }
                else if (string.Equals(Level, "DeptTeam"))
                {
                    if (!currentUser.HasDivDept(AppName, Division, Value.Substring(0, 2)))                    
                        returnMessage = "You do not have authority to create this hold.  You need dept level access.";                    
                }
                else if (string.Equals(Level, "DeptCatTeam"))
                {
                    if (!currentUser.HasDivDept(AppName, Division, Value.Substring(0, 2)))                    
                        returnMessage = "You do not have authority to create this hold.  You need dept level access.";                    
                }
                else if (string.Equals(Level, "DeptCatBrand"))
                {
                    if (!currentUser.HasDivDept(AppName, Division, Value.Substring(0, 2)))                    
                        returnMessage = "You do not have authority to create this hold.  You need dept level access.";                    
                }
                else
                {
                    //store hold
                    if ((Store == null) || (Store.Trim() == ""))
                    {
                        if (!usesRuleSet)                        
                            returnMessage = "For Store level, you must specify store.";                        
                    }
                    Value = "N/A";
                }
            }

            if (string.IsNullOrEmpty(returnMessage) && ReserveInventory == 0)
            {
                RDQDAO dao = new RDQDAO();
                if (dao.GetRDQsForHold(ID).Count > 0)
                {
                    //if it was cancel inventory before, then let it be
                    var reserveinventory = (from a in db.Holds 
                                            where a.ID == ID 
                                            select a.ReserveInventory).First();

                    if (reserveinventory > 0)                    
                        returnMessage = "RDQs already assigned to this hold, you must release the RDQs before setting this to Cancel Inventory";                    
                }
            }

            return returnMessage;
        }
    }
}
