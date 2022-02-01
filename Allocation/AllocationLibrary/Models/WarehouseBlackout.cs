using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class WarehouseBlackout
    {
        public int ID { get; set; }
        
        [Display(Name ="Distribution Center")]
        public int DCID { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        private DistributionCenter _dc;

        [NotMapped]
        public DistributionCenter DC
        {
            get 
            {
                if (_dc == null)
                {
                    Footlocker.Logistics.Allocation.Services.AllocationLibraryContext context = new Footlocker.Logistics.Allocation.Services.AllocationLibraryContext();
                    _dc = context.DistributionCenters.Where(dc => dc.ID == DCID).FirstOrDefault();
                }

                return _dc; 
            }

            set { _dc = value; }
        }
    }
}
