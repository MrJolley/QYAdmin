using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QY.Admin.Logic;
using QY.Admin.Logic.Models;
using System.Data;
using System.Text;
using System.IO;
using QY.Admin.Logic.ViewModels;
using Newtonsoft.Json;

namespace QY.Admin.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        #region 工资邮件功能请求
        [Authorize(Roles = "Admin")]
        public ActionResult SalaryMail()
        {
            return View(new SalaryFileModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult SalaryMail(SalaryFileModel sfViewModel)
        {
            if (ModelState.IsValid)
            {
                ViewBag.salaryDetail = false;
                if (sfViewModel.FileLoaded.ContentLength == 0)
                {
                    ViewBag.ErrorMsg = "The upload file is empty.";
                    return View();
                }
                string month, year;
                string tempFileName = Path.GetTempFileName();
                string[] arr = sfViewModel.Date.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                month = arr[0];
                year = arr[1];
                try
                {
                    SalaryUploadService fileService = new SalaryUploadService(sfViewModel.FileLoaded.InputStream, sfViewModel.FileLoaded.FileName, year, month);
                    SalaryFileModel salaryService = fileService.ReadFile();
                    sfViewModel.SalaryData = salaryService.SalaryData;
                    ViewBag.salaryDetail = true;
                    return View(sfViewModel);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMsg = ex.Message;
                }
                return View();
            }
            else
            {
                StringBuilder sbErrors = new StringBuilder();
                foreach (var key in ModelState.Values)
                {
                    foreach (var values in key.Errors)
                    {
                        sbErrors.AppendLine(string.Format("{0}", values.ErrorMessage));
                    }
                }
                ViewBag.ErrorMsg = sbErrors;
                return View();
            }
        }

        public JsonResult SendMail(string[] head, string[][] body, string sender, string password, string date, string description)
        {
            DataTable salaryTable = ConvertTable(head, body);
            List<string> successNames = new List<string>();
            List<string> failureNames = new List<string>();

            MailService mailService = new MailService(sender, password, date);
            var result = mailService.MailSender(salaryTable, description);
            return Json(result);
        }

        public static DataTable ConvertTable(string[] ColumnNames, string[][] Arrays)
        {
            DataTable dt = new DataTable();

            foreach (string ColumnName in ColumnNames)
            {
                dt.Columns.Add(ColumnName, typeof(string));
            }

            for (int i = 0; i < Arrays.GetLength(0); i++)
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < ColumnNames.Length; j++)
                {
                    dr[j] = Arrays[i][j].ToString();
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        #endregion

        #region 调休邮件功能请求
        [Authorize(Roles = "Admin")]
        public ActionResult TransferMail()
        {
            return View(new UserTransferList());
        }

        /// <summary>
        /// 调休，节假日excel上传服务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult HolidayFileHandler()
        {
            object responseData = new object();
            string ErrorMsg = "";

            if (Request.Files.Count == 0)
            {
                ErrorMsg = "上传失败：请选择上传的文件，文件不能为空";
                return Json(ErrorMsg);
            }
            HttpPostedFileBase file = Request.Files[0];
            if (file.ContentLength > 0)
            {
                string fileExtend = Path.GetExtension(file.FileName);
                if (fileExtend.ToLower().StartsWith(".xls"))
                {
                    string fileType = Request.Form["fileType"].ToLower();
                    Stream stream = file.InputStream;
                    //年假数据读取
                    if (fileType.Equals("annual"))
                    {
                        FileUploadHelper helper = new FileUploadHelper(stream, file.FileName, "");
                        var result = helper.ReadHolidayFile();
                        if (result.hasError)
                        {
                            ErrorMsg = result.ErrorMsg;
                            return Json(ErrorMsg);
                        }
                        else
                        {
                            //响应读取的数据结构
                            responseData = new
                            {
                                data = result.result
                            };
                            return Json(responseData);
                        }
                    }
                    else if (fileType.Equals("transfer"))
                    {
                        FileUploadHelper helper = new FileUploadHelper(stream, file.FileName, "");
                        var result = helper.ReadTransferFile();
                        if (result.hasError)
                        {
                            ErrorMsg = result.ErrorMsg;
                            return Json(ErrorMsg);
                        }
                        else
                        {
                            //响应读取的数据结构
                            responseData = new
                            {
                                type = fileType,
                                data = result.result
                            };
                            return Json(responseData);
                        }
                    }
                    else
                    {
                        ErrorMsg = "上传失败：文件类型有误，请至少选择一种文件类型";
                        return Json(ErrorMsg);
                    }
                }
                else
                {
                    ErrorMsg = "上传失败：请选择正确的文件类型，文件类型只能为.xls/.xlsx";
                    return Json(ErrorMsg);
                }
            }
            else
            {
                ErrorMsg = "上传失败：请选择正确的文件，文件内容不能为空";
                return Json(ErrorMsg);
            }
        }

        /// <summary>
        /// 调休，节假日邮件发送服务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult MailSendingHandler()
        {
            string email = Request.Form["email"];
            string password = Request.Form["password"];
            string type = Request.Form["mailType"];

            var service = new HolidayTransferService();
            MailResult result = new MailResult();
            switch (type)
            {
                case "annual":
                    List<HolidayDetail> rlt = JsonConvert.DeserializeObject<List<HolidayDetail>>(Request.Form["staffList"]);
                    var available = rlt.Where(r => r.RemainingTotalDuration > 0 && 
                    Convert.ToDateTime(r.HolidayEndDate) >= DateTime.Today &&
                    Convert.ToDateTime(r.HolidayEndDate).Subtract(DateTime.Today).Days <= 122).ToList();
                    foreach (var item in available)
                    {
                        string htmlData = string.Empty;
                        try
                        {
                            htmlData = service.HolidayDataConvert2Html(item);
                        }
                        catch (Exception ex)
                        {
                            result.FailureList.Add(item.ChineseName);
                            result.FailureMsg.Add(ex.Message);
                            continue;
                        }
                        try
                        {
                            service.MailSending(email, password, item.Email, htmlData, "annual");
                            result.SuccessList.Add(item.ChineseName);
                        }
                        catch (Exception ex)
                        {
                            result.FailureList.Add(item.ChineseName);
                            result.FailureMsg.Add(ex.Message);
                        }
                    }
                    break;
                case "transfer":
                    List<UserTransferList> lutl = JsonConvert.DeserializeObject<List<UserTransferList>>(Request.Form["staffList"]);
                    foreach (UserTransferList ul in lutl)
                    {
                        string htmlData = string.Empty;
                        var details = ul.UserTransferDetail.AsEnumerable().Where(r => r.TransferRemainingTime != 0);
                        if (details.Count() == 0)
                        {
                            continue;
                        }
                        try
                        {
                            htmlData = service.TransferDataConvert2Html(ul);
                        }
                        catch (Exception ex)
                        {
                            result.FailureList.Add(ul.StaffName);
                            result.FailureMsg.Add(ex.Message);
                            continue;
                        }
                        try
                        {
                            service.MailSending(email, password, ul.StaffEmail, htmlData, "transfer");
                            result.SuccessList.Add(ul.StaffName);
                        }
                        catch (Exception ex)
                        {
                            result.FailureList.Add(ul.StaffName);
                            result.FailureMsg.Add(ex.Message);
                        }
                    }
                    break;
                default:
                    break;
            }
            return Json(result);

        }

        #endregion

        #region 年假邮件功能请求
        [Authorize(Roles = "Admin")]
        public ActionResult HolidayMail()
        {
            return View(new HolidayDetail());
        }
        #endregion

    }
}