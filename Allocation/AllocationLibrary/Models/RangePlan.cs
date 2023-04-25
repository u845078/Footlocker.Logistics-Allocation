using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RangePlan
    {
        private long _id;

        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _sku;

        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        public string Sku
        {
            get { return _sku; }
            set { _sku = value; }
        }

        private string _description;

        [Display(Name = "Range Description")]
        [StringLength(50, ErrorMessage = "Max length 50 characters")]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private DateTime? _startDate;

        public DateTime? StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        private DateTime? _endDate;

        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        private string _planType;

        [NotMapped]
        public string PlanType
        {
            get
            {
                if (ALRStartDate != null)
                {
                    return "ALR";
                }
                else
                    return "OP";
            }
            set { _planType = value; }
        }

        private string _updatedBy;

        public string UpdatedBy
        {
            get { return _updatedBy; }
            set { _updatedBy = value; }
        }

        private DateTime? _updateDate;
        public DateTime? UpdateDate
        {
            get { return _updateDate; }
            set { _updateDate = value; }
        }

        private string _createdBy;

        public string CreatedBy
        {
            get { return _createdBy; }
            set { _createdBy = value; }
        }

        private DateTime _createDate;

        public DateTime CreateDate
        {
            get { return _createDate; }
            set { _createDate = value; }
        }

        [NotMapped]
        public string Division
        {
            get { return _sku.Substring(0, 2); }
            set { }
        }

        [NotMapped]
        public bool OPDepartment { get; set; }

        [NotMapped]
        public string Department
        {
            get { return _sku.Substring(3, 2); }
            set { }
        }

        public long? ItemID { get; set; }

        private int _storeCount;

        [DisplayName("# Stores")]
        public int StoreCount
        {
            get { return _storeCount; }
            set { _storeCount = value; }
        }

        public bool Launch { get; set; }

        public bool EvergreenSKU { get; set; }

        public bool LaunchMinihubInd { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? LaunchDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ALRStartDate { get; set; }

        public string ActiveAR { get; set; }

        [NotMapped]
        public bool ActiveOP { get; set; }

        [NotMapped]
        [DefaultValue("No")]
        public string PreSaleSKU { get; set; }

        [NotMapped]
        public string ReInitializeStatus { get; set; }

        public virtual ItemMaster ItemMaster { get; set; }

        /// <summary>
        /// Initializes a new instance of the RangePlan class.
        /// </summary>
        public RangePlan()
        {
            this.Id = 0L;
            this.Sku = String.Empty;
            this.Description = String.Empty;
            this.StartDate = new DateTime?();
            this.EndDate = new DateTime?();
            this.PlanType = String.Empty;
            this.UpdatedBy = String.Empty;
            this.UpdateDate = new DateTime?();
            this.CreatedBy = String.Empty;
            this.CreateDate = DateTime.MinValue;
            this.Division = String.Empty;
            this.Department = String.Empty;
            this.ItemID = new Int64?();
            this.StoreCount = 0;
            this.ItemMaster = null;            
            ActiveOP = false;
        }
    }
}
