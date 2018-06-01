using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class WarehouseBlackout
    {
        public int ID { get; set; }
        public int DCID { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
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
                    _dc = (from a in context.DistributionCenters where a.ID == this.DCID select a).FirstOrDefault();
                }

                return _dc; 
            }

            set { _dc = value; }
        }
    }
}
