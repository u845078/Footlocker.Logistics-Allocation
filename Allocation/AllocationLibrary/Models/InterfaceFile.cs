using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    /// <summary>
    /// Provides an object representation of an interface file.
    /// </summary>
    public sealed class InterfaceFile
    {
        /// <summary>
        /// The interface file types.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// The pack interface file type.
            /// </summary>
            Pack,

            /// <summary>
            /// The size interface file type.
            /// </summary>
            Size,

            /// <summary>
            /// The future inventory interface file type.
            /// </summary>
            FutureInventory,

            /// <summary>
            /// The description interface file type.
            /// </summary>
            Description,

            /// <summary>
            /// The product interface file type.
            /// </summary>
            Product,

            /// <summary>
            /// The location interface file type.
            /// </summary>
            Location,

            /// <summary>
            /// The inventory interface file type.
            /// </summary>
            Inventory,

            /// <summary>
            /// The transaction interface file type.
            /// </summary>
            Transaction,

            /// <summary>
            /// The store interface file type.
            /// </summary>
            Store,

            /// <summary>
            /// The item interface file type.
            /// </summary>
            Item,

            /// <summary>
            /// The product location interface file type.
            /// </summary>
            ProductLocation,

            /// <summary>
            /// The price interface file type.
            /// </summary>
            Price,

            /// <summary>
            /// The store plan interface file type.
            /// </summary>
            StorePlan,

            /// <summary>
            /// The requested distribtion quantity interface file type.
            /// </summary>
            RequestedDistributionQuantity,

            /// <summary>
            /// The Europe ecomm inventory interface file type.
            /// </summary>
            EcommInventory,

            /// <summary>
            /// The picked stores interface file type.
            /// </summary>
            PickedStores,

            /// <summary>
            /// The purchase order delivery date interface file type.
            /// </summary>
            PoDeliveryDate

        }

        /// <summary>
        /// Defines the pack file name.
        /// </summary>
        private const string PackFileName = "RC3967";

        /// <summary>
        /// Defines the pack file description.
        /// </summary>
        private const string PackFileDescription = "CASELOT PACK FILE";

        /// <summary>
        /// Defines the size file name.
        /// </summary>
        private const string SizeFileName = "RC3968";

        /// <summary>
        /// Defines the size file description.
        /// </summary>
        private const string SizeFileDescription = ".NET SKU SIZE FILE";

        /// <summary>
        /// Defines the description file name.
        /// </summary>
        private const string DescriptionFileName = "RC3971";

        /// <summary>
        /// Defines the description file description.
        /// </summary>
        private const string DescriptionFileDescription = "HIERARCHY DESCRIPTION FILE";

        /// <summary>
        /// Defines the future inventory file name.
        /// </summary>
        private const string FutureInventoryFileName = "RC4776";

        /// <summary>
        /// Defines the future inventory file description.
        /// </summary>
        private const string FutureInventoryFileDescription = "FUTURE INVENTORY FILE";

        /// <summary>
        /// Defines the product file name.
        /// </summary>
        private const string ProductFileName = "RC4779";

        /// <summary>
        /// Defines the product file description.
        /// </summary>
        private const string ProductFileDescription = "PRODUCT FILE";

        /// <summary>
        /// Defines the location file name.
        /// </summary>
        private const string LocationFileName = "RC4780";

        /// <summary>
        /// Defines the location file description.
        /// </summary>
        private const string LocationFileDescription = "LOCATION FILE";

        /// <summary>
        /// Defines the inventory file name.
        /// </summary>
        private const string InventoryFileName = "RC4781";

        /// <summary>
        /// Defines the inventory file description.
        /// </summary>
        private const string InventoryFileDescription = "INVENTORY BALANCE FILE";

        /// <summary>
        /// Defines the transaction file name.
        /// </summary>
        private const string TransactionFileName = "RC4786";

        /// <summary>
        /// Defines the transaction file description.
        /// </summary>
        private const string TransactionFileDescription = "TRANSACTION FILE";

        /// <summary>
        /// Defines the store file name.
        /// </summary>
        private const string StoreFileName = "RC4788";

        /// <summary>
        /// Defines the store file description.
        /// </summary>
        private const string StoreFileDescription = ".NET LOCATION FILE";

        /// <summary>
        /// Defines the item file name.
        /// </summary>
        private const string ItemFileName = "RC4789";

        /// <summary>
        /// Defines the item file description.
        /// </summary>
        private const string ItemFileDescription = ".NET PRODUCT FILE";

        /// <summary>
        /// Defines the product location file name.
        /// </summary>
        private const string ProductLocationFileName = "RC4791";

        /// <summary>
        /// Defines the product location file description.
        /// </summary>
        private const string ProductLocationFileDescription = ".NET PRODUCT LOCATION FILE";

        /// <summary>
        /// Defines the price file name.
        /// </summary>
        private const string PriceFileName = "RC3976";

        /// <summary>
        /// Defines the price file description.
        /// </summary>
        private const string PriceFileDescription = "PRICE FILE";

        /// <summary>
        /// Defines the store plan file name.
        /// </summary>
        private const string StorePlanFileName = "RC4733";

        /// <summary>
        /// Defines the store plan file description.
        /// </summary>
        private const string StorePlanFileDescription = "STORE PLAN FILE";

        /// <summary>
        /// Defines the requested distribtution quantities file name.
        /// </summary>
        private const string RequestedDistributionQuantitiesFileName = "RC3977";

        /// <summary>
        /// Defines the requested distribution quantities file description.
        /// </summary>
        private const string RequestedDistributionQuantitiesFileDescription = "REQUESTED DISTRIBUTION QUANTITIES FILE";

        /// <summary>
        /// Defines the requested distribtution quantities file name.
        /// </summary>
        private const string EcommFileName = "RC6235";

        /// <summary>
        /// Defines the requested distribution quantities file description.
        /// </summary>
        private const string EcommFileDescription = "ECOMM INVENTORY FILE";

        /// <summary>
        /// Defines the picked stores file name.
        /// </summary>
        private const string PickedStoresFileName = "RC3978";

        /// <summary>
        /// Defines the picked stores description.
        /// </summary>
        private const string PickedStoresDescription = "PICKED STORES";

        /// <summary>
        /// Defines the purchase order delivery date file name.
        /// </summary>
        private const string PoDeliveryDateFileName = "RC3982";

        /// <summary>
        /// Defines the purchase order delivery date description.
        /// </summary>
        private const string PoDeliveryDateDescription = "PO DELIVERY DATE FILE";

        /// <summary>
        /// The interface file type.
        /// </summary>
        private Type _type = Type.Pack;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown interface file type is encountered.</exception>
        public string Name
        {
            get
            {
                string returnValue = String.Empty;

                switch (this._type)
                {
                    case Type.Pack:
                        returnValue = InterfaceFile.PackFileName;
                        break;
                    case Type.Size:
                        returnValue = InterfaceFile.SizeFileName;
                        break;
                    case Type.Description:
                        returnValue = InterfaceFile.DescriptionFileName;
                        break;
                    case Type.FutureInventory:
                        returnValue = InterfaceFile.FutureInventoryFileName;
                        break;
                    case Type.Product:
                        returnValue = InterfaceFile.ProductFileName;
                        break;
                    case Type.Location:
                        returnValue = InterfaceFile.LocationFileName;
                        break;
                    case Type.Inventory:
                        returnValue = InterfaceFile.InventoryFileName;
                        break;
                    case Type.Transaction:
                        returnValue = InterfaceFile.TransactionFileName;
                        break;
                    case Type.Store:
                        returnValue = InterfaceFile.StoreFileName;
                        break;
                    case Type.Item:
                        returnValue = InterfaceFile.ItemFileName;
                        break;
                    case Type.ProductLocation:
                        returnValue = InterfaceFile.ProductLocationFileName;
                        break;
                    case Type.Price:
                        returnValue = InterfaceFile.PriceFileName;
                        break;
                    case Type.StorePlan:
                        returnValue = InterfaceFile.StorePlanFileName;
                        break;
                    case Type.RequestedDistributionQuantity:
                        returnValue = InterfaceFile.RequestedDistributionQuantitiesFileName;
                        break;
                    case Type.EcommInventory:
                        returnValue = InterfaceFile.EcommFileName;
                        break;
                    case Type.PickedStores:
                        returnValue = InterfaceFile.PickedStoresFileName;
                        break;
                    case Type.PoDeliveryDate:
                        returnValue = InterfaceFile.PoDeliveryDateFileName;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("this._type", this._type
                            , "An unknown interface file type has been encountered.");
                }
                return returnValue;
            }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown interface file type is encountered.</exception>
        public string Description
        {
            get
            {
                string returnValue = String.Empty;

                switch (this._type)
                {
                    case Type.Pack:
                        returnValue = InterfaceFile.PackFileDescription;
                        break;
                    case Type.Size:
                        returnValue = InterfaceFile.SizeFileDescription;
                        break;
                    case Type.Description:
                        returnValue = InterfaceFile.DescriptionFileDescription;
                        break;
                    case Type.FutureInventory:
                        returnValue = InterfaceFile.FutureInventoryFileDescription;
                        break;
                    case Type.Product:
                        returnValue = InterfaceFile.ProductFileDescription;
                        break;
                    case Type.Location:
                        returnValue = InterfaceFile.LocationFileDescription;
                        break;
                    case Type.Inventory:
                        returnValue = InterfaceFile.InventoryFileDescription;
                        break;
                    case Type.Transaction:
                        returnValue = InterfaceFile.TransactionFileDescription;
                        break;
                    case Type.Store:
                        returnValue = InterfaceFile.StoreFileDescription;
                        break;
                    case Type.Item:
                        returnValue = InterfaceFile.ItemFileDescription;
                        break;
                    case Type.ProductLocation:
                        returnValue = InterfaceFile.ProductLocationFileDescription;
                        break;
                    case Type.Price:
                        returnValue = InterfaceFile.PriceFileDescription;
                        break;
                    case Type.StorePlan:
                        returnValue = InterfaceFile.StorePlanFileDescription;
                        break;
                    case Type.RequestedDistributionQuantity:
                        returnValue = InterfaceFile.RequestedDistributionQuantitiesFileDescription;
                        break;
                    case Type.EcommInventory:
                        returnValue = InterfaceFile.EcommFileDescription;
                        break;
                    case Type.PickedStores:
                        returnValue = InterfaceFile.PickedStoresDescription;
                        break;
                    case Type.PoDeliveryDate:
                        returnValue = InterfaceFile.PoDeliveryDateDescription;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("this._type", this._type
                            , "An unknown interface file type has been encountered.");
                }
                return returnValue;
            }
        }

        /// <summary>
        /// Initializes a new instance of the InterfaceFile class.
        /// </summary>
        /// <param name="type">The initial value for the type attribute.</param>
        public InterfaceFile(Type type)
        {
            this._type = type;
        }

        /// <summary>
        /// Initializes a new instance of the InterfaceFile class.
        /// </summary>
        /// <param name="type">The file type.</param>
        public InterfaceFile(string type)
            : this(InterfaceFile.ConvertStringToType(type))
        {
        }

        /// <summary>
        /// Convert a string to a type.
        /// </summary>
        /// <param name="fileType">The string to be converted.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown file type is specified.</exception>
        private static Type ConvertStringToType(string fileType)
        {
            Type returnValue = Type.Pack;

            if (Enum.TryParse(fileType, true, out returnValue))
            {
            }
            else
            {
                throw new ArgumentOutOfRangeException("fileType", fileType
                    , "An unknown interface file type was specified.");
            }
            return returnValue;
        }

        /// <summary>
        /// Convert the current interface file to a string.
        /// </summary>
        /// <returns>The string representation of the current interface file.</returns>
        public override string ToString()
        {
            return String.Format("{0} - {1}", this.Name, this.Description);
        }
    }
}
