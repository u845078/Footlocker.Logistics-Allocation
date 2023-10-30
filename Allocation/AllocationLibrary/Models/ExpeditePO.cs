using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ExpeditePOs")]
    public class ExpeditePO 
    {      
        [Key]
        [Column(Order=0)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 1)]
        public string PO { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "ExpectedDeliveryDate")]
        [NotMapped]
        public DateTime? DeliveryDate { get; set; }

        [DataType(DataType.Date)]
        [Column("DeliveryDate")]
        public DateTime? StoredDeliveryDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime OverrideDate { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        
        public string Departments { get; set; }
        
        public string Sku { get; set; }

        [NotMapped]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public Decimal TotalRetail { get; set; }

        [NotMapped]
        public int TotalUnits { get; set; }

        [NotMapped]
        public string ErrorMessage { get; set; }
        /// <summary>
        /// Initializes a new instance of the ExpeditePO class.
        /// </summary>
        public ExpeditePO()
        {
            this.Division = String.Empty;
            this.PO = String.Empty;
            this.DeliveryDate = DateTime.MinValue;
            this.OverrideDate = DateTime.MinValue;
            this.CreatedBy = String.Empty;
            this.CreateDate = DateTime.MinValue;
            this.Departments = String.Empty;
            this.Sku = String.Empty;
            this.TotalRetail = Decimal.Zero;
            this.TotalUnits = 0;
        }
    }
}
