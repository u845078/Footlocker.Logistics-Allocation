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

        public RDQ(Int64 id, String division, String store, String sku, String size, int qty, string warehouseName, string pick, string status, string createdBy, DateTime? createdate, int? dcid)
        {
            this.ID = id;
            this.Division = division;
            this.Store = store;
            this.Sku = sku;
            this.Size = size;
            this.Qty = qty;
            this.WarehouseName = warehouseName;
            //this.Pick = pick;
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
            //this.Pick = pick;
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


        public Int64 ID { get; set; }
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Division must be in the format ##")]
        public String Division { get; set; }

        [Required]
        [RegularExpression(@"^(\d{2}|\d{5})$", ErrorMessage = "Store number must be in the format ##### or ## for warehouses")]
        public String Store { get; set; }

        [ForeignKey("DistributionCenter")]
        public Int32? DCID { get; set; }
        [NotMapped]
        [Display(Name="Warehouse")]
        public String WarehouseName { get; set; }
        public String PO { get; set; }
        public Int64? ItemID { get; set; }
        public String Type { get; set; }
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "Sku must be in the format ##-##-#####-##")]
        public String Sku { get; set; }
        [Display(Name="Size/Caselot")]
        [RegularExpression(@"^\d{3}(?:\d{2})?$", ErrorMessage = "Size must be 3 or 5 digits")]
        public String Size { get; set; }
        public Int32 Qty { get; set; }
        public Int32? TargetQty { get; set; }
        public Int32? ForecastQty { get; set; }
        public Int32? NeedQty { get; set; }
        [NotMapped]
        public Int32 UserRequestedQty 
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
        public String CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public String DestinationType { get; set; }
        public String Status { get; set; }

        [NotMapped]
        public String Pick
        {
            get
            {
                switch (Status)
                { 
                    case "WEB PICK":
                        return "Store's next pick day";
                    case "HOLD-REL":
                        return "Store's next pick day";
                    case "FORCE PICK":
                        return "Pick tomorrow";
                }
                return "unknown";
            }
        }

        [NotMapped]
        public string Category { get; set; }

        [XmlIgnore]
        public virtual DistributionCenter DistributionCenter { get; set; }

        [NotMapped]
        public Boolean Release { get; set; }

        //public virtual List<ItemPack> ItemPack { get; set; }

        private int _unitQty;
        [NotMapped]
        public int UnitQty
        {
            get
            {
                //if ((Size.Length > 3)&&(ItemPack != null))
                //{
                //    return (Qty * ItemPack[0].TotalQty);
                //}
                return _unitQty;
            }
            set {
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
        public Int32? RDQRejectedReasonCode { get; set; }

        public virtual RDQRejectReasonCode RDQRejectedReason { get; set; }

        [NotMapped]
        public Boolean CanPick
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
