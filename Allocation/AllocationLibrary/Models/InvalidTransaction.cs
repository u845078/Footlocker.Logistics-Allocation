using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("InvalidTransaction")]
    public class InvalidTransaction
    {
        public long ID { get; set; }
        public int SessionID { get; set; }
        public int RuleID { get; set; }
        public long ItemID { get; set; }

        public string LocationDiv { get; set; }
        public string LocationStore { get; set; }

        public int TotalQty { get; set; }
        public decimal TotalRetail { get; set; }

        public DateTime TransDt { get; set; }

        public string ProductTypeCode { get; set; }
        public string ProductInvID { get; set; }
        public string LocationType { get; set; }
        public string LocationID { get; set; }
        public string TransactionDate { get; set; }
        public string TransPostDate { get; set; }
        public string RSUnitQty { get; set; }
        public string RSGrossCost { get; set; }
        public string RSGrossRetail { get; set; }
        public string RSGrossWholesale { get; set; }
        public string RTUnitQty { get; set; }
        public string RTGrossCost { get; set; }
        public string RTGrossRetail { get; set; }
        public string RTGrossWholesale { get; set; }
        public string PSUnitQty { get; set; }
        public string PSGrossCost { get; set; }
        public string PSGrossRetail { get; set; }
        public string PSGrossWholesale { get; set; }
        public string PTUnitQty { get; set; }
        public string PTGrossCost { get; set; }
        public string PTGrossRetail { get; set; }
        public string PTGrossWholesale { get; set; }
        public string CSUnitQty { get; set; }
        public string CSGrossCost { get; set; }
        public string CSGrossRetail { get; set; }
        public string CSGrossWholesale { get; set; }
        public string CTUnitQty { get; set; }
        public string CTGrossCost { get; set; }
        public string CTGrossRetail { get; set; }
        public string CTGrossWholesale { get; set; }

        [ForeignKey("SessionID")]
        public ValidationSession Session { get; set; }

        [ForeignKey("ItemID")]
        public ItemMaster Item { get; set; }

        /// <summary>
        /// FK Relationship defined in context (as complex)
        /// </summary>
        public StoreLookup Location { get; set; }
    }
}
