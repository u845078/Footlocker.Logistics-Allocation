using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ExpeditePOs")]
    public class ExpeditePO 
    {      
        [Key]
        [Column(Order=0)]
        [StringLayoutDelimited(0)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLayoutDelimited(1)]
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
        [StringLayoutDelimited(3, "yyyy-MM-dd")]
        public DateTime OverrideDate { get; set; }

        [StringLayoutDelimited(4)]
        public string CreatedBy { get; set; }
        [StringLayoutDelimited(5, "yyyy-MM-dd h:mm:ss tt")]
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

        /// <summary>
        /// Initializes a new instance of the ExpeditePO class.
        /// </summary>
        /// <param name="division">The initial value for the division property.</param>
        /// <param name="po">The initial value for the purchase order property.</param>
        /// <param name="deliveryDate">The initial value for the delivery date property.</param>
        /// <param name="overrideDate">The initial value for the override date property.</param>
        /// <param name="createdBy">The initial value for the created by property.</param>
        /// <param name="createDate">The initial value for the create date property.</param>
        /// <param name="departments">The initial value for the departments property.</param>
        /// <param name="sku">The initial value for the stock keeping unit property.</param>
        public ExpeditePO(string division, string po, DateTime deliveryDate, DateTime overrideDate, string createdBy
                , DateTime createDate, string departments, string sku, decimal totalRetail, int totalUnits)
            : this()
        {
            this.Division = division;
            this.PO = po;
            this.DeliveryDate = deliveryDate;
            this.OverrideDate = overrideDate;
            this.CreatedBy = createdBy;
            this.CreateDate = createDate;
            this.Departments = departments;
            this.Sku = sku;
            this.TotalRetail = totalRetail;
            this.TotalUnits = totalUnits;
        }
    }
}
