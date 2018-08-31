using System;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SecuredDivisionModel
    {
        public bool IsUserWithAccess { get; set; }
        public string DivCode { get; set; }
        public string DivisionName { get; set; }
    }
}