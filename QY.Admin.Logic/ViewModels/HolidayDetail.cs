using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QY.Admin.Logic.ViewModels
{
    public class HolidayDetail
    {
        public string ChineseName { get; set; }

        public string HolidayStartDate { get; set; }

        public string HolidayEndDate { get; set; }

        public string PreviousStartDate { get; set; }

        public string PreviousEndDate { get; set; }

        public double PreviousAvailableDuration { get; set; }

        public double PreviousRemainingDuration { get; set; }

        public double CurrentAvailableDuration { get; set; }

        public double RemainingTotalDuration { get; set; }

        public string Email { get; set; }
    }
}
