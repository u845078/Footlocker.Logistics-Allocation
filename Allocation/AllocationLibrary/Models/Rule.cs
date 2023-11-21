using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class Rule
    {
        public long ID { get; set; }
        public string Field { get; set; }
        public string Compare { get; set; }
        public string Value { get; set; }
        public long RuleSetID { get; set; }
        public int Sort { get; set; }

        // HACK: Fields to be used by rules should probably be stored in a table by rule context, and with a display text field...
        [NotMapped]
        public string DisplayField
        {
            get
            {
                return (!string.IsNullOrWhiteSpace(Field) && Field.Contains(".")) ?
                    Field.Substring(Field.IndexOf(".") + 1, (Field.LastIndexOf(".") - (Field.IndexOf(".") + 1))).Replace("Type", string.Empty) :
                    Field ?? string.Empty;
            }
        }
    }
}