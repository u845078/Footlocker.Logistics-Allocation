using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Display(Name = "Conversion Date")]
        public DateTime ConvertDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Allocate Date")]
        public DateTime AllocateDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime OrderPlanningDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public bool CheckNormals { get; set; }

        [Column("MinihubInd")]
        [Display(Name ="Stock In Minihub?")]
        public bool StockedInMinihub { get; set; }
    }
}
