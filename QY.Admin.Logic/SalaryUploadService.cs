using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using QY.Admin.Logic.Models;
using System.Data;

namespace QY.Admin.Logic
{
    public class SalaryUploadService
    {
        public SalaryUploadService(Stream fileStream, string fileName, string year, string month)
        {
            this.m_fileStream = fileStream;
            this.m_fileName = fileName;
            this.m_year = year;
            this.m_month = month;
        }

        public SalaryFileModel ReadFile()
        {
            if (this.m_workBook == null)
                BuildWorkbook(this.m_fileStream);

            this.m_sheet = this.m_workBook.GetSheetAt(0);
            if (this.m_sheet == null)
                throw new Exception("no sheet was found in the special excel");

            if (!this.m_sheet.SheetName.Contains(this.m_year.Substring(2, 2) + this.m_month))
                throw new Exception(string.Format("no sheet name with special date '{0}' was found", this.m_year.Substring(2, 2) + this.m_month));

            DataTable fileTemplate = GetFileTitle();
            int colCount = fileTemplate.Columns.Count;
            for (int rowIdx = contentIdx; rowIdx < this.m_sheet.LastRowNum + 1; rowIdx++)
            {
                IRow contentRow = this.m_sheet.GetRow(rowIdx);
                if (contentRow == null)
                    break;
                if (string.IsNullOrEmpty(contentRow.GetCell(contentRow.FirstCellNum, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString()))
                    break;
                DataRow dr = fileTemplate.NewRow();
                for (int colIdx = contentRow.FirstCellNum; colIdx < colCount; colIdx++)
                {
                    ICell cell = contentRow.GetCell(colIdx, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                    if (cell.CellType == CellType.Formula)
                        dr[colIdx] = cell.NumericCellValue.ToString("F2");
                    else
                        dr[colIdx] = cell.ToString();
                }
                fileTemplate.Rows.Add(dr);
            }

            return new SalaryFileModel(fileTemplate, m_year, m_month);

        }

        private void BuildWorkbook(Stream file)
        {
            string fileExt = Path.GetExtension(m_fileName);
            if (fileExt == ".xls")
                this.m_workBook = new HSSFWorkbook(file);
            else if (fileExt == ".xlsx")
                this.m_workBook = new XSSFWorkbook(file);
            else
                throw new Exception("illegal excel extention with wrong format: " + fileExt);
        }

        protected DataTable GetFileTitle()
        {
            DataTable dtHeader = new DataTable();
            bool readToEnd = false;

            //get file title table
            for (int row = this.m_sheet.FirstRowNum; row < this.m_sheet.LastRowNum + 1; row++)
            {
                IRow curRow = this.m_sheet.GetRow(row);
                int tempFirstCell;
                string firsttCell = curRow.GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                if (string.IsNullOrEmpty(firsttCell))
                    continue;
                if (firsttCell.Contains(wage))
                    continue;
                if (firsttCell.Contains("月份") && int.TryParse(this.m_sheet.GetRow(row + 2).GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(), out tempFirstCell))
                {
                    readToEnd = !readToEnd;
                    contentIdx = row + 2;
                    int mergeCellIdx = 0;
                    string mergeCellVal = "";
                    bool mergeEnd = true;
                    IRow secRow = this.m_sheet.GetRow(row + 1);
                    for (int col = 0; col < curRow.LastCellNum; col++)
                    {
                        string curCellVal = curRow.GetCell(col, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim().ToLower();
                        string secCellVal = secRow.GetCell(col, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim().ToLower();
                        if (mergeEnd)
                        {
                            if (mergeCellIdx == 0)
                            {
                                if (EmergeCols.Keys.Contains(curCellVal))
                                {
                                    mergeEnd = !mergeEnd;
                                    mergeCellIdx = EmergeCols[curCellVal] - 1;
                                    mergeCellVal = curCellVal;
                                }
                            }
                        }
                        else if (mergeCellIdx > 0)
                        {
                            curCellVal = mergeCellVal;
                            mergeCellIdx--;
                        }
                        else if (mergeCellIdx == 0)
                            mergeEnd = true;

                        if (!string.IsNullOrWhiteSpace(curCellVal) && !string.IsNullOrWhiteSpace(secCellVal))
                            dtHeader.Columns.Add(string.Format("{0}/{1}", curCellVal, secCellVal));
                        else if (string.IsNullOrWhiteSpace(curCellVal))
                            dtHeader.Columns.Add(secCellVal);
                        else if (string.IsNullOrWhiteSpace(secCellVal))
                            dtHeader.Columns.Add(curCellVal);
                    }
                }
                if (readToEnd)
                    break;
            }

            return dtHeader;
        }

        private static string wage = "工资表";
        private int contentIdx;

        private Stream m_fileStream;
        private IWorkbook m_workBook;
        private ISheet m_sheet;
        private DataTable salaryDetail = new DataTable();

        private string m_fileName { get; set; }

        private string m_memoryFileName { get; set; }

        private string m_year { get; set; }

        private string m_month { get; set; }

        private Dictionary<string, int> EmergeCols = new Dictionary<string, int>(){
            {"加班", 6},
            {"事假", 2},
            {"病假", 2},
            {"个人", 3},
            {"公司", 5}
        };
    }
}
