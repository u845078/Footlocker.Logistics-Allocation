using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("POCrossdockData")]
    public class POCrossdockData
    {
        [Key]
        [Column(Order = 0)]
        public int InstanceID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 2)]
        public string PO { get; set; }
        public DateTime? ExpectedReceiptDate { get; set; }

        [NotMapped]
        public string ExpectedReceiptDateString { get; set; }
        public bool CancelInd { get; set; }
        
        [NotMapped]
        public string CancelIndString { get; set; }

        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedUser { get; set; }

        [NotMapped]
        public string ErrorMessage { get; set; }
    }
}
