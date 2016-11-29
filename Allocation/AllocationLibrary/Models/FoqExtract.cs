using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    class FoqExtract : BiExtract
    {
        public override bool IsValid()
        {
            return true;
        }

        [StringLayoutDelimited(0)]
        public int PullId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [StringLayoutDelimited(1, "yyyy-MM-dd")]
        public DateTime SessionDate { get; set; }

        [StringLayoutDelimited(2)]
        public int RangeSessionId { get; set; }

        [StringLayoutDelimited(3)]
        public Decimal RawQty { get; set; }

        [StringLayoutDelimited(4)]
        public Decimal DCQty { get; set; }

        [StringLayoutDelimited(5)]
        public Decimal Ratio { get; set; }

        [StringLayoutDelimited(6)]
        public Decimal QtyAdjustment { get; set; }

        [StringLayoutDelimited(7)]
        public Decimal FinalQty { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [StringLayoutDelimited(8, "yyyy-MM-dd")]
        public DateTime WeekStartDate { get; set; }

        [StringLayoutDelimited(9)]
        public Int64 ItemId { get; set; }

        [StringLayoutDelimited(10)]
        public String MerchantSku { get; set; }

        [StringLayoutDelimited(11)]
        public String Size { get; set; }

        [StringLayoutDelimited(12)]
        public String Division { get; set; }

        [StringLayoutDelimited(13)]
        public String Store { get; set; }
    }
}
