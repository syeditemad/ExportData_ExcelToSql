using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImportData_InExcel.Models;
using OfficeOpenXml;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace ImportData_InExcel.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }



        /// <summary>
        /// Exporting Data From Excel to Sql Database 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ExportData()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ExportData(FormCollection  formCollection)
        {
            var Export_Data = new List<ExcelData_Import>();
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if ((file != null) && (file.ContentLength > 0) && !String.IsNullOrEmpty(file.FileName))
                {
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    using (var Package = new ExcelPackage(file.InputStream))
                    {

                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var currentSheet = Package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;
                        for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                        {
                            var excelData_Import = new ExcelData_Import();
                            excelData_Import.Id = Convert.ToInt32(workSheet.Cells[rowIterator, 1].Value);
                            excelData_Import.first_name = workSheet.Cells[rowIterator, 2].Value.ToString();
                            excelData_Import.Last_name = workSheet.Cells[rowIterator, 3].Value.ToString();
                            excelData_Import.email = workSheet.Cells[rowIterator, 4].Value.ToString();
                            excelData_Import.gender = workSheet.Cells[rowIterator, 5].Value.ToString();
                            excelData_Import.ip_address = workSheet.Cells[rowIterator, 6].Value.ToString();
                            //excelData_Import.ip_address = Convert.ToInt32(workSheet.Cells[rowIterator, 3].Value);
                            Export_Data.Add(excelData_Import);
                        }
                    }

                }
            }
            using (Sql_PractiseEntities excelImportDBEntities = new Sql_PractiseEntities())
            {
                foreach (var item in Export_Data)
                {
                    excelImportDBEntities.ExcelData_Import.Add(item);
                }
                excelImportDBEntities.SaveChanges();
            }
                return View("Index");
        }
    }

}
