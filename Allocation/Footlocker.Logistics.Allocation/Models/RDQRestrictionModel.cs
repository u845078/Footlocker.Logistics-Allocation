using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RDQRestrictionModel
    {
        public RDQRestriction RDQRestriction { get; set; }

        public List<SelectListItem> Divisions { get; set; }

        public List<SelectListItem> Departments { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public List<SelectListItem> Brands { get; set; }

        public List<SelectListItem> DistributionCenters { get; set; }

        public List<SelectListItem> Vendors { get; set; }

        public List<SelectListItem> RDQTypes { get; set; }

        public List<StoreLookup> SelectedStores { get; set; }

        public List<RDQRestriction> RDQRestrictionDetails { get; set; }

        public bool CanEdit { get; set; }

        public RDQRestrictionModel()
        {
            RDQRestriction = new RDQRestriction();
        }

        public RDQRestrictionModel(RDQRestriction rr)
        {
            this.RDQRestriction = rr;
        }
    }
}