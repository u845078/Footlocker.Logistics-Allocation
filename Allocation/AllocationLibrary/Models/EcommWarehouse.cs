using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("EcommWarehouses")]
    public class EcommWarehouse
    {
        [Key]
        [Column(Order = 0)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 1)]
        public string Store { get; set; }

        [NotMapped]
        public string KeyDisplay
        {
            get
            {
                return string.Format("{0}-{1}", Division, Store);
            }
        }

        public string EcomGroup { get; set; }

        public string Name { get; set; }

        public int StorageDCID { get; set; }

        [ForeignKey("StorageDCID")]
        public virtual DistributionCenter StorageDistributionCenter { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
