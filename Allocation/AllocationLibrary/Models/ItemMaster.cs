using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ItemMaster")]
    public class ItemMaster
    {
        public Int32 InstanceID { get; set; }
        [Key]
        public Int64 ID { get; set; }
        public string CurrentSku { get; set; }

        [Display(Name = "SKU")]
        public string MerchantSku { get; set; }
        public string LifeCycle { get; set; }
        public DateTime? FirstReceipt { get; set; }

        [Display(Name = "Service Type")]
        public string ServiceCode { get; set; }
        public string WarehouseReceived { get; set; }
        public string TargetSku { get; set; }
        public DateTime? RenumberDate { get; set; }

        [Display(Name = "Team Code")]
        public string TeamCode { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string Vendor { get; set; }
        public string Div { get; set; }
        public string Dept { get; set; }
        public string Skuid1 { get; set; }
        public string Skuid2 { get; set; }
        public string Skuid3 { get; set; }
        public string Skuid4 { get; set; }
        public string Skuid5 { get; set; }
        public string Color1 { get; set; }
        public string Color1Desc { get; set; }
        public string Color2 { get; set; }
        public string Color2Desc { get; set; }
        public string Color3 { get; set; }
        public string Color3Desc { get; set; }
        public string Gender { get; set; }
        public string Material { get; set; }
        public string MaterialDesc { get; set; }
        [Display(Name = "Size Range")]
        public string SizeRange { get; set; }
        public string Exclusive { get; set; }
        public string Description { get; set; }
        public Int32? LifeCycleDays { get; set; }
        public DateTime? CreateDate { get; set; }
        public Int32? Renumbers { get; set; }
        public string PlayerID { get; set; }
        public Int16? Deleted { get; set; }

        [Display(Name = "Merchandise Season Code")]
        public string MerchandiseSeasonCode { get; set; }
        //public virtual ICollection<RangePlan> RangePlans { get; set; }
        //public virtual ICollection<DirectToStoreSku> DirectToStoreSkus { get; set; }
    }
}
