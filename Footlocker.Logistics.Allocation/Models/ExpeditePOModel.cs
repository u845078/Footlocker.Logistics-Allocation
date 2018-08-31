using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Models;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class ExpeditePOModel
    {
        public string CurrentDivision { get; set; }
        public List<Division> Divisions { get; set; }
        public List<ExpeditePO> POs { get; set; }
        public ExpeditePO NewPO { get; set; }

        //public List<ExpeditePO> POsToCreate { get; set; }

        public List<ExistingPO> ExistingPOs { get; set; }
        public string Message { get; set; }
        public List<ExpeditePOHeader> Headers { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal TotalRetail { get; set; }

        public Int32 TotalUnits { get; set; }
    }
}