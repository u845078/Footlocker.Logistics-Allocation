using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ApplicationUser")]
    public class ApplicationUser
    {
        [Key, Column("ID")]
        public int ID { get; set; }

        public int ApplicationID { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
