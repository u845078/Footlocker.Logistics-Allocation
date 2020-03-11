using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ReInitializeSKU")]
    public class ReInitializeSKU
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReInitializeSkuID { get; set; }
        public long ItemID { get; set; }
        public bool SkuExtracted { get; set; }
        [NotMapped]
        public string SkuStatus
        {
            get
            {
                if (SkuExtracted)
                { return "Extracted"; }
                else
                { return "Pending"; }
            }
            set { SkuStatus = value; }
        }
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedUser { get; set; }
    }
}
