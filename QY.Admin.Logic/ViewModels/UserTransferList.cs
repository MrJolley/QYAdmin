using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QY.Admin.Logic.ViewModels
{
    public class UserTransferList
    {
        #region constructor
        public UserTransferList()
        {
            UserTransferDetail = new List<TransferDetail>();
        }
        #endregion

        #region field && property
        /// <summary>
        /// 员工姓名
        /// </summary>
        private string _staffName;

        public string StaffName
        {
            get { return _staffName; }
            set { _staffName = value; }
        }


        /// <summary>
        /// 员工调休详情列表
        /// </summary>
        public List<TransferDetail> UserTransferDetail;

        private string staffEmail;

        public string StaffEmail
        {
            get { return staffEmail; }
            set { staffEmail = value; }
        }

        #endregion
    }
}
