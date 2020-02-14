using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("PreSaleSKU")]
    public class PreSaleSKU
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PreSaleSkuID { get; set; }
        public long ItemID { get; set; }
        public DateTime InventoryArrivalDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedUser { get; set; }
    }
}
