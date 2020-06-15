using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using Aspose.Excel;

using Footlocker.Common;
using Footlocker.Common.Utilities.File;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
//using Footlocker.Common.Services; 
using System.Data.Common;
using System.Data.Entity;
using System.Xml.Serialization;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class UploadController : AppController
    {
        #region Fields

        private string _currentFormattedDateTimeString = null;
        private string _userName = null;
        private string _fileLineFiller = null;

        #endregion

        #region Non-Public Properties

        private int ASCIINumericThreshold { get { return 127; } }

        private string EuropeDivCode { get { return "31"; } }

        private string CurrentFormattedDateTimeString
        {
            get
            {
                if (_currentFormattedDateTimeString == null) 
                {
                    _currentFormattedDateTimeString = 
                        DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffffff").PadRight(26, ' ').Substring(0, 26); 
                }
                return _currentFormattedDateTimeString;
            }
        }

        private string FileLineFiller
        {
            get
            {
                if (_fileLineFiller == null)
                {
                    _fileLineFiller = String.Empty.PadLeft(18, ' ');
                }
                return _fileLineFiller;
            }
        }

        private string UserName
        {
            get
            {
                if (_userName == null)
                {
                    _userName = User.Identity.Name.Split('\\')[1].PadRight(30, ' ').Substring(0, 30);
                }
                return _userName;
            }
        }

        #endregion

        //
        // GET: /Upload/
        //#region "Session Variables"
        //public List<string> UploadList
        //{
        //    get
        //    {
        //        if (Session["UploadList"] == null)
        //        {
        //            Session["UploadList"] = new List<string>();
        //        }

        //        return (List<string>)Session["UploadList"];

        //    }
        //    set { Session["UploadList"] = value; }
        //}
        //#endregion

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support")]
        public ActionResult SkuTypeUpload()
        {
            return View();
        }

        
        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support")]
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            string Division = "";
            string filepath = System.Configuration.ConfigurationManager.AppSettings["skutypefile"] + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmssffffff") + ".txt";
            TextWriter txtWrite = new StreamWriter(filepath);

            foreach (HttpPostedFileBase file in attachments)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];
                string effectiveDate = "";
                string mainDivision;
                int row = 1;
                Boolean writeAV = (Convert.ToString(mySheet.Cells[0, 4].Value).Contains("Availability"));
                string availabilityCodes = "";
                if (writeAV)
                {
                    Division = Convert.ToString(mySheet.Cells[row, 0].Value).Substring(0, 2);
                    availabilityCodes = (new MainframeDAO()).GetAvailabityCodes(Division);
                }

                if ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("SKU")) &&
                    (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("Type")) && (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("EffectiveDate"))
                    )
                {
                    Division = Convert.ToString(mySheet.Cells[row, 0].Value).Substring(0, 2);
                    mainDivision = Division;
                    if (!(Footlocker.Common.WebSecurityService.UserHasDivision(User.Identity.Name.Split('\\')[1], "Allocation", Division)))
                    {
                        txtWrite.Flush();
                        txtWrite.Close();
                        return Content("You do not have permission to update this division.");
                    }
                    string[] tokens;
                    while (mySheet.Cells[row, 0].Value != null)
                    {
                        if (mySheet.Cells[row, 3].Value != null)
                        {
                            effectiveDate = Convert.ToDateTime(mySheet.Cells[row, 3].Value).ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            effectiveDate = DateTime.Now.ToString("yyyy-MM-dd");
                        }

                        Division = Convert.ToString(mySheet.Cells[row, 0].Value).Substring(0, 2);
                        if (!(Division.Equals(mainDivision)))
                        {
                            txtWrite.Flush();
                            txtWrite.Close();
                            try
                            {
                                System.IO.File.Delete(filepath);
                            }
                            catch { }

                            return Content("Spreadsheet must be for one division only.");
                        }
                        tokens = Convert.ToString(mySheet.Cells[row, 0].Value).Split('-');
                        //txtWrite.WriteLine(
                        //        tokens[0].PadLeft(2, '0') +//div
                        //        tokens[1].PadLeft(2, '0') +//dept
                        //        tokens[2].PadLeft(5, '0') +//stk
                        //        tokens[3].PadLeft(2, '0') +//width
                        //        effectiveDate +//effective date
                        //        Convert.ToString(mySheet.Cells[row, 2].Value).PadRight(1, ' ') +//service type
                        //        DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffffff") +//create date
                        //        User.Identity.Name.Split('\\')[1].PadRight(30, ' ').Substring(0, 30) +//user
                        //        "".PadRight(12, ' ') //filler

                        //    );
                        txtWrite.WriteLine(
                            "SRVTY" +
                                (tokens[0].PadLeft(2, '0') +//div
                                tokens[1].PadLeft(2, '0') +//dept
                                tokens[2].PadLeft(5, '0') +//stk
                                tokens[3].PadLeft(2, '0')).PadRight(30,' ') +//width
                                (effectiveDate +Convert.ToString(mySheet.Cells[row, 2].Value).PadRight(1, ' ')).PadRight(60,' ') +//service type
                                DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffffff") +//create date
                                User.Identity.Name.Split('\\')[1].PadRight(30, ' ').Substring(0, 30) +//user
                                "".PadRight(9, ' ') //filler
                            );
                        if (writeAV && (Convert.ToString(mySheet.Cells[row, 4].Value).Length > 0))
                        {
                            if (availabilityCodes.Contains(Convert.ToString(mySheet.Cells[row, 4].Value).PadRight(1,' ')))
                            {
                                txtWrite.WriteLine(
                                "SKUAV" +
                                    (tokens[0].PadLeft(2, '0') +//div
                                    tokens[1].PadLeft(2, '0') +//dept
                                    tokens[2].PadLeft(5, '0') +//stk
                                    tokens[3].PadLeft(2, '0')).PadRight(30, ' ') +//width
                                    (Convert.ToString(mySheet.Cells[row, 4].Value).PadRight(1, ' ')).PadRight(60, ' ') +//availability code
                                    DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffffff") +//create date
                                    User.Identity.Name.Split('\\')[1].PadRight(30, ' ').Substring(0, 30) +//user
                                    "".PadRight(9, ' ') //filler
                                    );
                            }
                        }

                        row++;
                    }
                }
                else
                {
                    return Content("Incorrect header, please use template.");
                }
            }
            txtWrite.Flush();
            txtWrite.Close();

            int failcount = 1;
            while (failcount < 5)
            {
                try
                {
                    if ("true".Equals(System.Configuration.ConfigurationManager.AppSettings["enableFTP"]))
                    {
                        if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(Division))
                        {
                            Footlocker.Common.Services.FTPService ftp = new Footlocker.Common.Services.FTPService(System.Configuration.ConfigurationManager.AppSettings["FTPServer"], System.Configuration.ConfigurationManager.AppSettings["FTPUserName"], System.Configuration.ConfigurationManager.AppSettings["FTPPassword"]);
                            //code replaced for secure FTP
                            //ftp.Connect(0, 0);
                            //ftp.SendFileToMainframe(filepath, System.Configuration.ConfigurationManager.AppSettings["SkuTypeDatasetEurope"], System.Configuration.ConfigurationManager.AppSettings["QuoteFTPCommand"]);
                            //ftp.Quit();

                            ftp.FTPSToMainframe(filepath, System.Configuration.ConfigurationManager.AppSettings["SkuTypeDatasetEurope"], 0, 0, System.Configuration.ConfigurationManager.AppSettings["QuoteFTPCommand"]);
                            ftp.Disconnect();
                        }
                        else
                        {
                            Footlocker.Common.Services.FTPService ftp = new Footlocker.Common.Services.FTPService(System.Configuration.ConfigurationManager.AppSettings["FTPServer"], System.Configuration.ConfigurationManager.AppSettings["FTPUserName"], System.Configuration.ConfigurationManager.AppSettings["FTPPassword"]);
                            //code replaced for secure FTP
                            //ftp.Connect(0, 0);
                            //ftp.SendFileToMainframe(filepath, System.Configuration.ConfigurationManager.AppSettings["SkuTypeDataset"], System.Configuration.ConfigurationManager.AppSettings["QuoteFTPCommand"]);
                            //ftp.Quit();

                            ftp.FTPSToMainframe(filepath, System.Configuration.ConfigurationManager.AppSettings["SkuTypeDataset"], 0, 0, System.Configuration.ConfigurationManager.AppSettings["QuoteFTPCommand"]);
                            ftp.Disconnect();
                        }
                    }
                    System.IO.File.Delete(filepath);
                    return Content("");
                }
                catch (Exception ex)
                {
                    failcount++;
                    if (failcount == 5)
                    {
                        try
                        {
                            System.IO.File.Delete(filepath);
                        }
                        catch { }
                        return Content(ex.Message);
                    }
                }
            }
            return Content("Shouldn't ever get here");
        }

        public ActionResult ExcelTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SkuTypeTemplate"]), FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            excelDocument.Save("ServiceTypeUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult ExcelProductTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ProductTypeTemplate"]), FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            excelDocument.Save("LifeOfSkuTemplate.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }


        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ProductTypeUpload()
        {
            return View();
        }



        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult NetworkUpload()
        {
            return View();
        }

        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult BTSUpload()
        {
            return View();
        }

        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SeasonUpload()
        {
            return View();
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult SaveProductType(IEnumerable<HttpPostedFileBase> attachments)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            string Division = "";

            foreach (HttpPostedFileBase file in attachments)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                int row = 1;
                int validCount = 0;
                int errorCount = 0;
                if ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Div")) && (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("Dept")) &&
                    (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("Stock")) && (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("ProductType"))
                    )
                {
                    Division = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(2, '0');
                    ProductTypeDAO dao = new ProductTypeDAO();
                    List<ProductType> validationList = dao.GetProductTypeList(Division);
                    List<ProductType> updateList = new List<ProductType>();
                    List<ProductType> errorList = new List<ProductType>();
                    ProductType updateProduct;
                    ProductType tempProduct;
                    string division;
                    string dept;
                    string productCode;
                    while (mySheet.Cells[row, 0].Value != null)
                    {
                        division = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(2, '0');
                        if (!(Footlocker.Common.WebSecurityService.UserHasDivision(User.Identity.Name.Split('\\')[1], "allocation", division)))
                        {
                            return Content("You are not authorized to update division " + division);
                        }
                        dept = Convert.ToString(mySheet.Cells[row, 1].Value).PadLeft(2, '0');
                        productCode = Convert.ToString(mySheet.Cells[row, 3].Value);
                        //validate the product type is okay for that div/dept
                        var command = (from a in validationList
                                       where ((a.Division == division)
                                           && ((a.Dept == dept) || (a.Dept == "00"))
                                           && (a.ProductTypeCode == productCode)
                                           )
                                       select a);
                        if (command.Count() > 0)
                        {
                            //save the update in list
                            tempProduct = command.First();
                            updateProduct = new ProductType();
                            updateProduct.Division = tempProduct.Division;
                            updateProduct.Dept = dept;
                            updateProduct.ProductTypeCode = tempProduct.ProductTypeCode;
                            updateProduct.ProductTypeID = tempProduct.ProductTypeID;
                            updateProduct.ProductTypeName = tempProduct.ProductTypeName;
                            updateProduct.StockNumber = Convert.ToString(mySheet.Cells[row, 2].Value).PadLeft(5, '0');
                            updateList.Add(updateProduct);
                            validCount++;
                        }
                        else
                        {
                            updateProduct = new ProductType();
                            updateProduct.Division = division;
                            updateProduct.Dept = dept;
                            updateProduct.ProductTypeCode = productCode;
                            updateProduct.StockNumber = Convert.ToString(mySheet.Cells[row, 2].Value).PadLeft(5, '0');
                            errorList.Add(updateProduct);
                            errorCount++;
                        }

                        row++;
                    }
                    dao.UpdateList(updateList);

                    if (errorCount > 0)
                    {
                        Session["errorList"] = errorList;
                        return Content(errorCount + " Errors on spreadsheet (" + validCount + " successfully uploaded)");
                    }
                }
                else
                {
                    // Inform of missing/bad header row
                    return Content("Incorrectly formatted or missing header row. Please correct and re-process.");
                }
            }
            
            return Content("");
        }


        public ActionResult DownloadProductErrors()
        {
            List<ProductType> errorList = new List<ProductType>();
            if (Session["errorList"] != null)
            {
                errorList = (List<ProductType>)Session["errorList"];
            }


            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ProductTypeTemplate"]), FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            Worksheet mySheet = excelDocument.Worksheets[0];
            int row = 1;
            foreach (ProductType p in errorList)
            {
                mySheet.Cells[row, 0].PutValue(p.Division);
                mySheet.Cells[row, 1].PutValue(p.Dept);
                mySheet.Cells[row, 2].PutValue(p.StockNumber);
                mySheet.Cells[row, 3].PutValue(p.ProductTypeCode);
                row++;
            }

            excelDocument.Save("ProductTypeUploadErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);           
            return View();

        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SaveNetwork(IEnumerable<HttpPostedFileBase> attachments)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            foreach (HttpPostedFileBase file in attachments)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                int row = 1;
                string store;
                string div;
                string zone;
                int wc, ch, jc, jv, ed, te;
                DateTime createdate = DateTime.Now;
                while (mySheet.Cells[row, 0].Value != null)
                {
                    store = Convert.ToString(mySheet.Cells[row, 0].Value);
                    div = Convert.ToString(mySheet.Cells[row, 1].Value);
                    zone = "Champs " + Convert.ToString(mySheet.Cells[row, 25].Value);
                    jc = GetIntransit(mySheet, row, 4);
                    jv = GetIntransit(mySheet, row, 6);
                    te = GetIntransit(mySheet, row, 8);
                    ed = GetIntransit(mySheet, row, 10);
                    ch = GetIntransit(mySheet, row, 12);
                    wc = GetIntransit(mySheet, row, 14);

                    List<StoreLeadTime> list = new List<StoreLeadTime>();
                    StoreLeadTime s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 1;
                    s.LeadTime = jc;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);

                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 2;
                    s.LeadTime = ch;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);

                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 3;
                    s.LeadTime = ed;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);

                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 4;
                    s.LeadTime = wc;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);

                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 5;
                    s.LeadTime = jv;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);

                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 6;
                    s.LeadTime = te;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);

                    list = list.OrderBy(o => o.LeadTime).ToList();
                    int rank = 1;
                    foreach (StoreLeadTime slt in list)
                    {
                        slt.Rank = rank;
                        db.StoreLeadTimes.Add(slt);
                        db.SaveChanges();
                        rank++;
                    }

                    row++;
                }
            }

            return Content("");
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SaveNetworkLocker(IEnumerable<HttpPostedFileBase> attachmentsLocker)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            foreach (HttpPostedFileBase file in attachmentsLocker)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                int row = 1;
                string store;
                string div;
                string zone;
                int wc, ch, jc, jv, ed, te;

                NetworkZone z = null;
                RouteDetail det;
                NetworkZoneStore nzstore;
                string prevZone = "";
                DateTime createdate = DateTime.Now;

                while (mySheet.Cells[row, 0].Value != null)
                {
                    store = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(5,'0');
                    div = Convert.ToString(mySheet.Cells[row, 1].Value).PadLeft(2,'0');
                    zone = "Locker " + Convert.ToString(mySheet.Cells[row, 12].Value);
                    jc = GetIntransit(mySheet, row, 7);
                    jv = GetIntransit(mySheet, row, 11);
                    te = GetIntransit(mySheet, row, 6);
                    ed = GetIntransit(mySheet, row, 9);
                    ch = GetIntransit(mySheet, row, 8);
                    wc = GetIntransit(mySheet, row, 10);

                    List<StoreLeadTime> list = new List<StoreLeadTime>();
                    StoreLeadTime s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 1;
                    s.LeadTime = jc;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);

                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 2;
                    s.LeadTime = ch;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);
                    
                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 3;
                    s.LeadTime = ed;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);

                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 4;
                    s.LeadTime = wc;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);
                    
                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 5;
                    s.LeadTime = jv;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);

                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 6;
                    s.LeadTime = te;
                    s.Active = true;
                    s.CreateDate = createdate;
                    s.CreatedBy = User.Identity.Name;
                    list.Add(s);

                    list = list.OrderBy(o => o.LeadTime).ToList();
                    int rank = 1;
                    foreach (StoreLeadTime slt in list)
                    {
                        slt.Rank = rank;
                        db.StoreLeadTimes.Add(slt);
                        db.SaveChanges();
                        rank++;
                    }

                    row++;
                }
            }

            return Content("");
        }


        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SaveNetworkEurope(IEnumerable<HttpPostedFileBase> attachmentsEurope)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            foreach (HttpPostedFileBase file in attachmentsEurope)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                int row = 1;
                string store;
                string div="31";
                string zone;
                int fri, mon, tue, thur, wed;

                NetworkZone z = null;
                RouteDetail det;
                NetworkZoneStore nzstore;
                string prevZone = "";

                while (mySheet.Cells[row, 0].Value != null)
                {
                    store = Convert.ToString(mySheet.Cells[row, 0].Value);
                    zone = "Europe " + Convert.ToString(mySheet.Cells[row, 10].Value);
                    mon = GetIntransitEurope(mySheet, row, 4);
                    tue = GetIntransitEurope(mySheet, row, 5);
                    wed = GetIntransitEurope(mySheet, row, 6);
                    thur = GetIntransitEurope(mySheet, row, 7);
                    fri = GetIntransitEurope(mySheet, row, 8);

                    if (prevZone != zone)
                    {
                        //create zone
                        prevZone = zone;
                        var zonequery = (from a in db.NetworkZones where a.Name == zone select a);
                        if (zonequery.Count() == 0)
                        {
                            z = new NetworkZone();
                            z.LeadTimeID = 7;
                            z.Name = zone;
                            z.CreatedBy = User.Identity.Name;
                            z.CreateDate = DateTime.Now;
                            db.NetworkZones.Add(z);
                            db.SaveChanges();
                        }
                        else
                        {
                            z = zonequery.First();
                        }
                    }
                    var stores = (from a in db.NetworkZoneStores where ((a.Store == store) && (a.Division == div)) select a);

                    if (stores.Count() == 0)
                    {
                        nzstore = new NetworkZoneStore();
                        nzstore.Division = div;
                        nzstore.Store = store;
                        nzstore.ZoneID = z.ID;
                        db.NetworkZoneStores.Add(nzstore);
                    }
                    else
                    {
                        nzstore = stores.First();
                        nzstore.ZoneID = z.ID;
                    }

                    db.SaveChanges();
                    //create route(s)

                    //hein route (for everyone)
                    var jcquery = (from a in db.RouteDetails where ((a.RouteID == 5) && (a.DCID == 1) && (a.ZoneID == z.ID)) select a);
                    if (jcquery.Count() == 0)
                    {
                        det = new RouteDetail();
                        det.RouteID = 38;
                        det.DCID = 7;
                        if (mon > 0)
                        {
                            det.Days = mon;
                        }
                        else if (tue > 0)
                        {
                            det.Days = tue;
                        }
                        else if (wed > 0)
                        {
                            det.Days = wed;
                        }
                        else if (thur > 0)
                        {
                            det.Days = thur;
                        }
                        else if (fri > 0)
                        {
                            det.Days = fri;
                        }
                        det.ZoneID = z.ID;

                        SaveRouteDetail(det, db);

                    }

                    row++;
                }
            }

            return Content("");
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SaveNetworkCanada(IEnumerable<HttpPostedFileBase> attachmentsCanada)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            foreach (HttpPostedFileBase file in attachmentsCanada)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                int row = 1;
                string store;
                string div;
                string zone;
                int lt;

                NetworkZone z = null;
                RouteDetail det;
                NetworkZoneStore nzstore;
                string prevZone = "";
                StoreLeadTime s;

                while (mySheet.Cells[row, 0].Value != null)
                {
                    store = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(5, ' ');
                    div = Convert.ToString(mySheet.Cells[row, 1].Value).PadLeft(2, ' ');
                    zone = "Canada " + Convert.ToString(mySheet.Cells[row, 9].Value);
                    lt = Convert.ToInt32(mySheet.Cells[row, 9].Value);

                    if (prevZone != zone)
                    {
                        //create zone
                        prevZone = zone;
                        var zonequery = (from a in db.NetworkZones where a.Name == zone select a);
                        if (zonequery.Count() == 0)
                        {
                            z = new NetworkZone();
                            z.LeadTimeID = 1;
                            z.Name = zone;
                            z.CreatedBy = User.Identity.Name;
                            z.CreateDate = DateTime.Now;
                            db.NetworkZones.Add(z);
                            db.SaveChanges();
                        }
                        else
                        {
                            z = zonequery.First();
                        }
                    }
                    var stores = (from a in db.NetworkZoneStores where ((a.Store == store) && (a.Division == div)) select a);

                    if (stores.Count() == 0)
                    {
                        nzstore = new NetworkZoneStore();
                        nzstore.Division = div;
                        nzstore.Store = store;
                        nzstore.ZoneID = z.ID;
                        db.NetworkZoneStores.Add(nzstore);
                    }
                    else
                    {
                        nzstore = stores.First();
                        nzstore.ZoneID = z.ID;
                    }

                    db.SaveChanges();

                    s = new StoreLeadTime();
                    s.Division = div;
                    s.Store = store;
                    s.DCID = 8;
                    s.LeadTime = lt;
                    s.Active = true;
                    s.CreateDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.Rank = 1;
                    db.StoreLeadTimes.Add(s);

                    db.SaveChanges();
                    
                    row++;
                }
            }

            return Content("");
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SaveBTSEurope(IEnumerable<HttpPostedFileBase> attachmentsEurope)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            foreach (HttpPostedFileBase file in attachmentsEurope)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                int row = 1;
                string store;
                string div = "31";
                string BTSGroup;
                StoreBTS header2011, header2012, header2013;
                header2011 = new StoreBTS();
                header2012 = new StoreBTS();
                header2013 = new StoreBTS();

                StoreBTSDetail det;
                string prevBTSGroup = "";

                while (mySheet.Cells[row, 0].Value != null)
                {
                    store = Convert.ToString(mySheet.Cells[row, 4].Value).PadLeft(5,'0');
                    BTSGroup = Convert.ToString(mySheet.Cells[row, 1].Value);

                    if (prevBTSGroup != BTSGroup)
                    {
                        //create BTS
                        prevBTSGroup = BTSGroup;
                        var query = (from a in db.StoreBTS where a.Name == BTSGroup select a);
                        header2011 = null;
                        header2012 = null;
                        header2013 = null;

                        if (query.Count() > 0)
                        {
                            foreach (StoreBTS s in query)
                            {
                                if (s.Year == 2011)
                                {
                                    header2011 = s;
                                }
                                else if (s.Year == 2012)
                                {
                                    header2012 = s;
                                }
                                else if (s.Year == 2013)
                                {
                                    header2013 = s;
                                }
                            }
                        }
                        if (header2011 == null)
                        {
                            header2011 = new StoreBTS();
                            header2011.Name = BTSGroup;
                            header2011.Year = 2011;
                            header2011.CreateDate = DateTime.Now;
                            header2011.CreatedBy = User.Identity.Name;
                            header2011.Division = div;
                            db.StoreBTS.Add(header2011);
                            db.SaveChanges();
                        }
                        if (header2012 == null)
                        {
                            header2012 = new StoreBTS();
                            header2012.Name = BTSGroup;
                            header2012.Year = 2012;
                            header2012.CreateDate = DateTime.Now;
                            header2012.CreatedBy = User.Identity.Name;
                            header2012.Division = div;
                            db.StoreBTS.Add(header2012);
                            db.SaveChanges();
                        }
                        if (header2013 == null)
                        {
                            header2013 = new StoreBTS();
                            header2013.Name = BTSGroup;
                            header2013.Year = 2013;
                            header2013.CreateDate = DateTime.Now;
                            header2013.Division = div;
                            header2013.CreatedBy = User.Identity.Name;
                            db.StoreBTS.Add(header2013);
                            db.SaveChanges();
                        }
                    }
                    //now create detail
                    det = new StoreBTSDetail();
                    det.CreateDate = DateTime.Now;
                    det.CreatedBy = User.Identity.Name;
                    det.Division = div;
                    det.GroupID = header2011.ID;
                    det.Store = store;
                    det.Year = 2011;
                    db.StoreBTSDetails.Add(det);
                    db.SaveChanges();

                    det = new StoreBTSDetail();
                    det.CreateDate = DateTime.Now;
                    det.CreatedBy = User.Identity.Name;
                    det.Division = div;
                    det.GroupID = header2012.ID;
                    det.Store = store;
                    det.Year = 2012;
                    db.StoreBTSDetails.Add(det);
                    db.SaveChanges();

                    det = new StoreBTSDetail();
                    det.CreateDate = DateTime.Now;
                    det.CreatedBy = User.Identity.Name;
                    det.Division = div;
                    det.GroupID = header2013.ID;
                    det.Store = store;
                    det.Year = 2013;
                    db.StoreBTSDetails.Add(det);
                    db.SaveChanges();

                    row++;
                }
            }

            return Content("");
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SaveSeasonEurope(IEnumerable<HttpPostedFileBase> attachmentsEurope)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            foreach (HttpPostedFileBase file in attachmentsEurope)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                int row = 1;
                string store;
                string div = "31";
                string Group;
                StoreSeasonality header;
                header = new StoreSeasonality();

                StoreSeasonalityDetail det;
                string prevGroup = "";

                while (mySheet.Cells[row, 0].Value != null)
                {
                    store = Convert.ToString(mySheet.Cells[row, 4].Value).PadLeft(5, '0');
                    Group = Convert.ToString(mySheet.Cells[row, 3].Value);

                    if (prevGroup != Group)
                    {
                        //create BTS
                        prevGroup = Group;
                        var query = (from a in db.StoreSeasonality where a.Name == Group select a);

                        if (query.Count() > 0)
                        {
                            header = query.First();
                        }
                        else
                        {
                            header = new StoreSeasonality();
                            header.Name = Group;
                            header.Division = "31";
                            header.CreatedBy = "Original upload";
                            header.CreateDate = DateTime.Now;
                            db.StoreSeasonality.Add(header);
                            db.SaveChanges();
                        }
                    }
                    //now create detail
                    det = new StoreSeasonalityDetail();
                    det.CreateDate = DateTime.Now;
                    det.CreatedBy = User.Identity.Name;
                    det.Division = div;
                    det.GroupID = header.ID;
                    det.Store = store;
                    db.StoreSeasonalityDetails.Add(det);
                    db.SaveChanges();

                    row++;
                }
            }

            return Content("");
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SaveBTSLocker(IEnumerable<HttpPostedFileBase> attachmentsLocker)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            foreach (HttpPostedFileBase file in attachmentsLocker)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                int row = 1;
                string store;
                string div = "31";
                string BTSGroup;
                StoreBTS header2011, header2012, header2013;
                header2011 = new StoreBTS();
                header2012 = new StoreBTS();
                header2013 = new StoreBTS();

                StoreBTSDetail det;
                string prevBTSGroup = "";

                while (mySheet.Cells[row, 0].Value != null)
                {
                    store = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(5, '0');
                    BTSGroup = "Group " + Convert.ToString(mySheet.Cells[row, 1].Value);
                    switch (Convert.ToString(mySheet.Cells[row, 6].Value))
                    { 
                        case "FOOT LOCKER":
                            div = "03";
                            break;
                        case "LADY FOOT LOCKER":
                            div = "08";
                            break;
                        case "KIDS FOOT LOCKER":
                            div = "16";
                            break;
                        case "FOOTACTION":
                            div = "29";
                            break;
                    }

                    //if ((prevBTSGroup != BTSGroup)||())
                    //{
                        //create BTS
                        prevBTSGroup = BTSGroup;
                        var query = (from a in db.StoreBTS where a.Division == div select a);
                        header2011 = null;
                        header2012 = null;
                        header2013 = null;

                        if (query.Count() > 0)
                        {
                            foreach (StoreBTS s in query)
                            {
                                if ((s.Year == 2011) && (s.Name == "Group " + Convert.ToString(mySheet.Cells[row, 3].Value)))
                                {
                                    header2011 = s;
                                }
                                else if ((s.Year == 2012) && (s.Name == "Group " + Convert.ToString(mySheet.Cells[row, 2].Value)))
                                {
                                    header2012 = s;
                                }
                                else if ((s.Year == 2013) && (s.Name == BTSGroup))
                                {
                                    header2013 = s;
                                }
                            }
                        }
                        if (header2011 == null)
                        {
                            header2011 = new StoreBTS();
                            header2011.Name = "Group " + Convert.ToString(mySheet.Cells[row, 3].Value);
                            header2011.Year = 2011;
                            header2011.CreateDate = DateTime.Now;
                            header2011.CreatedBy = User.Identity.Name;
                            header2011.Division = div;
                            db.StoreBTS.Add(header2011);
                            db.SaveChanges();
                        }
                        if (header2012 == null)
                        {
                            header2012 = new StoreBTS();
                            header2012.Name = "Group " + Convert.ToString(mySheet.Cells[row, 2].Value);
                            header2012.Year = 2012;
                            header2012.CreateDate = DateTime.Now;
                            header2012.CreatedBy = User.Identity.Name;
                            header2012.Division = div;
                            db.StoreBTS.Add(header2012);
                            db.SaveChanges();
                        }
                        if (header2013 == null)
                        {
                            header2013 = new StoreBTS();
                            header2013.Name = BTSGroup;
                            header2013.Year = 2013;
                            header2013.CreateDate = DateTime.Now;
                            header2013.Division = div;
                            header2013.CreatedBy = User.Identity.Name;
                            db.StoreBTS.Add(header2013);
                            db.SaveChanges();
                        }
                    //}
                    //now create detail
                    det = new StoreBTSDetail();
                    det.CreateDate = DateTime.Now;
                    det.CreatedBy = User.Identity.Name;
                    det.Division = div;
                    det.GroupID = header2011.ID;
                    det.Store = store;
                    det.Year = 2011;
                    db.StoreBTSDetails.Add(det);
                    db.SaveChanges();

                    det = new StoreBTSDetail();
                    det.CreateDate = DateTime.Now;
                    det.CreatedBy = User.Identity.Name;
                    det.Division = div;
                    det.GroupID = header2012.ID;
                    det.Store = store;
                    det.Year = 2012;
                    db.StoreBTSDetails.Add(det);
                    db.SaveChanges();

                    det = new StoreBTSDetail();
                    det.CreateDate = DateTime.Now;
                    det.CreatedBy = User.Identity.Name;
                    det.Division = div;
                    det.GroupID = header2013.ID;
                    det.Store = store;
                    det.Year = 2013;
                    db.StoreBTSDetails.Add(det);
                    db.SaveChanges();

                    row++;
                }
            }

            return Content("");
        }


        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SaveBTSChamps(IEnumerable<HttpPostedFileBase> attachmentsChamps)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            foreach (HttpPostedFileBase file in attachmentsChamps)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet;

                int row = 0;
                int col = 0;
                StoreBTS header;
                StoreBTSDetail det;
                int year = 2013;
                string name;
                string store;
                while (year < 2014)
                {
                    mySheet = workbook.Worksheets[year + " Groups"];
                    row = 0; 
                    col = 0;
                    while (mySheet.Cells[row, col].Value != null)
                    {
                        name = Convert.ToString(mySheet.Cells[row, col].Value);
                        var query = (from a in db.StoreBTS where ((a.Division == "18") && (a.Name == name) && (a.Year == year)) select a);

                        if (query.Count() > 0)
                        {
                            header = query.First();
                        }
                        else
                        {
                            header = new StoreBTS();
                            header.Year = year;
                            header.Name = name;
                            header.Division = "18";
                            header.CreatedBy = User.Identity.Name;
                            header.CreateDate = DateTime.Now;
                            db.StoreBTS.Add(header);
                            db.SaveChanges();
                        }
                        row++;
                        while (mySheet.Cells[row, col].Value != null)
                        {
                            store = Convert.ToString(mySheet.Cells[row, col].Value);
                            var detquery = (from a in db.StoreBTSDetails where ((a.GroupID == header.ID) && (a.Division == "18") && (a.Store == store)) select a);

                            if (detquery.Count() == 0)
                            {
                                det = new StoreBTSDetail();
                                det.GroupID = header.ID;
                                det.Year = year;
                                det.Store = store;
                                det.Division = "18";
                                det.CreatedBy = User.Identity.Name;
                                det.CreateDate = DateTime.Now;
                                db.StoreBTSDetails.Add(det);
                                db.SaveChanges();
                            }
                            row++;
                        }
                        col++;
                        row = 0;
                    }
                    year++;
                }
            }
            return Content("");
        }

        [CheckPermission(Roles = "Admin,Support")]
        public void SaveRouteDetail(RouteDetail det, Footlocker.Logistics.Allocation.DAO.AllocationContext db)
        {
            if (!((from a in db.RouteDetails where ((a.RouteID == det.RouteID) && (a.DCID == det.DCID) && (a.ZoneID == det.ZoneID)) select a).Count() > 0))
            {
                db.RouteDetails.Add(det);
                db.SaveChanges();
            }
        }

        [CheckPermission(Roles = "Admin,Support")]
        private int GetIntransitEurope(Aspose.Excel.Worksheet mySheet, int row, int col)
        {
            int intransit;
            try
            {
                intransit = Convert.ToInt32(mySheet.Cells[row, col].Value);
            }
            catch (Exception ex)
            {
                intransit = 9999;
            }
            return intransit;
        }

        [CheckPermission(Roles = "Admin,Support")]
        private int GetIntransit(Aspose.Excel.Worksheet mySheet, int row, int col)
        {
            int intransit;
            try
            {
                intransit = Convert.ToInt32(mySheet.Cells[row, col].Value);
            }
            catch (Exception ex)
            {
                intransit = 5;
            }
            return intransit;
        }


        #region Sku Id Upload

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ExcelSkuIdUploadTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SkuIdUploadTemplate"]), FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            excelDocument.Save("SkuIdUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            //return View();
            return View("SkuIdUpload");
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ExcelARSkusUploadTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ARSkusUploadTemplate"]), FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            excelDocument.Save("ARSkusUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            //return View();
            return View("ARSkusUpload");
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ExcelARConstraintsUploadTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ARConstraintsUploadTemplate"]), FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            excelDocument.Save("ARConstraintsUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            //return View();
            return View("ARConstraintsUpload");
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult SaveSkuId(IEnumerable<HttpPostedFileBase> attachments)
        {
            string divCodeOfCurrentSpreadsheet = String.Empty;

            //Set the license 
            Aspose.Excel.License license = new Aspose.Excel.License();
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            // Get and create configured temp, output text file if not existing
            string filePath = System.Configuration.ConfigurationManager.AppSettings["SkuIdFile"] + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmssffffff") + ".txt";
            string dirPath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
            if (!(new FileInfo(dirPath)).Directory.Exists) { System.IO.Directory.CreateDirectory(dirPath); }

            try
            {
                // Get writer to temp, output text file
                using (TextWriter txtWrite = new StreamWriter(filePath))
                {
                    // Burn through all uploaded spreadsheets
                    foreach (HttpPostedFileBase file in attachments)
                    {
                        //Instantiate a Workbook object that represents an Excel file
                        Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                        Byte[] data1 = new Byte[file.InputStream.Length];
                        file.InputStream.Read(data1, 0, data1.Length);
                        file.InputStream.Close();
                        MemoryStream memoryStream1 = new MemoryStream(data1);
                        workbook.Open(memoryStream1);
                        Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                        // Determine if the spreadsheet contains a valid header row
                        var hasValidHeaderRow = ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("SKU"))
                            && (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("Sku ID Code 1"))
                            && (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("Sku ID Code 2"))
                            && (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("Sku ID Code 3"))
                            && (Convert.ToString(mySheet.Cells[0, 4].Value).Contains("Sku ID Code 4"))
                            && (Convert.ToString(mySheet.Cells[0, 5].Value).Contains("Sku ID Code 5")));

                        // Validate that the template's header row exists... (else error out)
                        if (!hasValidHeaderRow) { return Content("Incorrect header, please use template."); }

                        // Get the sku's division
                        divCodeOfCurrentSpreadsheet = Convert.ToString(mySheet.Cells[1, 0].Value).PadLeft(14, '0').Substring(0, 2);

                        // Validate that the current user has access to the division that the sku is referencing... (else error out)
                        if (!(Footlocker.Common.WebSecurityService.UserHasDivision(User.Identity.Name.Split('\\')[1], "Allocation", divCodeOfCurrentSpreadsheet)))
                            { return Content("You do not have permission to update this division."); }

                        // Burn through each populated row of spreadsheet
                        int row = 1;
                        while (mySheet.Cells[row, 0].Value != null)
                        {
                            // Get and possibly transform data from spreadsheet for current row
                            var constructedFileLine = String.Empty;
                            var errorMsg = String.Empty;
                            if (!TryGetFileLineOfSkuIdSheetRow(mySheet.Cells, row, divCodeOfCurrentSpreadsheet, out constructedFileLine, out errorMsg))
                            {
                                // Display error message
                                return Content(errorMsg);
                            }

                            // Write constructed string for row to flat file (90 chars total per line)
                            txtWrite.WriteLine(constructedFileLine);

                            // Incrmenent loop var to process next row
                            row++;
                        }
                    }
                }

                // If configured, attempt to FTP the local, temp output file to the configured FTP server address
                if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["enableFTP"]))
                {
                    // NOTE: THIS LOGIC ALWAYS SAYS IF THE LAST PROCESSED SPREADSHEET'S DIVISION IS 31 THEN FTP ENTIRE OUTPUT TO EUROPE SERVER...
                    //            SO THIS IMPLIES THAT ONLY EUROPE DIVISION SKUS CANT BE UPDATED WITH USA AS PART OF SAME GROUP....(is current logic, is this ok? )
                    // Determine if we are using the US or Europe mainframe dataset, and get corresponding key
                    var dataSetKeyName = divCodeOfCurrentSpreadsheet.Equals(EuropeDivCode) ? "SkuIdDatasetEurope" : "SkuIdDataset";

                    // FTP file to mainframe dataset address
                    Footlocker.Common.Services.FTPService ftp = new Footlocker.Common.Services.FTPService(System.Configuration.ConfigurationManager.AppSettings["FTPServer"], System.Configuration.ConfigurationManager.AppSettings["SkuIdFTPUserName"], System.Configuration.ConfigurationManager.AppSettings["SkuIdFTPPassword"]);
                    //code replaced for FTP secure
                    //ftp.Connect(0, 0);
                    //ftp.SendFileToMainframe(filePath, System.Configuration.ConfigurationManager.AppSettings[dataSetKeyName], System.Configuration.ConfigurationManager.AppSettings["QuoteFTPCommand_SkuID"]);
                    //ftp.Quit();

                    ftp.FTPSToMainframe(filePath, System.Configuration.ConfigurationManager.AppSettings[dataSetKeyName], 0, 0, System.Configuration.ConfigurationManager.AppSettings["QuoteFTPCommand_SkuID"]);
                    ftp.Disconnect();
                }

                return Content("");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            finally
            {
                try
                {
                    // Delete the local, temp output file
                    System.IO.File.Delete(filePath);
                }
                catch { }
            }
        }

        [CheckPermission(
            Roles =
                "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support"
            )]
        public ActionResult SaveARSkus(IEnumerable<HttpPostedFileBase> attachments)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            string message = string.Empty;
            bool errorsFound = false;
            List<DirectToStoreSku> list = new List<DirectToStoreSku>();

            foreach (HttpPostedFileBase file in attachments)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                // Determine if the spreadsheet contains a valid header row
                var hasValidHeaderRow = ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("SKU"))
                    && (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("Start Date"))
                    && (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("End Date"))
                    && (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("Buying Multiple"))
                    && (Convert.ToString(mySheet.Cells[0, 4].Value).Contains("Order Sunday"))
                    && (Convert.ToString(mySheet.Cells[0, 5].Value).Contains("Order Monday"))
                    && (Convert.ToString(mySheet.Cells[0, 6].Value).Contains("Order Tuesday"))
                    && (Convert.ToString(mySheet.Cells[0, 7].Value).Contains("Order Wednesday"))
                    && (Convert.ToString(mySheet.Cells[0, 8].Value).Contains("Order Thursday")));

                // Validate that the template's header row exists... (else error out)
                if (!hasValidHeaderRow)
                {
                    errorsFound = true;
                    message = "Upload failed: Incorrect header - please use template.";
                }
                else
                {
                    int row = 1;
                    try
                    {
                        while (mySheet.Cells[row, 0].Value != null)
                        {
                            DirectToStoreSku item = new DirectToStoreSku();
                            item.Sku = Convert.ToString(mySheet.Cells[row, 0].Value).Trim();
                            item.StartDate = Convert.ToDateTime(mySheet.Cells[row, 1].Value);
                            item.EndDate = Convert.ToDateTime(mySheet.Cells[row, 2].Value);
                            item.VendorPackQty = Convert.ToInt32(mySheet.Cells[row, 3].Value);
                            item.OrderSun = Convert.ToBoolean(mySheet.Cells[row, 4].Value);
                            item.OrderMon = Convert.ToBoolean(mySheet.Cells[row, 5].Value);
                            item.OrderTue = Convert.ToBoolean(mySheet.Cells[row, 6].Value);
                            item.OrderWed = Convert.ToBoolean(mySheet.Cells[row, 7].Value);
                            item.OrderThur = Convert.ToBoolean(mySheet.Cells[row, 8].Value);

                            item.CreateDate = DateTime.Now;
                            item.CreatedBy = this.UserName;

                            list.Add(item);
                            row++;
                        }
                    }
                    catch (Exception)
                    {
                        errorsFound = true;
                        message = "Upload failed: One ore more columns has missing or invalid data.";
                    }                    
                }
            }

            // Validate divisions 
            if (!errorsFound)
            {
                var invalidList = (from a in list
                    where !this.Divisions().Any(p => p.DivCode == a.Division)
                    select a);

                if (invalidList.Any())
                {
                    errorsFound = true;
                    message = "Upload failed: One ore more Skus are in a division you do not have permissions for.";
                }
            }
            
            // Upload if spreadsheet has valid data
            if (!errorsFound)
            {
                try
                {
                    var dao = new DirectToStoreDAO();
                    dao.SaveARSkusUpload(list);
                }
                catch (Exception ex)
                {
                    message = "Upload failed: " + ex.Message;               
                }
            }

            return Content(message);
        }

        [CheckPermission(
            Roles =
                "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support"
            )]
        public ActionResult SaveARConstraints(IEnumerable<HttpPostedFileBase> attachments)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            string message = string.Empty;
            bool errorsFound = false;
            List<DirectToStoreConstraint> list = new List<DirectToStoreConstraint>();

            foreach (HttpPostedFileBase file in attachments)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                // Determine if the spreadsheet contains a valid header row
                var hasValidHeaderRow = ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("SKU"))
                    && (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("Size"))
                    && (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("Start Date"))
                    && (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("End Date"))
                    && (Convert.ToString(mySheet.Cells[0, 4].Value).Contains("Max Qty")));

                // Validate that the template's header row exists... (else error out)
                if (!hasValidHeaderRow)
                {
                    errorsFound = true;
                    message = "Upload failed: Incorrect header - please use template.";
                }

                int row = 1;
                try
                {
                    while (mySheet.Cells[row, 0].Value != null)
                    {
                        DirectToStoreConstraint item = new DirectToStoreConstraint();
                        item.Sku = Convert.ToString(mySheet.Cells[row, 0].Value).Trim();
                        item.Size = Convert.ToString(mySheet.Cells[row, 1].Value).Trim();
                        item.StartDate = Convert.ToDateTime(mySheet.Cells[row, 2].Value);
                        item.EndDate = Convert.ToDateTime(mySheet.Cells[row, 3].Value);
                        item.MaxQty = Convert.ToInt32(mySheet.Cells[row, 4].Value);

                        item.CreateDate = DateTime.Now;
                        item.CreatedBy = "ARConstraintsUpload";

                        list.Add(item);
                        row++;
                    }
                }
                catch (Exception)
                {
                    errorsFound = true;
                    message = "Upload failed: One ore more columns has missing or invalid data.";
                }
            }


            // Validate divisions 
            if (!errorsFound)
            {
                var invalidList = (from a in list
                                   where !this.Divisions().Any(p => p.DivCode == a.Division)
                                   select a);

                if (invalidList.Any())
                {
                    errorsFound = true;
                    message = "Upload failed: One ore more Skus are in a division you do not have permissions for.";
                }
            }

            // Upload if spreadsheet has valid data
            if (!errorsFound)
            {
                try
                {
                    var dao = new DirectToStoreDAO();
                    dao.SaveARConstraintsUpload(list);
                }
                catch (Exception ex)
                {
                    message = "Upload failed: " + ex.Message;
                }
            }

            return Content(message);
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult SkuIdUpload()
        {
            return View();
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ARSkusUpload()
        {
            return View();
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ARConstraintsUpload()
        {
            return View();
        }

        private bool TryGetFileLineOfSkuIdSheetRow(Cells spreadsheetCells, int rowIndex, string spreadsheetDivCode, out string constructedFileLine, out string errorMsg)
        {
            constructedFileLine = String.Empty;
            errorMsg = String.Empty;
            var isValid = false;

            try
            {
                // Get a string representing a line of text to write to the text file (to be sent to mainframe) from this row of the spreadsheet
                constructedFileLine = GetFileLineOfSkuIdSheetRow(spreadsheetCells, rowIndex, spreadsheetDivCode);

                // Flag as success
                isValid = true;
            }
            catch (Exception ex)
            {
                // Capture error message to be returned
                errorMsg = ex.Message;
            }

            return isValid;
        }

        private string GetFileLineOfSkuIdSheetRow(Cells spreadsheetCells, int rowIndex, string spreadsheetDivCode)
        {
            // Get and possibly transform data from spreadsheet for current row
            string[] tokens = Convert.ToString(spreadsheetCells[rowIndex, 0].Value).Split('-');
            var division = tokens[0].PadLeft(2, '0').Substring(0, 2).ToUpper();
            var dept = tokens[1].PadLeft(2, '0').Substring(0, 2).ToUpper();
            var sku = tokens[2].PadLeft(5, '0').Substring(0, 5).ToUpper();
            var wc = tokens[3].PadLeft(2, '0').Substring(0, 2).ToUpper();

            var rawSkuIdCode1 = Convert.ToString(spreadsheetCells[rowIndex, 1].Value).PadLeft(1, ' ')[0];
            var skuIdCode1 = (rawSkuIdCode1 > ASCIINumericThreshold) ? " " : rawSkuIdCode1.ToString().ToUpper();

            var rawSkuIdCode2 = Convert.ToString(spreadsheetCells[rowIndex, 2].Value).PadLeft(1, ' ')[0];
            var skuIdCode2 = (rawSkuIdCode2 > ASCIINumericThreshold) ? " " : rawSkuIdCode2.ToString().ToUpper();

            var rawSkuIdCode3 = Convert.ToString(spreadsheetCells[rowIndex, 3].Value).PadLeft(1, ' ')[0];
            var skuIdCode3 = (rawSkuIdCode3 > ASCIINumericThreshold) ? " " : rawSkuIdCode3.ToString().ToUpper();

            var rawSkuIdCode4 = Convert.ToString(spreadsheetCells[rowIndex, 4].Value).PadLeft(1, ' ')[0];
            var skuIdCode4 = (rawSkuIdCode4 > ASCIINumericThreshold) ? " " : rawSkuIdCode4.ToString().ToUpper();

            var rawSkuIdCode5 = Convert.ToString(spreadsheetCells[rowIndex, 5].Value).PadLeft(1, ' ')[0];
            var skuIdCode5 = (rawSkuIdCode5 > ASCIINumericThreshold) ? " " : rawSkuIdCode5.ToString().ToUpper();


            // Validate the same division
            if (!division.Equals(spreadsheetDivCode))
            {
                throw new InvalidOperationException("Spreadsheet must be for one division only.");
            }

            // Validate no invalid characters in sku id buckets
            if (skuIdCode1.Contains('|') || skuIdCode2.Contains('|') || skuIdCode3.Contains('|') || skuIdCode4.Contains('|') || skuIdCode5.Contains('|'))
            {
                throw new InvalidOperationException("Spreadsheet must not contain any '|' characters as Sku Id values.");
            }

            // Build string to write to flat file to represent this spreadsheet row
            var fileLineStringBuilder = new StringBuilder();
            fileLineStringBuilder.Append(division); // 2 chars
            fileLineStringBuilder.Append(dept); // 2 chars
            fileLineStringBuilder.Append(sku); // 5 chars
            fileLineStringBuilder.Append(wc); // 2 chars
            fileLineStringBuilder.Append(skuIdCode1); // 1 chars
            fileLineStringBuilder.Append(skuIdCode2); // 1 chars
            fileLineStringBuilder.Append(skuIdCode3); // 1 chars
            fileLineStringBuilder.Append(skuIdCode4); // 1 chars
            fileLineStringBuilder.Append(skuIdCode5); // 1 chars
            fileLineStringBuilder.Append(CurrentFormattedDateTimeString); // 26 chars
            fileLineStringBuilder.Append(UserName); // 30 chars
            fileLineStringBuilder.Append(FileLineFiller); // 18 chars

            return fileLineStringBuilder.ToString();
        }

        #endregion


    }
}
