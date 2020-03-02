using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;

using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RangePlan : BiExtract
    {
        public override bool IsValid()
        {
            return true;
        }

        private Int64 _id;

        [StringLayoutDelimited(0)]
        public Int64 Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _sku;

        [StringLayoutDelimited(1)]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        public string Sku
        {
            get { return _sku; }
            set { _sku = value; }
        }

        private string _description;

        [StringLayoutDelimited(2)]
        [Display(Name = "Range Description")]
        [StringLength(50, ErrorMessage = "Max length 50 characters")]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private DateTime? _startDate;

        [StringLayoutDelimited(3, "d")]
        public DateTime? StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        private DateTime? _endDate;

        [StringLayoutDelimited(4, "d")]
        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        private string _planType;

        [NotMapped]
        [StringLayoutDelimited(5)]
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

        [StringLayoutDelimited(6)]
        public string UpdatedBy
        {
            get { return _updatedBy; }
            set { _updatedBy = value; }
        }

        private DateTime? _updateDate;

        [StringLayoutDelimited(7)]
        public DateTime? UpdateDate
        {
            get { return _updateDate; }
            set { _updateDate = value; }
        }

        private string _createdBy;

        [StringLayoutDelimited(8)]
        public string CreatedBy
        {
            get { return _createdBy; }
            set { _createdBy = value; }
        }

        private DateTime _createDate;

        [StringLayoutDelimited(9, "yyyy-MM-dd h:mm:ss tt")]
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

        public Int64? ItemID { get; set; }

        private Int32 _storeCount;

        [DisplayName("# Stores")]
        public Int32 StoreCount
        {
            get { return _storeCount; }
            set { _storeCount = value; }
        }

        public Boolean Launch { get; set; }

        public Boolean EvergreenSKU { get; set; }

        public Boolean LaunchMinihubInd { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? LaunchDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ALRStartDate { get; set; }

        public String ActiveAR { get; set; }

        [NotMapped]
        [DefaultValue("No")]
        public String PreSaleSKU { get; set; }

        [NotMapped]
        public string ReInitializeStatus { get; set; }
        [NotMapped]
        public Boolean ReInitializeSKU { get; set; }

        public virtual ItemMaster ItemMaster { get; set; }

        //public virtual DirectToStoreSku DirectToStoreSku { get; set; }

        //[NotMapped]
        //public string AR 
        //{
        //    get 
        //    {
        //        if (this.DirectToStoreSku != null)
        //            return "yes";
        //        return "no";
        //    }
        //}

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
        }

        /// <summary>
        /// Initializes a new instance of the RangePlan class.
        /// </summary>
        /// <param name="id">The initial value for the identifier property.</param>
        /// <param name="sku">The initial value for the stock keeping unit property.</param>
        /// <param name="description">The initial value for the description property.</param>
        /// <param name="startDate">The initial value for the start date property.</param>
        /// <param name="endDate">The initial value for the end date property.</param>
        /// <param name="planType">The initial value for the plan type property.</param>
        /// <param name="updatedBy">The initial value for the updated by property.</param>
        /// <param name="updateDate">The initial value for the update date property.</param>
        /// <param name="createdBy">The initial value for the created by property.</param>
        /// <param name="createDate">The initial value for the create date property.</param>
        /// <param name="division">The initial value for the division property.</param>
        /// <param name="department">The initial value for the department property.</param>
        /// <param name="itemId">The initial value for the item identifier property.</param>
        /// <param name="storeCount">The initial value for the store count property.</param>
        /// <param name="itemMaster">The initial value for the item master property.</param>
        public RangePlan(Int64 id, string sku, string description, DateTime? startDate, DateTime? endDate
                , string planType, string updatedBy, DateTime? updateDate, string createdBy, DateTime createDate
                , string division, string department, Int64? itemId, Int32 storeCount, ItemMaster itemMaster)
            : this()
        {
            this.Id = id;
            this.Sku = sku;
            this.Description = description;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.PlanType = planType;
            this.UpdatedBy = updatedBy;
            this.UpdateDate = updateDate;
            this.CreatedBy = createdBy;
            this.CreateDate = createDate;
            this.Division = division;
            this.Department = department;
            this.ItemID = itemId;
            this.StoreCount = storeCount;
            this.ItemMaster = itemMaster;
        }
    }
}
