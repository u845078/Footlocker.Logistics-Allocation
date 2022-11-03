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