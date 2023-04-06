using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using Footlocker.Logistics.Allocation.Services;


namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFence
    {
        private string _store;
        private string _sku;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        public string Division { get; set; }

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
                    if (value.Length <= 2)
                    {
                        _store = value.PadLeft(2, '0');
                    }
                    else
                    {
                        _store = value.PadLeft(5, '0');
                    }
                }
                else
                {
                    _store = value;
                }
            }
        }

        [NotMapped]
        public bool CanPick
        {
            get
            {
                return (this.EndDate == null || this.EndDate >= DateTime.Now);
            }
        }

        [Required(ErrorMessage ="SKU is required")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "Invalid Sku, format should be ##-##-#####-##")]
        public string Sku
        {
            get
            {
                return _sku;
            }
            set
            {
                _sku = value.Trim();
            }
        }

        [NotMapped]
        public string Department
        {
            get {
                return Sku.Substring(3, 2);
            }
        }

        [Display(Name = "Caselot / Size")]
        public string Size { get; set; }

        [NotMapped]
        [Display(Name = "Purchase Order")]
        public string PO { get; set; }

        [NotMapped]        
        [Display(Name = "DCID")]
        public int DCID { get; set; }

        [NotMapped]
        public string MFCode { get; set; }

        [NotMapped]
        public virtual DistributionCenter DistributionCenter { get; set; }

        [NotMapped]
        [Display(Name = "Bin Qty")]
        public int BinQty { get; set; }

        [NotMapped]
        [Display(Name = "Case Qty")]
        public int CaseQty { get; set; }

        //[StringLayoutDelimited(7)]
        [Display(Name="Quantity")]
        public int Qty { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string LastModifiedUser { get; set; }

        [NotMapped]
        public string LastModifiedUserName { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Comments { get; set; }

        public long ItemID { get; set; }

        public virtual ItemMaster ItemMaster { get; set; }

        private string _itemDescription;

        [NotMapped]
        public string ItemDescription
        {
            get
            {
                if ((_itemDescription == null) && (ItemMaster != null))
                {
                    _itemDescription = ItemMaster.Description;
                }
                return _itemDescription;
            }
            set { _itemDescription = value; }
        }

        public virtual RingFenceType RingFenceType { get; set; }

        private string _ringFenceTypeDescription;

        [NotMapped]
        public string RingFenceTypeDescription
        {
            get {
                if ((_ringFenceTypeDescription == null)&&(RingFenceType != null))
                {
                    _ringFenceTypeDescription = RingFenceType.Description;
                }
                return _ringFenceTypeDescription; 
            }
            set { _ringFenceTypeDescription = value; }
        }

        private int _type;

        public int Type 
        {
            get { return _type; }
            set { _type = value; }
        }

        [NotMapped]
        public string ringFenceStatusCode { get; set; }

        [NotMapped]
        public RingFenceStatusCodes ringFenceStatus { get; set; }

        public virtual List<RingFenceDetail> ringFenceDetails { get; set; }

        public void calculateTotalRingFenceQuantity()
        {
            int tempQuantity = 0;

            if (this.ringFenceDetails != null)
            {
                tempQuantity = (from a in this.ringFenceDetails
                                where ((a.Size.Length == 3) &&
                                    (a.ActiveInd == "1"))
                                select a.Qty).Sum();

                var caselotRFD = (from a in this.ringFenceDetails
                                where (a.Size.Length == 5 &&
                                       a.ActiveInd == "1")                                      
                                select a).ToList();

                if (caselotRFD.Count() > 0)
                {
                    AllocationLibraryContext alc = new AllocationLibraryContext();
                   
                    foreach (RingFenceDetail cs in caselotRFD)
                    {
                        try
                        {                                                        
                            var clQty = (from a in alc.ItemPacks
                                          where a.Name == cs.Size                                                
                                          select a.TotalQty).FirstOrDefault();

                            tempQuantity += (cs.Qty * clQty);
                        }
                        catch
                        {
                            //don't have details, leave qty without caselots
                        }
                    }
                }
            }

            Qty = tempQuantity;
        }


        /// <summary>
        /// Initializes a new instance of the RingFence class.
        /// </summary>
        public RingFence()
        {
            this.ID = 0L;
            this.Division = String.Empty;
            this.Store = String.Empty;
            this.Sku = String.Empty;
            this.Size = String.Empty;
            this.PO = String.Empty;
            this.MFCode = String.Empty;            
            this.BinQty = 0;
            this.CaseQty = 0;
            this.Qty = 0;
            this.StartDate = DateTime.MinValue;
            this.EndDate = new DateTime?();
            this.CreatedBy = String.Empty;
            this.CreateDate = new DateTime?();
            this.ItemID = 0L;
            this.ItemMaster = null;
            this.RingFenceType = null;
            this.Type = 1;
        }
    }
}
