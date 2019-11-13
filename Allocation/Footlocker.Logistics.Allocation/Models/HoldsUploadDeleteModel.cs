using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class HoldsUploadDeleteModel
    {
        public string Store;
        public string Division;

        private string _level;

        public string Level
        {
            get
            {
                if (_level == null)
                {
                    _level = "";
                }
                return _level;
            }

            set
            {
                _level = value;
            }
        }

        public string Value { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Duration { get; set; }
        public string HoldType { get; set; }
        public string Comments { get; set; }

        public string ErrorMessage { get; set; }
    }
}