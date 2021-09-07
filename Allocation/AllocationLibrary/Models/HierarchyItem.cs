using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class HierarchyItem : StringLayoutDelimitedUtility
    {
        public override bool IsValid()
        {
            return true;
        }

        [StringLayoutDelimited(0)]
        public string HierarchyID { get; set; }
        //[StringLayoutDelimited(1)]
        //public string HierarchyLevel { get; set; }
        [StringLayoutDelimited(1)]
        public string FromNodeType { get; set; }
        [StringLayoutDelimited(2)]
        public string FromNodeID { get; set; }
        [StringLayoutDelimited(3)]
        public string ToNodeType { get; set; }
        [StringLayoutDelimited(4)]
        public string ToNodeID { get; set; }
        [StringLayoutDelimited(5)]
        public string Quantity { get; set; }
        [StringLayoutDelimited(6)]
        public string StrAttribute1 { get; set; }
        [StringLayoutDelimited(7)]
        public string StrAttribute2 { get; set; }
        [StringLayoutDelimited(8)]
        public string StrAttribute3 { get; set; }
        [StringLayoutDelimited(9)]
        public string StrAttribute4 { get; set; }
        [StringLayoutDelimited(10)]
        public string StrAttribute5 { get; set; }
        [StringLayoutDelimited(11)]
        public string NumAttribute1 { get; set; }
        [StringLayoutDelimited(12)]
        public string NumAttribute2 { get; set; }
        [StringLayoutDelimited(13)]
        public string NumAttribute3 { get; set; }
        [StringLayoutDelimited(14)]
        public string NumAttribute4 { get; set; }
        [StringLayoutDelimited(15)]
        public string NumAttribute5 { get; set; }
        [StringLayoutDelimited(16)]
        public string DateAttribute1 { get; set; }
        [StringLayoutDelimited(17)]
        public string DateAttribute2 { get; set; }
        [StringLayoutDelimited(18)]
        public string DateAttribute3 { get; set; }

        public string ToStringWithQuotesFast(char delimiter)
        {
            string line = "\"" + HierarchyID + "\"" + delimiter;
            line = line + "\"" + FromNodeType + "\"" + delimiter;
            line = line + "\"" + FromNodeID + "\"" + delimiter;
            line = line + "\"" + ToNodeType + "\"" + delimiter;
            line = line + "\"" + ToNodeID + "\"" + delimiter;
            line = line + Quantity + delimiter;
            for (int i = 0; i < 13; i++)
            {
                line = line + delimiter;
            }

            return line;
        }
    }
}
