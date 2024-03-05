using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("DataChangeLog")]
    public class DataChangeLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ChangeID { get; set; }
        public string CommandText { get; set; }
        public string UsedDatabase { get; set; }
        public bool ExecutedInd { get; set; }
        public long ChangedRows { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedUser { get; set; }
    }
}
