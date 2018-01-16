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
        public HolidayResult ReadHolidayFile()
        {
            var sheet = this._workBook.GetSheetAt(0);
            if (sheet == null)
            {
                return new HolidayResult()
                {
                    hasError = true,
                    ErrorMsg = "当前文件无工作表数据"
                };
            }
            int titleRowIdx = 0;
            for (int i = 0; i < sheet.LastRowNum; i++)
            {
                if (i > 10)
                {
                    return new HolidayResult()
                    {
                        hasError = true,
                        ErrorMsg = "当前文件数据格式不正确，未在规定范围内找到起始数据列'序号'"
                    };
                }
                var flag = sheet.GetRow(i).GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                if (flag.Equals("序号"))
                {
                    titleRowIdx = i;
                    break;
                }
            }
            List<HolidayDetail> result = new List<HolidayDetail>();
            if (titleRowIdx > 0)
            {
                IRow title = sheet.GetRow(titleRowIdx);
                //获取最后剩余年假的列
                int totalRemainingIdx = 0;
                for (int row = 0; row < title.LastCellNum + 1; row++)
                {
                    if (title.GetCell(row, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Contains("剩余年假"))
                    {
                        totalRemainingIdx = row;
                        break;
                    }
                }
                if (totalRemainingIdx == 0)
                {
                    return new HolidayResult()
                    {
                        hasError = true,
                        ErrorMsg = "当前文件数据格式不正确，未在规定范围内找到数据列'剩余年假'"
                    };
                }
                //读取所有年假信息，默认title为两行数据
                int cols = title.LastCellNum;
                var users = new UserService().GetAllUsers().ToList();
                for (int row = titleRowIdx + 2; row < sheet.LastRowNum + 1; row++)
                {
                    string name = string.Empty;
                    try
                    {
                        IRow sRow = sheet.GetRow(row);
                        name = sRow.GetCell(1, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                        bool nameExt = name != null && name != ""; //名字单元格不为空
                                                                   //确保第一个名字单元格存在
                        if (nameExt)
                        {
                            if (users.Where(r => r.ChineseName == name).Count() == 0)
                            {
                                throw new InvalidDataException("user chinese name not match the special data in database");
                            }
                            //读取所有基本信息详情
                            var specialCol = sRow.GetCell(totalRemainingIdx, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                            double val = 0.0;
                            if (specialCol.CellType == CellType.Formula)
                            {
                                val = specialCol.NumericCellValue;
                            }
                            else
                            {
                                val = string.IsNullOrWhiteSpace(specialCol.ToString().Trim()) ? 0 : double.Parse(specialCol.ToString().Trim());
                            }
                            var pr = sRow.GetCell(8, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                            var dds = sRow.GetCell(9, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                            var holiday = new HolidayDetail()
                            {
                                ChineseName = name,
                                HolidayStartDate = DateTime.Parse(sRow.GetCell(6).ToString().Trim()).ToString("yyyy-MM-dd"),
                                HolidayEndDate = DateTime.Parse(sRow.GetCell(7).ToString().Trim()).ToString("yyyy-MM-dd"),
                                PreviousRemainingDuration = string.IsNullOrWhiteSpace(pr) ? 0 : double.Parse(pr),
                                CurrentAvailableDuration = double.Parse(sRow.GetCell(9, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()),
                                //PreviousUsedDuration = sRow.AsEnumerable().Where(r => r.RowIndex > 6 && r.RowIndex < 19).Sum(r => r.NumericCellValue), //上一区间idx between 7 and 18
                                //CurrentUsedDuration = sRow.AsEnumerable().Where(r => r.RowIndex > 18 && r.RowIndex < 31).Sum(r => r.NumericCellValue), //当前区间idx between 19 and 30
                                RemainingTotalDuration = val,
                                Email = users.Where(r => r.ChineseName == name).Select(r => r.EmailAddress).First()
                            };
                            if (holiday.PreviousRemainingDuration > 0)
                            {
                                holiday.PreviousStartDate = DateTime.Parse(sRow.GetCell(3).ToString().Trim()).ToString("yyyy-MM-dd");
                                holiday.PreviousEndDate = DateTime.Parse(sRow.GetCell(4).ToString().Trim()).ToString("yyyy-MM-dd");
                                holiday.PreviousAvailableDuration = double.Parse(sRow.GetCell(5, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim());
                            }
                            result.Add(holiday
                                );
                        }
                        else
                        {
                            break; //读取空数据时，默认所有有效记录结束
                        }
                    }
                    catch (Exception ex)
                    {
                        return new HolidayResult()
                        {
                            hasError = true,
                            ErrorMsg = $"数据记录有误，定位：姓名=>{name}, 错误信息：{ex.Message}",
                        };
                    }
                }
            }
            if (result.Count > 0)
            {
                return new HolidayResult()
                {
                    hasError = false,
                    ErrorMsg = "",
                    result = result
                };
            }
            return new HolidayResult()
            {
                hasError = true,
                ErrorMsg = "无法读取相关数据"
            };
        }

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

        public class HolidayResult
        {
            public bool hasError { get; set; }

            public string ErrorMsg { get; set; }

            public List<HolidayDetail> result { get; set; }
        }

        public class TransferResult
        {
            public bool hasError { get; set; }

            public string ErrorMsg { get; set; }

            public List<UserTransferList> result { get; set; }
        }
    }
}
