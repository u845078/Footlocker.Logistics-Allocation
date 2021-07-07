using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DistributionCenter
    {
        [Key]
        [Column("ID")]
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

        [NotMapped]
        public string LastModifiedUserName { get; set; }

        [Display(Name="Last Modified Date")]
        public DateTime LastModifiedDate { get; set; }

        [Column("MinihubInd")]
        public bool IsMinihub { get; set; }

        [Display(Name = "Ecom Fulfillment Center")]
        [Column("FulfillmentCenterInd")]
        public bool IsFulfillmentCenter { get; set; }

        [Column("WarehouseAllocationType")]        
        [ForeignKey("WarehouseAllocationType")]
        [Display(Name = "Warehouse Allocation Type")]
        public int WarehouseAllocationTypeCode { get; set; }

        public virtual WarehouseAllocationType WarehouseAllocationType { get; set; }
        //public virtual ICollection<EcomCustomerFulfillmentXref> ECOMCustomerXrefs { get; set; }


        [Column("DistributionCenterRestriction")]
        [ForeignKey("DistributionCenterRestriction")]
        [Display(Name="Distribution Center Restriction")]
        public int DistrictionCenterRestrictionCode { get; set; }

        public virtual DistributionCenterRestrictions DistributionCenterRestriction { get; set; }

        [Column("TransmitRDQsToKafkaInd")]
        [Display(Name="Transmit RDQs To Kafka")]
        public bool TransmitRDQsToKafka { get; set; }

        public string displayValue
        {
            get
            {
                return MFCode + " - " + Name;
            }
        }
    }
}
