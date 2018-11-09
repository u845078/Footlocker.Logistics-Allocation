using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Footlocker.Common.Utilities.File;
using System.Reflection;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceBIExtract : StringLayoutDelimitedUtility
    {
        public override bool IsValid()
        {
            return true;
        }

        [StringLayoutDelimited(0)]
        public Int64 ID { get; set; }
        [StringLayoutDelimited(1)]
        public string Division { get; set; }
        [StringLayoutDelimited(2)]
        public string Store { get; set; }
        [StringLayoutDelimited(3)]
        public string Sku { get; set; }
        [NotMapped]
        public string Department
        {
            get
            {
                return Sku.Substring(3, 2);
            }
        }
        //public Int64 ItemID { get; set; }
        [Display(Name = "Caselot / Size")]
        [StringLayoutDelimited(4)]
        public string Size { get; set; }
        [StringLayoutDelimited(5)]
        public string CaseLot { get; set; }
        [StringLayoutDelimited(5)]
        public int Qty { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [StringLayoutDelimited(6, "yyyy-MM-dd")]
        public DateTime StartDate { get; set; }
        [StringLayoutDelimited(7, "yyyy-MM-dd")]
        public DateTime? EndDate { get; set; }
        [StringLayoutDelimited(8)]
        public string CreatedBy { get; set; }
        [StringLayoutDelimited(9, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime? CreateDate { get; set; }

        public Int64 ItemID { get; set; }
        public virtual ItemMaster ItemMaster { get; set; }

    }
}
