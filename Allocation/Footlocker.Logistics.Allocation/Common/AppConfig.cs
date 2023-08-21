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
        private string _ringFenceUploadTemplate;
        private string _storeTemplate;
        private string _vendorGroupTemplate;
        private string _skuTypeFile;
        private bool _enableFTP;
        private string _europeDivisions;
        private string _ftpServer;
        private string _ftpUserName;
        private string _ftpPassword;
        private string _quoteFTPCommand;
        private string _skuTypeDataset;
        private string _skuTypeDatasetEurope;
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

        public string HoldsUploadTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_holdsUploadTemplate))
                    _holdsUploadTemplate = ConfigurationManager.AppSettings["HoldsUploadTemplate"].ToString();

                return _holdsUploadTemplate;
            }
        }

        public string RingFenceUploadTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_ringFenceUploadTemplate))
                    _ringFenceUploadTemplate = ConfigurationManager.AppSettings["RingFenceUploadTemplate"].ToString();

                return _ringFenceUploadTemplate;
            }
        }

        public string StoreTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_storeTemplate))
                    _storeTemplate = ConfigurationManager.AppSettings["StoreTemplate"].ToString();

                return _storeTemplate;
            }
        }

        public string VendorGroupTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_vendorGroupTemplate))
                    _vendorGroupTemplate = ConfigurationManager.AppSettings["VendorGroupTemplate"].ToString();

                return _vendorGroupTemplate;
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

        public string SkuTypeTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_skuTypeTemplate))
                    _skuTypeTemplate = ConfigurationManager.AppSettings["SkuTypeTemplate"].ToString();

                return _skuTypeTemplate;
            }
        }

        public string ProductTypeTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_productTypeTemplate))
                    _productTypeTemplate = ConfigurationManager.AppSettings["ProductTypeTemplate"].ToString();

                return _productTypeTemplate;
            }
        }

        public string RDQRestrictionsTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_rdqRestrictionsTemplate))
                    _rdqRestrictionsTemplate = ConfigurationManager.AppSettings["RDQRestrictionsTemplate"].ToString();

                return _rdqRestrictionsTemplate;
            }
        }

        public string HoldDeleteTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_holdDeleteTemplate))
                    _holdDeleteTemplate = ConfigurationManager.AppSettings["HoldDeleteTemplate"].ToString();

                return _holdDeleteTemplate;
            }
        }

        public string RerankStoresTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_rerankStoresTemplate))
                    _rerankStoresTemplate = ConfigurationManager.AppSettings["RerankStoresTemplate"].ToString();

                return _rerankStoresTemplate;
            }
        }

        public string SkuTypeFile
        {
            get
            {
                if (string.IsNullOrEmpty(_skuTypeFile))
                    _skuTypeFile = ConfigurationManager.AppSettings["skutypefile"].ToString();

                return _skuTypeFile;
            }
        }

        public bool EnableFTP
        {
            get
            {
                _enableFTP = ConfigurationManager.AppSettings["enableFTP"].ToString().ToLower() == "true";

                return _enableFTP;
            }
        }

        public string EuropeDivisions
        {
            get
            {
                if (string.IsNullOrEmpty(_europeDivisions))
                    _europeDivisions = ConfigurationManager.AppSettings["EUROPE_DIV"].ToString();

                return _europeDivisions;
            }
        }

        public string FTPServer
        {
            get
            {
                if (string.IsNullOrEmpty(_ftpServer))
                    _ftpServer = ConfigurationManager.AppSettings["FTPServer"].ToString();

                return _ftpServer;
            }
        }

        public string FTPUserName
        {
            get
            {
                if (string.IsNullOrEmpty(_ftpUserName))
                    _ftpUserName = ConfigurationManager.AppSettings["FTPUserName"].ToString();

                return _ftpUserName;
            }
        }

        public string FTPPassword
        {
            get
            {
                if (string.IsNullOrEmpty(_ftpPassword))
                    _ftpPassword = ConfigurationManager.AppSettings["FTPPassword"].ToString();

                return _ftpPassword;
            }
        }

        public string QuoteFTPCommand
        {
            get
            {
                if (string.IsNullOrEmpty(_quoteFTPCommand))
                    _quoteFTPCommand = ConfigurationManager.AppSettings["QuoteFTPCommand"].ToString();

                return _quoteFTPCommand;
            }
        }

        public string SKUTypeDataset
        {
            get
            {
                if (string.IsNullOrEmpty(_skuTypeDataset))
                    _skuTypeDataset = ConfigurationManager.AppSettings["SkuTypeDataset"].ToString();

                return _skuTypeDataset;
            }
        }

        public string SKUTypeDatasetEurope
        {
            get
            {
                if (string.IsNullOrEmpty(_skuTypeDatasetEurope))
                    _skuTypeDatasetEurope = ConfigurationManager.AppSettings["SkuTypeDatasetEurope"].ToString();

                return _skuTypeDatasetEurope;
            }
        }
    }
}