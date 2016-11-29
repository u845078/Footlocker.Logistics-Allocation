using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceUploadModel
    {
        public string Division {get;set;}
        public string Store {get;set;}
        public string SKU {get;set;}
        public string EndDate {get;set;}
        public string PO {get;set;}
        public string Warehouse {get;set;}
        public string Size {get;set;}
        public string Qty {get;set;}
        public string Comments {get;set;}
        public string ErrorMessage { get; set; }

        public RingFenceUploadModel()
        { }

    }
}