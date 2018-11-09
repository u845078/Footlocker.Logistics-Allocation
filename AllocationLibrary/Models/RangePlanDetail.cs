using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RangePlanDetail
    {
        private Int64 _id;
        [Key]
        [Column(Order=0)]
        public Int64 ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _division;

        [Key]
        [Column(Order = 1)]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Division must be in the format ##")]
        public string Division
        {
            get { return _division; }
            set { _division = value; }
        }

        private string _store;

        [Key]
        [Column(Order = 2)]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Store number must be in the format #####")]
        public string Store
        {
            get { return _store; }
            set { _store = value; }
        }

        private string _createdBy;

        public string CreatedBy
        {
            get { return _createdBy; }
            set { _createdBy = value; }
        }

        private DateTime _createDate;

        public DateTime CreateDate
        {
            get { return _createDate; }
            set { _createDate = value; }
        }

        private DateTime? _startDate;

        public DateTime? StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        private DateTime? _endDate;

        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        private DateTime? _firstReceiptDate;

        public DateTime? FirstReceipt
        {
            get { return _firstReceiptDate; }
            set { _firstReceiptDate = value; }
        }

        private string _rangeType = "Both";

        public string RangeType
        {
            get { return _rangeType; }
            set { _rangeType = value; }
        }

        public XmlNode ToXmlNode(XmlNode parentNode)
        {
            XmlNode xmlDetail;
            XmlNode newNode;
            XmlText newText;
            xmlDetail = parentNode.OwnerDocument.CreateElement("RangePlanDetail");

            newNode = xmlDetail.OwnerDocument.CreateElement("ID");
            newText = xmlDetail.OwnerDocument.CreateTextNode(this.ID.ToString());
            newNode.AppendChild(newText);
            xmlDetail.AppendChild(newNode);

            newNode = xmlDetail.OwnerDocument.CreateElement("Division");
            newText = xmlDetail.OwnerDocument.CreateTextNode(this.Division.ToString());
            newNode.AppendChild(newText);
            xmlDetail.AppendChild(newNode);

            newNode = xmlDetail.OwnerDocument.CreateElement("Store");
            newText = xmlDetail.OwnerDocument.CreateTextNode(this.Store.ToString());
            newNode.AppendChild(newText);
            xmlDetail.AppendChild(newNode);


            newNode = xmlDetail.OwnerDocument.CreateElement("CreatedBy");
            newText = xmlDetail.OwnerDocument.CreateTextNode(this.CreatedBy.ToString());
            newNode.AppendChild(newText);
            xmlDetail.AppendChild(newNode);

            return xmlDetail;
        }

    }
}
