using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DistributionCenter
    {
        public int ID { get; set; }
        [Required]
        [Display(Name="DC")]
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int InstanceID { get; set; }
        [Display(Name="Warehouse Code")]
        public string MFCode { get; set; }
        public string Type { get; set; }

        [Display(Name="Last Modified User")]
        public string LastModifiedUser { get; set; }
        [Display(Name = "Last Modified Date")]
        public DateTime LastModifiedDate { get; set; }

        [Column("MinihubInd")]
        public bool IsMinihub { get; set; }

        [Column("WarehouseAllocationType")]        
        [ForeignKey("WarehouseAllocationType")]
        public int WarehouseAllocationTypeCode { get; set; }

        public virtual WarehouseAllocationType WarehouseAllocationType { get; set; }

        public string displayValue
        {
            get
            {
                return MFCode + " - " + Name;
            }
        }
    }
}
