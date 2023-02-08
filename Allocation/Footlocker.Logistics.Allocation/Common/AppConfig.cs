using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.DAO;

namespace Footlocker.Logistics.Allocation.Common
{
    public class AppConfig
    {
        private string _asposeLicenseFile;
        private string _logFile;
        private string _webPickTemplate;
        private string _skuTypeTemplate;
        private string _productTypeTemplate;
        private string _skuIdUploadTemplate;
        private string _arSkusUploadTemplate;
        private string _holdsUploadTemplate;
        private string _arConstraintsUploadTemplate;
        private string _skuRangePlanDGUploadTemplate;
        private string _rdqRestrictionsTemplate;
        private string _rangeTemplate;
        private string _ringFenceDeleteTemplate;
        private string _holdDeleteTemplate;
        private string _rerankStoresTemplate;
        private string _skuAttributeTemplate;
        private string _crossdockLinkTemplate;

        public string AppName;
        public string AppPath;
        public WebUser currentUser;
        public AllocationContext db;

        public string WebPickTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_webPickTemplate))
                    _webPickTemplate = ConfigurationManager.AppSettings["WebPickTemplate"].ToString();

                return _webPickTemplate;
            }
        }

        public string RangeTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_rangeTemplate))
                    _rangeTemplate = ConfigurationManager.AppSettings["RangeTemplate"].ToString();

                return _rangeTemplate;
            }
        }

        public string ARConstraintsUploadTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_arConstraintsUploadTemplate))
                    _arConstraintsUploadTemplate = ConfigurationManager.AppSettings["ARConstraintsUploadTemplate"].ToString();

                return _arConstraintsUploadTemplate;
            }
        }

        public string ARSKUsUploadTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_arSkusUploadTemplate))
                    _arSkusUploadTemplate = ConfigurationManager.AppSettings["ARSkusUploadTemplate"].ToString();

                return _arSkusUploadTemplate;
            }
        }

        public string SKUIDUploadTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_skuIdUploadTemplate))
                    _skuIdUploadTemplate = ConfigurationManager.AppSettings["SkuIdUploadTemplate"].ToString();

                return _skuIdUploadTemplate;
            }
        }

        public string SKURangePlanDGUploadTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_skuRangePlanDGUploadTemplate))
                    _skuRangePlanDGUploadTemplate = ConfigurationManager.AppSettings["SkuRangePlanDGUploadTemplate"].ToString();

                return _skuRangePlanDGUploadTemplate;
            }
        }

        public string RingFenceDeleteTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_ringFenceDeleteTemplate))
                    _ringFenceDeleteTemplate = ConfigurationManager.AppSettings["RingFenceDeleteTemplate"].ToString();

                return _ringFenceDeleteTemplate;
            }
        }

        public string SKUAttributeTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_skuAttributeTemplate))
                    _skuAttributeTemplate = ConfigurationManager.AppSettings["SKUAttributeTemplate"].ToString();

                return _skuAttributeTemplate;
            }
        }

        public string CrossdockLinkTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_crossdockLinkTemplate))
                    _crossdockLinkTemplate = ConfigurationManager.AppSettings["CrossdockLinkTemplate"].ToString();

                return _crossdockLinkTemplate;
            }
        }

        public string AsposeLicenseFile 
        { 
            get
            {
                if (string.IsNullOrEmpty(_asposeLicenseFile))               
                    _asposeLicenseFile = ConfigurationManager.AppSettings["AsposeLicenseFile"].ToString();
                
                return _asposeLicenseFile;
            }            
        }

        public string LogFile
        {
            get
            {
                if (string.IsNullOrEmpty(_logFile))
                    _logFile = ConfigurationManager.AppSettings["LogFile"].ToString();

                return _logFile;
            }
        }

    }
}