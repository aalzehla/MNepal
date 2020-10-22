using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MNSuperadmin.Helper;
using MNSuperadmin.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static MNSuperadmin.Models.Notifications;
using DataTable = System.Data.DataTable;
using Excel = Microsoft.Office.Interop;
using UserInfo = MNSuperadmin.Models.UserInfo;

namespace MNSuperadmin.Controllers
{
    public class BulkSMSController : Controller
    {
        // GET: BulkSMS
        public ActionResult SendBulkSMS()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                ViewBag.SenderMobileNo = userName;

                if (this.TempData["registration_message"] != null)
                {
                    this.ViewData["registration_message"] = this.TempData["registration_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                int id = TraceIdGenerator.GetID() + 1;
                string stringid = (id).ToString();//this.GetID() + 1
                string traceID = stringid.PadLeft(11, '0') + 'W';
                ViewBag.TraceID = traceID;

                UserInfo userInfo = new UserInfo();

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public async Task<ActionResult> SendBulkSMS(HttpPostedFileBase excelUpload, string txtMsg)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;

            //file validation
            FileUpload fs = new FileUpload();
            fs.filesize = 550;
            string us = fs.UploadUserFile(excelUpload, txtMsg);
            if (us != null)
            {
                ViewBag.ResultErrorMessage = fs.ErrorMessage;
            }
            //file validation ends
            else
            {
                if (excelUpload.FileName.EndsWith("xls") || excelUpload.FileName.EndsWith("xlsx"))
                {
                    string basePath = string.Format("{0}/{1}", AppDomain.CurrentDomain.BaseDirectory, "FileRepository/ImportExport/Excel");
                    string path = string.Format("{0}/{1}", basePath, excelUpload.FileName);
                    if (!Directory.Exists(basePath))
                    {
                        Directory.CreateDirectory(basePath);
                    }
                    excelUpload.SaveAs(path);
                    List<NotificationModel> listProducts = new List<NotificationModel>();
                    using (SpreadsheetDocument doc = SpreadsheetDocument.Open(path, false))
                    {
                        //Read the first Sheet from Excel file.
                        Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
                        //Get the Worksheet instance.
                        Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
                        //Fetch all the rows present in the Worksheet.
                        IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                        //DataTable dt = new DataTable();
                        //Loop through the Worksheet rows.
                        foreach (Row row in rows)
                        {
                            //Use the first row to add columns to DataTable.
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                NotificationModel p = new NotificationModel();
                                p.CustomerNumber = (GetValue(doc, cell));
                                if (p.CustomerNumber != "" && p.CustomerNumber.Substring(0, 2) == "98" && p.CustomerNumber.Length == 10)
                                {
                                    p.Message = txtMsg;
                                    listProducts.Add(p);
                                }

                            }
                        }
                    }

                    var payload = new RootObject
                    {
                        notificationsList = listProducts
                    };
                    int responseCode = 0;
                    string message = string.Empty;
                    string responsetext = "Unauthorized";
                    bool result = false;
                    var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(payload));

                    // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                    var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                    var bulkSMSUrl = ConfigurationManager.AppSettings["BulkSMSUrl"];
                    using (var httpClient = new HttpClient())
                    {
                        var BasicAuthUserName = ConfigurationManager.AppSettings["BasicAuthUserName"];
                        var BasicAuthPassword = ConfigurationManager.AppSettings["BasicAuthPassword"];
                        var byteArray = new UTF8Encoding().GetBytes(BasicAuthUserName + ":" + BasicAuthPassword);
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        var httpResponse = await httpClient.PostAsync(bulkSMSUrl, httpContent);
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            try
                            {
                                result = true;
                                responseCode = (int)httpResponse.StatusCode;
                                responsetext = await httpResponse.Content.ReadAsStringAsync();
                                message = httpResponse.Content.ReadAsStringAsync().Result;
                                string respmsg = "";
                                if (string.IsNullOrEmpty(message))
                                {
                                    int code = 200;
                                    if (responseCode == 200)
                                    {
                                        return RedirectToAction("Index", "SuperAdminDashboard");
                                    }
                                    respmsg = "Success";
                                    if (code != responseCode)
                                    {
                                        responseCode = code;
                                    }
                                }
                                return Json(new { responseCode = responseCode, responseText = respmsg },
                                JsonRequestBehavior.AllowGet);


                            }
                            catch (Exception ex)
                            {
                                return Json(new { responseCode = "400", responseText = ex.Message },
                                    JsonRequestBehavior.AllowGet);
                            }
                        }

                        else
                        {
                            result = false;
                            responseCode = (int)httpResponse.StatusCode;
                            if (responsetext == null)
                            {
                                return Json(new { responseCode = responseCode, responseText = responsetext },
                            JsonRequestBehavior.AllowGet);
                            }
                            else
                            {

                                return Json(new { responseCode = responseCode, responseText = responsetext },
                                JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.Error("File type is incorrect");
                    return View("Index");
                }
            }
            return View();


            string GetValue(SpreadsheetDocument doc, Cell cell)
            {
                string value = string.Empty;
                if (cell.CellValue != null)
                {
                    value = cell.CellValue.InnerText;
                    if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                    {
                        return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
                    }
                }
                return value;
            }

        }


    }
}