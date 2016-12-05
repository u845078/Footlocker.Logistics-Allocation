using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("Departments")]
    public class Departments
    {
        [Key, Column("InstanceID", Order = 0)]
        public int instanceID { get; set; }

        [Key, Column("Div", Order = 1)]
        public string divisionCode { get; set; }

        [Key, Column("Dept", Order = 2)]
        public string departmentCode { get; set; }

        [Column("Description")]
        public string departmentName { get; set; }

        [NotMapped]
        public string departmentDisplay
        {
            get
            {
                return departmentCode + " - " + departmentName;
            }
        }
    }
}
