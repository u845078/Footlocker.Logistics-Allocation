using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class WarehouseAvailableInventory
    {
        public string Division { get; set; }
        public string Department { get; set; }
        public string StockNumber { get; set; }
        public string WidthColor { get; set; }
        public string Size { get; set; }
        public string MFCode { get; set; }
        public string PO { get; set; }
        public int Quantity { get; set; }
        public int PickReserveQuantity { get; set; }
        public string Sku
        {
            get
            {
                return string.Format("{0}-{1}-{2}-{3}", Division, Department, StockNumber, WidthColor);
            }
        }

        public WarehouseAvailableInventory()
            : base()
        {
            this.Division = string.Empty;
            this.Department = string.Empty;
            this.StockNumber = string.Empty;
            this.WidthColor = string.Empty;
            this.Size = string.Empty;
            this.MFCode = string.Empty;
            this.PO = string.Empty;
            this.Quantity = 0;
        }

        public WarehouseAvailableInventory(string division, string department, string stockNumber
                                           , string widthColor, string size, string distributionCenterID, int quantity)
            : this()
        {
            this.Division = division;
            this.Department = department;
            this.StockNumber = stockNumber;
            this.WidthColor = widthColor;
            this.Size = size;
            this.MFCode = distributionCenterID;
            this.Quantity = quantity;
        }

        public WarehouseAvailableInventory(string division, string department, string stockNumber
                                           , string widthColor, string size, string distributionCenterID, string po, int quantity)
            : this(division, department, stockNumber, widthColor, size, distributionCenterID, quantity)
        {
            this.PO = po;
        }

        public WarehouseAvailableInventory(string division, string department, string stockNumber
                                           , string widthColor, string size, string distributionCenterID, int quantity, int pickReserve)
            : this(division, department, stockNumber, widthColor, size, distributionCenterID, quantity)
        {
            this.PickReserveQuantity = pickReserve;
        }

        
    }
}
