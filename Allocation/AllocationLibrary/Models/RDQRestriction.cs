using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("RDQRestrictions")]
    public class RDQRestriction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RDQRestrictionID { get; set; }

        private string _division;
        [Required(ErrorMessage = "Division is required")]
        [StringLength(2, ErrorMessage = "Division cannot exceed more than 2 characters")]
        public string Division
        {
            get { return _division; }
            set
            {
                if (value == null)
                {
                    _division = null;
                }
                else
                {
                    _division = value.Trim();
                }
            }
        }

        private string _department;
        [StringLength(2, ErrorMessage = "Department cannot exceed more than 2 characters")]
        public string Department
        {
            get { return _department; }
            set
            {
                if (value == null)
                {
                    _department = null;
                }
                else
                {
                    _department = value.Trim();
                }
            }
            //set { _department = value?.Trim(); }
        }

        private string _category;
        [StringLength(3, ErrorMessage = "Category cannot exceed more than 3 characters")]
        public string Category
        {
            get { return _category; }
            set { _category = value?.Trim(); }
        }

        private string _brand;
        [StringLength(3, ErrorMessage = "Brand cannot exceed more than 3 characters")]
        public string Brand
        {
            get { return _brand; }
            set { _brand = value?.Trim(); }
        }

        private string _vendor;
        [StringLength(5, ErrorMessage = "Vendor cannot exceed more than 5 characters")]
        public string Vendor
        {
            get { return _vendor; }
            set { _vendor = value?.Trim(); }
        }

        private string _rdqType;
        [Display(Name = "RDQ Type")]
        public string RDQType
        {
            get { return _rdqType; }
            set { _rdqType = value?.Trim(); }
        }

        [Display(Name = "From Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Required]
        public DateTime FromDate { get; set; }

        [Display(Name = "To Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Required]
        public DateTime ToDate { get; set; }

        private string _fromDCCode;
        [Display(Name = "From DC Code")]
        public string FromDCCode
        {
            get { return _fromDCCode; }
            set { _fromDCCode = value?.Trim(); }
        }

        private string _toLeague;
        [Display(Name = "To League")]
        [StringLength(50, ErrorMessage = "To League should not exceed more than 50 characters")]
        public string ToLeague
        {
            get { return _toLeague; }
            set { _toLeague = value?.Trim(); }
        }

        private string _toRegion;
        [Display(Name = "To Region")]
        [StringLength(50, ErrorMessage = "To Region should not exceed more than 50 characters")]
        public string ToRegion
        {
            get { return _toRegion; }
            set { _toRegion = value?.Trim(); }
        }

        private string _toStore;
        [Display(Name = "To Store")]
        [StringLength(5, ErrorMessage = "The store cannot exceed more than 5 characters")]
        public string ToStore
        {
            get { return _toStore; }
            set { _toStore = value?.Trim(); }
        }

        private string _toDCCode;
        [Display(Name = "To DC Code")]
        public string ToDCCode
        {
            get { return _toDCCode; }
            set { _toDCCode = value?.Trim(); }
        }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime LastModifiedDate { get; set; }

        public string LastModifiedUser { get; set; }

        public RDQRestriction()
            : base()
        {
            this.Division = string.Empty;
            this.Department = string.Empty;
            this.Category = string.Empty;
            this.Brand = string.Empty;
            this.Vendor = string.Empty;
            this.RDQType = string.Empty;
            this.FromDCCode = string.Empty;
            this.ToLeague = string.Empty;
            this.ToRegion = string.Empty;
            this.ToStore = string.Empty;
            this.ToDCCode = string.Empty;
        }

        public RDQRestriction(string division, string department, string category, string brand)
            : this()
        {
            this.Division = division;
            this.Department = department;
            this.Category = category;
            this.Brand = brand;
        }

        public RDQRestriction(string division, string department, string category, string brand, string vendor, string rdqType, DateTime fromDate,
                                DateTime toDate, string fromDCCode, string toLeague, string toRegion, string toStore, string toDCCode)
            : this()
        {
            this.Division = division;
            this.Department = department;
            this.Category = category;
            this.Brand = brand;
            this.Vendor = vendor;
            this.RDQType = rdqType;
            this.FromDate = fromDate;
            this.ToDate = toDate;
            this.FromDCCode = fromDCCode;
            this.ToLeague = toLeague;
            this.ToRegion = toRegion;
            this.ToStore = toStore;
            this.ToDCCode = toDCCode;
        }
    }
}
