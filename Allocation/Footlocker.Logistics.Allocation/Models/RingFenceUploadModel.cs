using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceUploadModel
    {
        private string _store;
        private string _division;
        private string _po;

        public string Division
        {
            get
            {
                return _division;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _division = value.PadLeft(2, '0');
                }
                else
                    _division = value;
            }
        }

        public string Store
        {
            get
            {
                return _store;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _store = value.PadLeft(5, '0');
                }
                else
                    _store = value;
            }
        }

        public string PO
        {
            get
            {
                if (_po == null)
                    return "";
                else
                    return _po;
            }
            set
            {
                if (value == null)
                    _po = "";
                else
                    _po = value;
            }
        }

        public string SKU { get; set; }
        public string EndDate { get; set; }
        public string Warehouse { get; set; }
        public string Size { get; set; }
        public string Qty { get; set; }
        public int Quantity { get; set; }
        public string Comments { get; set; }
        public string ErrorMessage { get; set; }
    }
}