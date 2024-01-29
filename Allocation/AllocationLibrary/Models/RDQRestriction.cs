using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("RDQRestrictions")]
    public class RDQRestriction
    {
        private string _division;
        private string _department;
        private string _category;
        private string _brand;
        private string _vendor;
        private string _rdqType;
        private string _fromDCCode;
        private string _toLeague;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RDQRestrictionID { get; set; }

        [Required(ErrorMessage = "Division is required")]
        [StringLength(2, ErrorMessage = "Division cannot exceed more than 2 characters")]
        public string Division
        {
            get { return _division; }
            set
            {
                if (value == null)                
                    _division = null;                
                else                
                    _division = value.Trim();                
            }
        }
        
        public string Department
        {
            get { return _department; }
            set
            {
                if (value == null)                
                    _department = null;                
                else                
                    _department = value.Trim();                
            }
        }
        
        [StringLength(3, ErrorMessage = "Category cannot exceed more than 3 characters")]
        public string Category
        {
            get { return _category; }
            set
            {
                if (value == null)                
                    _category = null;                
                else                
                    _category = value.Trim();                
            }
        }
        
        [StringLength(3, ErrorMessage = "Brand cannot exceed more than 3 characters")]
        public string Brand
        {
            get { return _brand; }
            set
            {
                if (value == null)                
                    _brand = null;                
                else                
                    _brand = value.Trim();                
            }
        }
        
        [StringLength(5, ErrorMessage = "Vendor cannot exceed more than 5 characters")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Vendor must be in ##### format with leading zeros")]
        public string Vendor
        {
            get { return _vendor; }
            set
            {
                if (value == null)                
                    _vendor = null;                
                else                
                    _vendor = value.Trim();                
            }
        }

        private string _sku;
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in ##-##-#####-## format")]
        public string SKU
        {
            get { return _sku; }
            set 
            {
                if (!string.IsNullOrEmpty(value))
                    _sku = value.Trim();
                else
                    _sku = string.Empty;
            }
        }
        
        [Display(Name = "RDQ Type")]
        public string RDQType
        {
            get { return _rdqType; }
            set
            {
                if (value == null)                
                    _rdqType = null;                
                else                
                    _rdqType = value.Trim();                
            }
        }

        [Display(Name = "From Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Required]
        public DateTime FromDate { get; set; }

        [Display(Name = "To Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Required]
        public DateTime ToDate { get; set; }
        
        [Display(Name = "From DC Code")]
        public string FromDCCode
        {
            get { return _fromDCCode; }
            set
            {
                if (value == null)               
                    _fromDCCode = null;                
                else                
                    _fromDCCode = value.Trim();                
            }
        }
        
        [Display(Name = "To League")]
        [StringLength(50, ErrorMessage = "To League should not exceed more than 50 characters")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "League must be in ### format")]
        public string ToLeague
        {
            get { return _toLeague; }
            set
            {
                if (value == null)                
                    _toLeague = null;                
                else                
                    _toLeague = value.Trim();                
            }
        }

        private string _toRegion;
        [Display(Name = "To Region")]
        [StringLength(50, ErrorMessage = "To Region should not exceed more than 50 characters")]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Region must be in ## format")]
        public string ToRegion
        {
            get { return _toRegion; }
            set
            {
                if (value == null)
                {
                    _toRegion = null;
                }
                else
                {
                    _toRegion = value.Trim();
                }
            }
        }

        private string _toStore;
        [Display(Name = "To Store")]
        [StringLength(5, ErrorMessage = "The store cannot exceed more than 5 characters")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Store must be in ##### format with leading zeros")]
        public string ToStore
        {
            get { return _toStore; }
            set
            {
                if (value == null)
                {
                    _toStore = null;
                }
                else
                {
                    _toStore = value.Trim();
                }
            }
        }

        private string _toDCCode;
        [Display(Name = "To DC Code")]
        public string ToDCCode
        {
            get { return _toDCCode; }
            set
            {
                if (value == null)
                {
                    _toDCCode = null;
                }
                else
                {
                    _toDCCode = value.Trim();
                }
            }
        }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime LastModifiedDate { get; set; }

        public string LastModifiedUser { get; set; }

        public RDQRestriction()
        {
            Division = string.Empty;
            Department = string.Empty;
            Category = string.Empty;
            Brand = string.Empty;
            Vendor = string.Empty;
            RDQType = string.Empty;
            FromDCCode = string.Empty;
            ToLeague = string.Empty;
            ToRegion = string.Empty;
            ToStore = string.Empty;
            ToDCCode = string.Empty;
        }
    }
}
