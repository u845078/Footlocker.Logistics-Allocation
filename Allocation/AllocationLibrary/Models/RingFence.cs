using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using Footlocker.Logistics.Allocation.Services;

using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFence : BiExtract
    {
        private string _store;
        private string _sku;

        public override bool IsValid()
        {
            return true;
        }

        [StringLayoutDelimited(0)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        [StringLayoutDelimited(1)]
        public string Division { get; set; }

        [StringLayoutDelimited(2)]
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

        [StringLayoutDelimited(3)]
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
        [StringLayoutDelimited(4)]
        public string Size { get; set; }

        [NotMapped]
        [Display(Name = "Purchase Order")]
        [StringLayoutDelimited(5)]
        public string PO { get; set; }

        [NotMapped]
        [StringLayoutDelimited(6)]
        [Display(Name = "DCID")]
        public Int32 DCID { get; set; }

        [NotMapped]
        [Display(Name = "Bin Qty")]
        [StringLayoutDelimited(7)]
        public Int32 BinQty { get; set; }

        [NotMapped]
        [Display(Name = "Case Qty")]
        [StringLayoutDelimited(8)]
        public Int32 CaseQty { get; set; }

        //[StringLayoutDelimited(7)]
        [Display(Name="Total Units")]
        public Int32 Qty { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [StringLayoutDelimited(9, "yyyy-MM-dd")]
        public DateTime StartDate { get; set; }

        [StringLayoutDelimited(10, "yyyy-MM-dd")]
        public DateTime? EndDate { get; set; }

        [StringLayoutDelimited(11)]
        public string CreatedBy { get; set; }

        [StringLayoutDelimited(12, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime? CreateDate { get; set; }

        public string LastModifiedUser { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Comments { get; set; }

        public Int64 ItemID { get; set; }

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

        private Int32 _type;

        public Int32 Type 
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
            this.DCID = 0;
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

        /// <summary>
        /// Initializes a new instance of the RingFence class.
        /// </summary>
        /// <param name="id">The initial value for the identifier property.</param>
        /// <param name="division">The initial value for the division property.</param>
        /// <param name="store">The initial value for the store property.</param>
        /// <param name="sku">The initial value for the stock keeping unit property.</param>
        /// <param name="size">The initial value for the size property.</param>
        /// <param name="qty">The initial value for the quantity property.</param>
        /// <param name="startDate">The initial value for the start date property.</param>
        /// <param name="endDate">The initial value for the end date property.</param>
        /// <param name="createdBy">The initial value for the created by property.</param>
        /// <param name="createDate">The initial value for the create date property.</param>
        /// <param name="itemId">The initial value for the item identifier property.</param>
        /// <param name="itemMaster">The initial value for the item master property.</param>
        /// <param name="ringFenceType">The initial value for the ring fence type property.</param>
        /// <param name="type">The initial value for the type property.</param>
        public RingFence(Int64 id, string division, string store, string sku, string size, string po, Int32 dcid, Int32 binQty, Int32 caseQty, Int32 qty, DateTime startDate
                , DateTime? endDate, string createdBy, DateTime? createDate, Int64 itemId, ItemMaster itemMaster
                , RingFenceType ringFenceType, Int32 type)
            : this()
        {
            this.ID = id;
            this.Division = division;
            this.Store = store;
            this.Sku = sku;
            this.Size = size;
            this.PO = po;
            this.DCID = dcid;
            this.BinQty = binQty;
            this.CaseQty = caseQty;
            this.Qty = qty;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.CreatedBy = createdBy;
            this.CreateDate = createDate;
            this.ItemID = itemId;
            this.ItemMaster = itemMaster;
            this.RingFenceType = ringFenceType;
            this.Type = type;
        }
    }
}
