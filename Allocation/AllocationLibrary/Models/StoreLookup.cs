using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;
using Footlocker.Logistics.Allocation.Services;
using System.Linq.Expressions;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreLookup
    {
        private string _division;

        [Key]
        [Column(Order = 0)]
        public string Division
        {
            get { return _division; }
            set { _division = value; }
        }

        private string _store;

        [Key]
        [Column(Order = 1)]
        public string Store
        {
            get { return _store; }
            set { _store = value; }
        }

        private string _region;

        [Display(Name = "Region")]
        public string Region
        {
            get { return _region; }
            set { _region = value; }
        }


        private string _league;

        public string League
        {
            get { return _league; }
            set { _league = value; }
        }

        private string _state;

        public string State
        {
            get { return _state; }
            set { _state = value; }
        }

        private string _mall;

        public string Mall
        {
            get { return _mall; }
            set { _mall = value; }
        }

        private string _storeType;

        public string StoreType
        {
            get { return _storeType; }
            set { _storeType = value; }
        }

        private string _marketArea;

        public string MarketArea
        {
            get { return _marketArea; }
            set { _marketArea = value; }
        }

        private string _climate;

        public string Climate
        {
            get { return _climate; }
            set { _climate = value; }
        }

        private string _city;

        public string City
        {
            get { return _city; }
            set { _city = value; }
        }

        private string _dba;
        
        public string DBA
        {
            get { return _dba; }
            set { _dba = value; }
        }

        public string AdHoc1 { get; set; }
        public string AdHoc2 { get; set; }
        public string AdHoc3 { get; set; }
        public string AdHoc4 { get; set; }
        public string AdHoc5 { get; set; }
        public string AdHoc6 { get; set; }
        public string AdHoc7 { get; set; }
        public string AdHoc8 { get; set; }
        public string AdHoc9 { get; set; }
        public string AdHoc10 { get; set; }
        public string AdHoc11 { get; set; }
        public string AdHoc12 { get; set; }

        public string status { get; set; }

        private StoreExtension _storeExtension;
        public StoreExtension StoreExtension 
        {
            get
            {
                return _storeExtension;
            }
            set
            {
                _storeExtension = value;
            }
        }

        private bool? _excludeStore;

        [NotMapped]
        public bool ExcludeStore
        {
            get
            {
                if (_excludeStore == null)
                {
                    if (StoreExtension != null)                    
                        _excludeStore = StoreExtension.ExcludeStore;                    
                    else                    
                        _excludeStore = false;                                     
                }

                if (_excludeStore != null)                
                    return (bool)_excludeStore;
                
                return false;
            }
            set
            {
                _excludeStore = value;
                if (StoreExtension == null)
                {
                    StoreExtension = new StoreExtension()
                    {
                        Division = Division,
                        Store = Store,
                        ExcludeStore = value
                    };
                }
            }
        }

        private DateTime? _firstReceipt;

        [NotMapped]
        public DateTime? FirstReceipt
        {
            get
            {
                if (_firstReceipt == null)
                {
                    if (StoreExtension != null)                    
                        _firstReceipt = StoreExtension.FirstReceipt;                    
                }
                return _firstReceipt;
            }
            set
            {
                _firstReceipt = value;
                if (StoreExtension == null)
                {
                    StoreExtension = new StoreExtension()
                    {
                        Division = Division,
                        Store = Store,
                        FirstReceipt = value
                    };
                }
            }
        }

        public XmlNode ToXmlNode(XmlNode parentNode)
        {
            XmlNode xmlDetail;
            XmlNode newNode;
            XmlText newText;
            xmlDetail = parentNode.OwnerDocument.CreateElement("StoreLookup");

            newNode = xmlDetail.OwnerDocument.CreateElement("Division");
            newText = xmlDetail.OwnerDocument.CreateTextNode(this.Division.ToString());
            newNode.AppendChild(newText);
            xmlDetail.AppendChild(newNode);

            newNode = xmlDetail.OwnerDocument.CreateElement("Store");
            newText = xmlDetail.OwnerDocument.CreateTextNode(this.Store.ToString());
            newNode.AppendChild(newText);
            xmlDetail.AppendChild(newNode);

            return xmlDetail;
        }
    }
}
