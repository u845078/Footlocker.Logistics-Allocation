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

        [Required(ErrorMessage = "Division is required")]
        [StringLength(2, ErrorMessage = "Division cannot exceed more than 2 characters")]
        public string Division { get; set; }

        [StringLength(5, ErrorMessage = "Vendor cannot exceed more than 5 characters")]
        public string Vendor { get; set; }

        [StringLength(2, ErrorMessage = "Department cannot exceed more than 2 characters")]
        public string Department { get; set; }

        [StringLength(3, ErrorMessage = "Category cannot exceed more than 3 characters")]
        public string Category { get; set; }

        [StringLength(3, ErrorMessage = "Brand cannot exceed more than 3 characters")]
        public string Brand { get; set; }

        [Display(Name = "RDQ Type")]
        public string RDQType { get; set; }

        [Display(Name = "From Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Required]
        public DateTime FromDate { get; set; }

        [Display(Name = "To Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Required]
        public DateTime ToDate { get; set; }

        [Display(Name = "From DC Code")]
        [StringLength(2, ErrorMessage = "From DC Code cannot exceed more than 2 characters")]
        public string FromDCCode { get; set; }

        [Display(Name = "To League")]
        [StringLength(50, ErrorMessage = "To League should not exceed more than 50 characters")]
        public string ToLeague { get; set; }

        [Display(Name = "To Region")]
        [StringLength(50, ErrorMessage = "To Region should not exceed more than 50 characters")]
        public string ToRegion { get; set; }

        [Display(Name = "To Store")]
        [StringLength(5, ErrorMessage = "The store cannot exceed more than 5 characters")]
        public string ToStore { get; set; }

        [Display(Name = "To DC Code")]
        [StringLength(2, ErrorMessage = "To DC Code cannot exceed more than 2 characters")]
        public string ToDCCode { get; set; }

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

        //public override bool Equals(object obj)
        //{
        //    var item = obj as RDQRestriction;

        //    if (obj == null)
        //    {
        //        return false;
        //    }
            
        //    // division is required and will not be null
        //    if (!this.Division.Equals(item.Division))
        //    {
        //        return false;
        //    }

        //    var equal = ((this.Department == null && item.Department == null) || this.Department == item.Department) &&
        //                 ((this.Category == null && item.Category == null) || this.Category == item.Category) &&
        //                 ((this.Brand == null && item.Brand == null) || this.Brand == item.Brand) &&
        //                 ((this.))
        //}

        //public override int GetHashCode()
        //{
        //    return this.RDQRestrictionID.GetHashCode();
        //}
    }
}
