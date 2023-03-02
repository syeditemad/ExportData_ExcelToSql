using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImportData_InExcel.Models;
using OfficeOpenXml;
using System.Configuration;
using System.Net.Mime;

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
        public ActionResult ExportData(FormCollection formCollection)
        {
            int rowNumber = 0;
            try
            {
                //var Export_Data = new List<Vendor_inventory_Upload>();
                DateTime inventoryDate = Convert.ToDateTime(formCollection["invtDate"]);
                DataTable InventoryDataTable = new DataTable("Inventory");
                //Add Columns to the Data Table as per the columns defined in the Table Type Parameter
                DataColumn Id = new DataColumn("vendor_id");
                InventoryDataTable.Columns.Add(Id);
                DataColumn Name = new DataColumn("Item_Code");
                InventoryDataTable.Columns.Add(Name);
                DataColumn Email = new DataColumn("Item_Name");
                InventoryDataTable.Columns.Add(Email);
                DataColumn Mobile = new DataColumn("UOM");
                InventoryDataTable.Columns.Add(Mobile);
                DataColumn itemType = new DataColumn("Item_Type");
                InventoryDataTable.Columns.Add(itemType);
                DataColumn closingStock = new DataColumn("Closing_Stock");
                InventoryDataTable.Columns.Add(closingStock);
                DataColumn invtDate = new DataColumn("DATE");
                InventoryDataTable.Columns.Add(invtDate);
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
                                rowNumber = rowIterator;
                                var excelData_Import = new Vendor_inventory_Upload();
                                excelData_Import.vendor_id = (workSheet.Cells[rowIterator, 1].Value != null) ? workSheet.Cells[rowIterator, 1].Value.ToString() : string.Empty;
                                excelData_Import.Item_Code = (workSheet.Cells[rowIterator, 2].Value!=null)? workSheet.Cells[rowIterator, 2].Value.ToString():string.Empty;
                                excelData_Import.Item_Name = (workSheet.Cells[rowIterator, 3].Value!=null)? workSheet.Cells[rowIterator,3].Value.ToString():string.Empty;
                                excelData_Import.Unit_Of_Measurement = (workSheet.Cells[rowIterator, 4].Value!=null) ? workSheet.Cells[rowIterator, 4].Value.ToString() : string.Empty;
                                excelData_Import.Item_Type = (workSheet.Cells[rowIterator, 5].Value!=null) ? workSheet.Cells[rowIterator, 5].Value.ToString(): string.Empty;
                               // excelData_Import.Closing_Stock =Convert.ToInt32(Convert.ToDouble(workSheet.Cells[rowIterator, 6].Value!=null ? workSheet.Cells[rowIterator,6].Value)* 100);
                                excelData_Import.Closing_Stock = Convert.ToInt32(Convert.ToDouble(workSheet.Cells[rowIterator, 6].Value) * 100);
                                //string Date= ((workSheet.Cells[rowIterator, 7]).Value.ToString());
                                //excelData_Import.C_date = DateTime.ParseExact(Date, "M/d/yyyy h:mm", CultureInfo.InvariantCulture);
                                // DateTime Date = ((workSheet.Cells[rowIterator, 7]).Value);
                                //long numDate = long.Parse(workSheet.Cells[rowIterator, 7].Value.ToString());
                                // excelData_Import.UploadDate = DateTime.FromOADate(numDate);
                                excelData_Import.UploadDate = inventoryDate;
                                InventoryDataTable.Rows.Add(excelData_Import.vendor_id, excelData_Import.Item_Code, excelData_Import.Item_Name, excelData_Import.Unit_Of_Measurement, excelData_Import.Item_Type, excelData_Import.Closing_Stock, inventoryDate);
                                //Export_Data.Add(excelData_Import);
                            }
                        }
                    }
                }
                string connectionString = ConfigurationManager.ConnectionStrings["BulkCopy"].ToString();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.Vendor_inventory_Upload";
                        // sqlBulkCopy.ColumnMappings.Add("InventoryId", "InventoryId");
                        sqlBulkCopy.ColumnMappings.Add("vendor_id", "vendor_id");
                        sqlBulkCopy.ColumnMappings.Add("Item_Code", "Item_Code");
                        sqlBulkCopy.ColumnMappings.Add("Item_Name", "Item_Name");
                        sqlBulkCopy.ColumnMappings.Add("UOM", "Unit_Of_Measurement");
                        sqlBulkCopy.ColumnMappings.Add("Item_Type", "Item_Type");
                        sqlBulkCopy.ColumnMappings.Add("Closing_Stock", "Closing_Stock");
                        //sqlBulkCopy.ColumnMappings.Add("Closing_Stock", Abs);
                        sqlBulkCopy.ColumnMappings.Add("DATE", "UploadDate");
                        connection.Open();
                        sqlBulkCopy.WriteToServer(InventoryDataTable);
                        ViewBag.Message = "File Data Insert Successflly!";
                    }
                }
            }
            catch (FormatException fex)
            {
                ViewBag.Error = "Format exception occured. Please check entry at row number " + rowNumber;
            }
            catch (Exception ex)
            {
                ViewBag.Error = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
            }
            //using (Sql_PractiseEntities1 excelImportDBEntities = new Sql_PractiseEntities1())
            //{
            //    foreach (var item in Export_Data)
            //    {
            //        excelImportDBEntities.Vendor_inventory_Upload.Add(item);
            //    }
            //    excelImportDBEntities.SaveChanges();
            //}
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection formCollection)
        {
            if (ModelState.IsValid)
            {
                HttpPostedFileBase fileBase = Request.Files["UploadedFile"];
                DateTime inventoryDate = Convert.ToDateTime(formCollection["invtDate"]);
                string Abs = Convert.ToString(inventoryDate);
                string path = Server.MapPath("~/Content/Upload/" + fileBase.FileName);
                fileBase.SaveAs(path);
                string excelConnectionString = @"Provider='Microsoft.ACE.OLEDB.12.0';Data Source='" + path + "';Extended Properties='Excel 12.0 Xml;IMEX=1'";
                OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                excelConnection.Open();
                string tableName = excelConnection.GetSchema("Tables").Rows[0]["TABLE_NAME"].ToString();
                excelConnection.Close();
                DataTable dataTable = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter("Select * from [" + tableName + "]", excelConnection);
                adapter.Fill(dataTable);
                string connectionString = ConfigurationManager.ConnectionStrings["BulkCopy"].ToString();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.Vendor_inventory_Upload";
                       // sqlBulkCopy.ColumnMappings.Add("InventoryId", "InventoryId");
                        sqlBulkCopy.ColumnMappings.Add("vendor_id", "vendor_id");
                        sqlBulkCopy.ColumnMappings.Add("Item_Code", "Item_Code");
                        sqlBulkCopy.ColumnMappings.Add("Item_Name", "Item_Name");
                        sqlBulkCopy.ColumnMappings.Add("UOM", "Unit_Of_Measurement");
                        sqlBulkCopy.ColumnMappings.Add("Item_Type", "Item_Type");
                        sqlBulkCopy.ColumnMappings.Add("Closing_Stock", "Closing_Stock");
                        //sqlBulkCopy.ColumnMappings.Add("Closing_Stock", Abs);
                        sqlBulkCopy.ColumnMappings.Add("DATE", Abs);
                        connection.Open();
                        sqlBulkCopy.WriteToServer(dataTable);
                    }

                }


            }
            return View();
        }

        [HttpGet]
        public ActionResult  UploadFile()
        {
            string path = Server.MapPath("~/Content/Upload/Inventory_Sheet.xlsx");

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", "template.xlsx");
        }

        [HttpGet]
        public ActionResult ReportList(string Filter_value)
        {
            Sql_PractiseEntities1 db = new Sql_PractiseEntities1();
            if (string.IsNullOrEmpty(Filter_value))
            {
                
                List<Vendor_inventory_Upload> vendorList = db.Vendor_inventory_Upload.ToList();
                return View(vendorList);
            }
            else
            {
                DateTime date_P = Convert.ToDateTime(Filter_value);
                TempData["date"] =date_P;
                List<Vendor_inventory_Upload> vendorList = db.Vendor_inventory_Upload.Where(x => x.UploadDate == date_P).ToList();
                if (vendorList.Count == 0)
                {
                    ViewBag.Message = " Inventory Details Not Founded ";
                    return View(vendorList);
                }
                else
                {
                    return View(vendorList);
                }
            }
        }
    }

}
