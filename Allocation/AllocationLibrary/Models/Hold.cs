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
        [XmlAttribute]
        public long ID { get; set; }

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
        public short ReserveInventory { get; set; }

        [NotMapped]
        [XmlIgnore]
        public bool ReserveInventoryBool
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
                if (ReserveInventory == 1)                
                    return "Reserve Inventory";                
                else                
                    return "Cancel Inventory";               
            }
            set 
            {
                if (value.ToLower() == "reserve inventory")                
                    ReserveInventory = 1;                
                else                
                    ReserveInventory = 0;                
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
            set  {   }
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
            resultBuilder.Append(ReserveInventoryChar.ToString()).Append(delimiterString);
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
    }
}
