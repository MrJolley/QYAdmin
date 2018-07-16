using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QY.Admin.Logic.ViewModels
{
    public class HolidayDetail
    {
        private DateTime curDate = DateTime.Now;

        public string ChineseName { get; set; }

        public DateTime HolidayStartDate { get; set; }

        public DateTime HolidayEndDate { get; set; }

        public DateTime PreviousStartDate { get; set; }

        public DateTime PreviousEndDate { get; set; }

        public double BeforePaidLeaveRemainingHours { get; set; }

        public double CurrentPaidLeaveTotalHours { get; set; }

        public double CurrentUsedPaidLeaveHours { get; set; }

        public double PaidLeaveRemainingHours { get; set; }

        public double CurrentAvailableRemainingHours
        {
            get
            {
                double availableMonth = (curDate.Year - HolidayStartDate.Year) * 12 + (curDate.Month - HolidayStartDate.Month) + 1;
                double num = curDate < HolidayStartDate ? 0 :
                    (curDate > HolidayEndDate ? 12 :
                    (availableMonth > 12 ? 12 : availableMonth));
                var legalRemaining = Math.Round((num > 6 ? BeforePaidLeaveRemainingHours : BeforePaidLeaveRemainingHours / 2) + CurrentPaidLeaveTotalHours / 12 * num, 2);
                return legalRemaining > CurrentUsedPaidLeaveHours ? (legalRemaining - CurrentUsedPaidLeaveHours) : 0;
            }
        }

        public string Email { get; set; }
    }
}
