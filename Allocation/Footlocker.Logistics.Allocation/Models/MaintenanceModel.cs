using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public enum MaintenanceDataBases
    {
        Allocation, Footlocker_Common
    }

    public class MaintenanceModel
    {
        public string SQLCommand { get; set; }
        public string ReturnMessage { get; set; }

        [Display(Name = "Target Database")]
        public MaintenanceDataBases SelectedDatabase { get; set; }
    }
}