using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QY.Admin.Logic.ViewModels
{
    public class HolidayTransferMailResult
    {
        public HolidayTransferMailResult()
        {
            SuccessList = new List<string>();
            FailureList = new List<string>();
            FailureMsg = new List<string>();
        }

        public List<string> SuccessList
        {
            get;
            set;
        }

        public List<string> FailureList { get; set; }

        public List<string> FailureMsg { get; set; }

        public int SuccessCount
        {
            get { return SuccessList.Count; }
        }

        public int FailureCount
        {
            get { return FailureList.Count; }
        }
    }
}
