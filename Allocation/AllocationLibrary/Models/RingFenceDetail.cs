using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceDetail
    {
        public RingFenceDetail()
        {
            ActiveInd = "1";
            PackDetails = new List<ItemPackDetail>();
        }

        [Key]
        [Column(Order=0)]
        public Int64 RingFenceID { get; set; }
        
        [Key]
        [Column(Order = 1)]
        public int DCID { get; set; }

        [NotMapped]
        public string Warehouse { get; set; }

        private string _size;

        [Key]
        [Column(Order = 2)]
        public string Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value.Trim();
            }
        }

        private string _PO;
        [Key]
        [Column(Order = 3)]
        public string PO 
        {
            get
            {
                return _PO;
                //to avoid the "N/A" vs "" logic everywhere, return "" for N/A.
                //if ((_PO != null)&&(_PO != ""))
                //    return _PO;
                //return "N/A";
            }
            set 
            {
                if (value == null)
                    _PO = "";
                else
                    _PO = value; 
            }
        }

        [NotMapped]
        public string PriorityCode { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Must be > 0")]
        public int Qty { get; set; }

        [NotMapped]
        public int Units { get; set; }

        [NotMapped]
        public int AvailableQty { get; set; }

        [NotMapped]
        public int AssignedQty { get; set; }

        [NotMapped]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DueIn { get; set; }
        
        [NotMapped]
        public string Message { get; set; }

        [NotMapped]
        public List<ItemPackDetail> PackDetails { get; set; }

        public string ActiveInd { get; set; }

        public string ringFenceStatusCode { get; set; }

        [NotMapped]
        public RingFenceStatusCodes ringFenceStatus { get; set; }
    }
}
