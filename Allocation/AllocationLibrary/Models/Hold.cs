using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class Hold : BiExtract
    {
        public override bool IsValid()
        {
            return true;
        }

        public override Boolean CopyForAllStoresInDivision()
        {
            return (Store == "");
        }

        public override Boolean SubstituteStore(string division, string store)
        {
            if (this.Division == division)
            {
                this.Store = store;
                return true;
            }
            else
            {
                return false;
            }
        }
        [StringLayoutDelimited(0)]
        [XmlAttribute]
        public Int64 ID { get; set; }

        [StringLayoutDelimited(1)]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Division must be in the format ##")]
        [XmlAttribute]
        public string Division { get; set; }

        [StringLayoutDelimited(2)]
        [XmlIgnore]
        public string Store { get; set; }

        private string _level;

        [StringLayoutDelimited(3)]
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

        [StringLayoutDelimited(4)]
        [XmlIgnore]
        public string Value { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [StringLayoutDelimited(5, "yyyy-MM-dd")]
        [XmlIgnore]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [StringLayoutDelimited(6, "yyyy-MM-dd")]
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

        /// <summary>
        /// Gets or sets the reserved inventory character.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a value other than 'Y' or 'N' is specified.</exception>
        [StringLayoutDelimited(7)]
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
        [StringLayoutDelimited(8)]
        [XmlIgnore]
        public string Duration { get; set; }

        [StringLayoutDelimited(9)]
        [XmlIgnore]
        public string Comments { get; set; }

        [StringLayoutDelimited(10)]
        [XmlIgnore]
        public string CreatedBy { get; set; }

        [StringLayoutDelimited(11, "yyyy-MM-dd h:mm:ss tt")]
        [XmlIgnore]
        public DateTime? CreateDate { get; set; }

        [NotMapped]
        [StringLayoutDelimited(12)]
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
        /// Initializes a new instance of the Hold class.
        /// </summary>
        /// <param name="id">The initial value for the identifier property.</param>
        /// <param name="division">The initial value for the division property.</param>
        /// <param name="store">The initial value for the store property.</param>
        /// <param name="level">The initial value for the level property.</param>
        /// <param name="value">The initial value for the value property.</param>
        /// <param name="startDate">The initial value for the start date property.</param>
        /// <param name="endDate">The initial value for the end date property.</param>
        /// <param name="reserveInventoryChar">The initial value for the reserve inventory character property.</param>
        /// <param name="duration">The initial value for the duration property.</param>
        /// <param name="comments">The initial value for the comments property.</param>
        /// <param name="createdBy">The initial value for the created by property.</param>
        /// <param name="createDate">The initial value for the create date property.</param>
        public Hold(Int64 id, string division, string store, string bisku, string level, string value, DateTime startDate
                , DateTime? endDate, char reserveInventoryChar, string duration, string comments, string createdBy
                , DateTime? createDate)
            : this()
        {
            this.ID = id;
            this.Division = division;
            this.Store = store;
            this.BISKU = bisku;
            this.Level = level;
            this.Value = value;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.ReserveInventoryChar = reserveInventoryChar;
            this.Duration = duration;
            this.Comments = comments;
            this.CreatedBy = createdBy;
            this.CreateDate = createDate;
        }

        /// <summary>
        /// Overrides the Footlocker.Common 'StringLayoutDelimitedUtility' ToString for performance gain
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public override string ToString(char delimiter)
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
    }
}
