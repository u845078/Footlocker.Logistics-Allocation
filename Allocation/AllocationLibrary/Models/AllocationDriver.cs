using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class AllocationDriver
    {
        [Key, Column(Order=0)]
        public string Division { get; set; }
        [Key, Column(Order=1)]
        public string Department { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ConvertDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AllocateDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime OrderPlanningDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public Boolean CheckNormals { get; set; }
    }
}
