using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace QY.Admin.Logic.Models
{
    public class SalaryFileModel
    {
        public SalaryFileModel()
        {
            SalaryData = new DataTable();
            Date = string.Format("{0}/{1}", (DateTime.Today.Month < 10 ? "0" : "") + DateTime.Today.Month.ToString(), DateTime.Today.Year.ToString());
        }

        public SalaryFileModel(DataTable dt, string year, string month)
        {
            SalaryData = dt;
            Date = string.Format("{0}/{1}", month, year);
        }

        public DataTable SalaryData { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public HttpPostedFileBase FileLoaded { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [RegularExpression(@"^\d{2}/\d{4}$", ErrorMessage = "Pls confirm Date to format: mm/yyyy")]
        public string Date { get; set; }

    }
}
