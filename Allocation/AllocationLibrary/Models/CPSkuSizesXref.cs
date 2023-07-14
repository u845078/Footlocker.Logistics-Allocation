using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vCPSkuSizesXRef")]
    public class CPSkuSizesXref
    {
        [Key, Column("SkuNumber", Order = 0)]
        public long SKUNumber { get; set; }
        public long CPID { get; set; }
        public long ModelID { get; set; }
        public string VendorStyle { get; set; }
        public string SizeDesc { get; set; }
        
        [Key, Column("SizeID", Order = 1)]
        public string SizeID { get; set; }
        public string SizeCode { get; set; }
        public string StyleDesc { get; set; }

        public string Division { get; set; }
        public string Department { get; set; }
        public string CountryCode { get; set; }

        [Key, Column("LegacySku", Order = 2)]
        public string LegacySku { get; set; }
        public string LegacySizeCode { get; set; }
        public string LegacySizeDesc { get; set; }
        public string LegacyBrandID { get; set; }
        public string LegacyBrandDesc { get; set; }
        public string OnlineStatusCode { get; set; }
        public string OnlineStatusDesc { get; set; }
        public bool IsDropship { get; set; }
        public float DropshipMSRP { get; set; }
        public float DropshipCost { get; set; }
        public float DropshipFee { get; set; }
    }
}
