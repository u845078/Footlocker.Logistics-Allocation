using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RDQ
    {
        public RDQ()
        {}

        public RDQ(Int64 id, String division, String store, String sku, String size, int qty, string warehouseName, string status, string createdBy, DateTime? createdate, int? dcid)
        {
            this.ID = id;
            this.Division = division;
            this.Store = store;
            this.Sku = sku;
            this.Size = size;
            this.Qty = qty;
            this.WarehouseName = warehouseName;
            this.Status = status;
            this.CreateDate = createdate;
            this.CreatedBy = createdBy;
            this.DCID = dcid;
        }

        public RDQ(RDQ copyFrom)
        {
            this.ID = copyFrom.ID;
            this.Division = copyFrom.Division;
            this.Store = copyFrom.Store;
            this.Sku = copyFrom.Sku;
            this.Size = copyFrom.Size;
            this.Qty = copyFrom.Qty;
            if (copyFrom.DistributionCenter != null)
            {
                this.WarehouseName = copyFrom.DistributionCenter.Name;
            }
            this.Status = copyFrom.Status;
            this.CreateDate = copyFrom.CreateDate;
            this.CreatedBy = copyFrom.CreatedBy;
            this.DCID = copyFrom.DCID;
            this.ItemID = copyFrom.ItemID;
            this.DestinationType = copyFrom.DestinationType;
            this.ForecastQty = copyFrom.ForecastQty;
            this.NeedQty = copyFrom.NeedQty;
            this.PO = copyFrom.PO;
            this.TargetQty = copyFrom.TargetQty;
            this.Type = copyFrom.Type;
            this.UserRequestedQty = copyFrom.UserRequestedQty;
            this.UnitQty = copyFrom.UnitQty;
        }


        public long ID { get; set; }
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Division must be in the format ##")]
        public string Division { get; set; }

        [Required]
        [RegularExpression(@"^(\d{2}|\d{5})$", ErrorMessage = "Store number must be in the format ##### or ## for warehouses")]
        [Display(Name ="Store or Warehouse Code")]
        public string Store { get; set; }

        [ForeignKey("DistributionCenter")]
        public int? DCID { get; set; }
        [NotMapped]
        [Display(Name="Warehouse")]
        public string WarehouseName { get; set; }
        public string PO { get; set; }
        public long? ItemID { get; set; }
        public string Type { get; set; }
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "Sku must be in the format ##-##-#####-##")]
        public string Sku { get; set; }
        [Display(Name="Size/Caselot")]
        [RegularExpression(@"^\d{3}(?:\d{2})?$", ErrorMessage = "Size must be 3 or 5 digits")]
        public string Size { get; set; }
        public int Qty { get; set; }
        public int? TargetQty { get; set; }
        public int? ForecastQty { get; set; }
        public int? NeedQty { get; set; }
        [NotMapped]
        public int UserRequestedQty 
        {
            get 
            {
                if (_userRequestedQty != null)
                {
                    return Convert.ToInt32(_userRequestedQty);
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                try 
                {
                    _userRequestedQty = Convert.ToString(value);
                }
                catch { }
            }
        }
        private string _userRequestedQty;
        [Column("UserRequestedQty")]
        public string UserRequestedQtyString 
        {
            get
            {
                return _userRequestedQty;
            }
            set
            {
                _userRequestedQty = value;
            }        
        }
        public string CreatedBy { get; set; }

        public string LastModifiedUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string DestinationType { get; set; }
        public string Status { get; set; }

        [NotMapped]
        public string Pick
        {
            get
            {
                switch (Status)
                { 
                    case "WEB PICK":
                        return "Store's next pick day";
                    case "HOLD-REL":
                        return "Store's next pick day";
                    case "E-PICK":
                        return "Pick right away";
                }
                return "unknown";
            }
        }

        [NotMapped]
        public string Category { get; set; }

        [XmlIgnore]
        public virtual DistributionCenter DistributionCenter { get; set; }

        [NotMapped]
        public bool Release { get; set; }

        private int _unitQty;
        [NotMapped]
        public int UnitQty
        {
            get
            {
                return _unitQty;
            }
            set 
            {
                _unitQty = value;
            }
        }

        [Column("QuantumRecordTypeCode")]
        [ForeignKey("QuantumRecordType")]
        public string RecordType { get; set; }
        
        public virtual QuantumRecordTypeCode QuantumRecordType { get; set; }

        [NotMapped]
        public string DC { get; set; }

        [NotMapped]
        public string RingFencePickStore { get; set; }

        [ForeignKey("RDQRejectedReason")]
        [Column("RDQRejectReasonCode")]
        public int? RDQRejectedReasonCode { get; set; }

        public virtual RDQRejectReasonCode RDQRejectedReason { get; set; }

        public DateTime? TransmitControlDate { get; set; }

        [NotMapped]
        public bool CanPick
        {
            get
            {
                if (Status == null)
                    return true;
                return ((Status.StartsWith("HOLD")) && (Status != "HOLD-XDC"));
            }
        }
    }
}
