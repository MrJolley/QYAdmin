using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QY.Admin.Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QY.Admin.Logic
{
    public class FileUploadHelper
    {
        #region private field
        private Stream _fileStream;
        private IWorkbook _workBook;
        string _fileName;
        const string _chineseLocationName = "姓名";
        private string _loginName;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileName">文件名，包括后缀</param>
        public FileUploadHelper(Stream stream, string fileName, string loginName)
        {
            this._fileStream = stream;
            this._fileName = fileName;
            this._loginName = loginName;
            BuildWorkbook();
        }

        /// <summary>
        /// 读取当前年假，上一年年假结余的excel信息
        /// </summary>
        //public HolidayResult ReadHolidayFile()
        //{
        //    var sheet = this._workBook.GetSheetAt(0);
        //    if (sheet == null)
        //    {
        //        return new HolidayResult()
        //        {
        //            hasError = true,
        //            ErrorMsg = "当前文件无工作表数据"
        //        };
        //    }
        //    IRow title = sheet.GetRow(0);
        //    if (!title.GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim().Equals("Name"))
        //    {
        //        return new HolidayResult()
        //        {
        //            hasError = true,
        //            ErrorMsg = "当前文件数据格式不正确，请确保'A1'单元格为数据起始点"
        //        };
        //    }
        //    //读取所有年假信息
        //    int cols = title.LastCellNum;
        //    List<UserHolidayList> result = new List<UserHolidayList>();
        //    UserHolidayList list = new UserHolidayList();
        //    try
        //    {

        //        for (int row = 1; row < sheet.LastRowNum + 1; row++)
        //        {
        //            IRow sRow = sheet.GetRow(row);
        //            string name = sheet.GetRow(row).GetCell(0).ToString();
        //            bool nameExt = name != null && name != ""; //名字单元格不为空
        //            //确保第一个名字单元格存在
        //            if (row == 1 && !nameExt)
        //            {
        //                return new HolidayResult()
        //                {
        //                    hasError = true,
        //                    ErrorMsg = "当前文件数据格式不正确，请确保'A2'单元格名字数据不能为空"
        //                };
        //            }
        //            if (nameExt)
        //            {
        //                if (row != 1)
        //                {
        //                    result.Add(list);
        //                }
        //                list = new UserHolidayList()
        //                {
        //                    StaffName = name,
        //                };
        //            }
        //            //读取所有基本信息详情
        //            list.UserHolidayDetail.Add(
        //                new HolidayDetail()
        //                {
        //                    PaidLeavePeriod = sRow.GetCell(1, MissingCellPolicy.CREATE_NULL_AS_BLANK).StringCellValue.ToString(),
        //                    PaidLeaveHours = double.Parse(sRow.GetCell(2, MissingCellPolicy.CREATE_NULL_AS_BLANK).NumericCellValue.ToString().Trim()),
        //                    PaidLeaveRemainingHours = double.Parse(sRow.GetCell(3, MissingCellPolicy.CREATE_NULL_AS_BLANK).NumericCellValue.ToString().Trim()),
        //                    ExpirationDate = sRow.GetCell(4, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(),
        //                    PostponedDate = sRow.GetCell(5, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(),
        //                });
        //            //数据读取结束，返回result
        //            if (row == sheet.LastRowNum)
        //            {
        //                result.Add(list);
        //                break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new HolidayResult()
        //        {
        //            hasError = true,
        //            ErrorMsg = ex.Message,
        //        };
        //    }
        //    foreach (var lst in result)
        //    {
        //        foreach (var holiday in lst.UserHolidayDetail)
        //        {
        //            DateTime expire = DateTime.MinValue;
        //            DateTime postpone = DateTime.MinValue;
        //            if (DateTime.TryParse(holiday.ExpirationDate, out expire))
        //            {
        //                holiday.ExpirationDate = expire.ToString("MM/dd/yyyy");
        //            }
        //            if (DateTime.TryParse(holiday.PostponedDate, out postpone))
        //            {
        //                holiday.PostponedDate = postpone.ToString("MM/dd/yyyy");
        //            }
        //        }
        //    }
        //    return new HolidayResult()
        //    {
        //        hasError = false,
        //        ErrorMsg = "",
        //        result = result
        //    };
        //}

        /// <summary>
        /// 读取所有的调休excel信息
        /// </summary>
        public TransferResult ReadTransferFile()
        {
            var num = this._workBook.NumberOfSheets;
            List<UserTransferList> result = new List<UserTransferList>();
            while (num > 0)
            {
                var sheet = this._workBook.GetSheetAt(num - 1);
                if (sheet == null)
                {
                    return new TransferResult()
                    {
                        hasError = true,
                        ErrorMsg = $"错误定位：工作表=>{sheet.SheetName},<br />" +
                        $"错误信息：当前工作表无数据"
                    };
                }
                IRow title = sheet.GetRow(0);
                if (!title.GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim().Equals("Analyst Involved"))
                {
                    return new TransferResult()
                    {
                        hasError = true,
                        ErrorMsg = $"错误定位：工作表=>{sheet.SheetName},<br />" +
                        $"错误信息：当前工作表数据格式不正确，请确保'A1'单元格为数据起始点，其值为'Analyst Involved'"
                    };
                }
                //读取所有调休信息
                int cols = title.LastCellNum;
                UserTransferList list = new UserTransferList();
                var allUsers = new UserService().GetAllUsers();
                try
                {
                    for (int row = 1; row < sheet.LastRowNum + 1; row++)
                    {
                        IRow sRow = sheet.GetRow(row);
                        string name = sheet.GetRow(row).GetCell(0).ToString();
                        bool nameExt = name != null && name != ""; //名字单元格不为空
                                                                   //确保第一个名字单元格存在
                        if (nameExt)
                        {
                            var user = allUsers.Where(r => r.EnglishName.ToLower() == name.ToLower().Trim());
                            if (user.Count() > 0)
                            {
                                var userModel = user.FirstOrDefault();
                                try
                                {
                                    //读取所有基本信息详情
                                    var staffRecord = result.Where(r => r.StaffName.ToLower() == name.ToLower());
                                    //读取公式列（剩余调休时间）
                                    var specialCol = sRow.GetCell(6, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    double val = 0.0;
                                    if (specialCol.CellType == CellType.Formula)
                                    {
                                        val = specialCol.NumericCellValue;
                                    }
                                    else
                                    {
                                        val = double.Parse(specialCol.ToString().Trim());
                                    }
                                    string date = sRow.GetCell(1, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                                    if (DateTime.TryParse(date, out DateTime transDate))
                                    {
                                        date = transDate.ToString("yyyy-MM-dd");
                                    }
                                    if (result.Count > 0 && staffRecord.Count() > 0)
                                    {
                                        staffRecord.First().UserTransferDetail.Add(
                                        new TransferDetail()
                                        {
                                            ExtraWorkDate = date,
                                            ExtraWorkPeriod = sRow.GetCell(2, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(),
                                            ExtraWorkTime = double.Parse(sRow.GetCell(3, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()),
                                            TransferPeriod = sRow.GetCell(4, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(),
                                            TransferRemainingTime = val,
                                            MaturityDate = DateTime.Parse(sRow.GetCell(8, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()).ToString("yyyy-MM-dd")
                                        });
                                    }
                                    else
                                    {
                                        var detail = new TransferDetail()
                                        {
                                            ExtraWorkDate = date,
                                            ExtraWorkPeriod = sRow.GetCell(2, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(),
                                            ExtraWorkTime = double.Parse(sRow.GetCell(3, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()),
                                            TransferPeriod = sRow.GetCell(4, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(),
                                            TransferRemainingTime = val,
                                            MaturityDate = DateTime.Parse(sRow.GetCell(8, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()).ToString("yyyy-MM-dd")
                                        };
                                        var userlist = new UserTransferList()
                                        {
                                            StaffName = name,
                                            StaffEmail = userModel.EmailAddress,
                                        };
                                        userlist.UserTransferDetail.Add(detail);
                                        result.Add(userlist);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return new TransferResult()
                                    {
                                        hasError = true,
                                        ErrorMsg = $"错误定位：工作表=>{sheet.SheetName}, 姓名=>{name}, <br />" +
                                $"错误信息：当前文件字段类型有误，无法转换字段: {ex.Message}" +
                                $"错误追踪：{ex.StackTrace}"
                                    };
                                }
                            }
                            else
                            {
                                return new TransferResult()
                                {
                                    hasError = true,
                                    ErrorMsg = $"错误定位：工作表=>{sheet.SheetName}, 姓名=>{name}, <br />" +
                                "错误信息：当前文件姓名有误，无法匹配数据库员工字段"
                                };
                            }
                        }
                        //数据读取结束，返回result
                    }
                }
                catch (Exception ex)
                {
                    return new TransferResult()
                    {
                        hasError = true,
                        ErrorMsg = $"错误定位：工作表=>{sheet.SheetName}, <br />" +
                            $"错误追踪：{ex.StackTrace}"
                    };
                }
                num--;
            }
            return new TransferResult()
            {
                hasError = false,
                ErrorMsg = "",
                result = result
            };
        }

        private void BuildWorkbook()
        {
            string fileExt = Path.GetExtension(_fileName).ToLower();
            if (fileExt == ".xls")
                this._workBook = new HSSFWorkbook(_fileStream);
            else if (fileExt == ".xlsx")
                this._workBook = new XSSFWorkbook(_fileStream);
            else
                throw new Exception("Excel wrong format:" + fileExt);
        }

        //public class HolidayResult
        //{
        //    public bool hasError { get; set; }

        //    public string ErrorMsg { get; set; }

        //    public List<UserHolidayList> result { get; set; }
        //}

        public class TransferResult
        {
            public bool hasError { get; set; }

            public string ErrorMsg { get; set; }

            public List<UserTransferList> result { get; set; }
        }
    }
}
