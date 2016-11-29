using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class NodeItem : StringLayoutDelimitedUtility
    {
        public override bool IsValid()
        {
            return true;
        }

        [StringLayoutDelimited(0)]
        public string NodeType { get; set; }
        [StringLayoutDelimited(1)]
        public string NodeID { get; set; }
        [StringLayoutDelimited(2)]
        public string NodeDesc { get; set; }
        [StringLayoutDelimited(3)]
        public string DisplayString { get; set; }
        [StringLayoutDelimited(4)]
        public string NodeTreeString { get; set; }
        [StringLayoutDelimited(5)]
        public string StrAttribute1 { get; set; }
        [StringLayoutDelimited(6)]
        public string StrAttribute2 { get; set; }
        [StringLayoutDelimited(7)]
        public string StrAttribute3 { get; set; }
        [StringLayoutDelimited(8)]
        public string StrAttribute4 { get; set; }
        [StringLayoutDelimited(9)]
        public string StrAttribute5 { get; set; }
        [StringLayoutDelimited(10)]
        public string NumAttribute1 { get; set; }
        [StringLayoutDelimited(11)]
        public string NumAttribute2 { get; set; }
        [StringLayoutDelimited(12)]
        public string NumAttribute3 { get; set; }
        [StringLayoutDelimited(13)]
        public string NumAttribute4 { get; set; }
        [StringLayoutDelimited(14)]
        public string NumAttribute5 { get; set; }
        [StringLayoutDelimited(15)]
        public string DateAttribute1 { get; set; }
        [StringLayoutDelimited(16)]
        public string DateAttribute2 { get; set; }
        [StringLayoutDelimited(17)]
        public string DateAttribute3 { get; set; }

        public string ToStringWithQuotesFast(char delimiter)
        {
            string line = "\"" + NodeType + "\"" + delimiter;
            line = line + "\"" + NodeID + "\"" + delimiter;
            if (NodeDesc != null)
            {
                line = line + "\"" + NodeDesc + "\"" + delimiter;
            }
            else
            {
                line = line + delimiter;
            }
            line = line + "\"" + DisplayString + "\"" + delimiter;
            for (int i = 0; i < 14; i++)
            {
                line = line + delimiter;
            }

            return line;
        }
    }
}
