using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class Rule
    {
        public Int64 ID { get; set; }
        public string Field { get; set; }
        public string Compare { get; set; }
        public string Value { get; set; }
        public Int64 RuleSetID { get; set; }
        public int Sort { get; set; }

        // HACK: Fields to be used by rules should probably be stored in a table by rule context, and with a display text field...
        [NotMapped]
        public string DisplayField
        {
            get
            {
                return (!String.IsNullOrWhiteSpace(Field) && Field.Contains(".")) ?
                    Field.Substring(Field.IndexOf(".") + 1, (Field.LastIndexOf(".") - (Field.IndexOf(".") + 1))).Replace("Type", String.Empty) :
                    Field ?? String.Empty;
            }
        }
    }
}