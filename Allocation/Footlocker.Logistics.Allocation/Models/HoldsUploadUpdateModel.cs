using System;

namespace Footlocker.Logistics.Allocation.Models
{
    public class HoldsUploadUpdateModel
    {
        public string Store;
        public string Division;

        private string _level;
        private string _duration;
        private string _holdType;

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
        public string Duration 
        {
            get
            {
                if (_duration == "permanent")
                    return "Permanent";
                else if (_duration == "temporary")
                    return "Temporary";
                else
                    return _duration;
            }
            set 
            {
                _duration = value.ToLower();
            } 
        }
        public string HoldType 
        { 
            get
            {
                if (_holdType == "cancel inventory")
                    return "Cancel Inventory";
                else if (_holdType == "reserve inventory")
                    return "Reserve Inventory";
                else
                    return _holdType;
            }
            set
            {
                _holdType = value.ToLower();
            } 
        }

        public string Comments { get; set; }

        public string ErrorMessage { get; set; }
    }
}