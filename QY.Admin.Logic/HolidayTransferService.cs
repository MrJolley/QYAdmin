﻿using Newtonsoft.Json;
using QY.Admin.Logic.MailConfig;
using QY.Admin.Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QY.Admin.Logic
{
    public class HolidayTransferService
    {
        #region field && configuration
        const string newLine = "<br />";
        //配置年假字段
        private string _holidaySubject = "年假即将到期提醒";
        private string _holidayContentComment = "截止到<span style='color:red;font-weight:bold;'>{0}</span>" + 
            "你有<span style='color:red;font-weight:bold;'>{1}</span>小时的年假。" + "<br />" + "<br />" +
            "年假有效期为{2}年，请在年假区间内合理规划使用您的年假，请年假时请提前向主管申请。如因个人原因无法使用，未使用完年假将作废。" +
            "<br />" + "<br />" + "以下是年假明细：";
        //配置调休字段
        private string _transferSubject = "调休剩余时间提醒";
        private string _transferContentComment = "您还有剩余调休时间<span style='color:red;font-weight:bold;'>{0}</span>小时，" +
            "请在调休有效期内用完您的调休时间。以下为详细信息：";
        //配置公共字段
        private string _contentRespect = "Hi {0},";
        private string _mailSenderDisplay = ConfigurationManager.AppSettings["holidayTransferMailSenderDisplayName"];
        private string _mailSenderSignature = "<P>BR,</P><p>" +
           ConfigurationManager.AppSettings["holidayTransferMailSenderSignature"] + "</p>";
        #endregion

        /// <summary>
        /// 给员工发送邮件
        /// </summary>
        /// <param name="account">发件人账号</param>
        /// <param name="password">发件人密码</param>
        /// <param name="name">收件人姓名</param>
        /// <param name="content">收件内容-HTML格式</param>
        /// <param name="mailType">邮件类型：年假？调休</param>
        public void MailSending(string account, string password, string userAddress, string content, string mailType)
        {
            MailConfiguration config = new MailConfiguration();

            var client = config.GetMailClient(account, password);
            string subject = mailType.Equals("annual") ? _holidaySubject :
                mailType.Equals("transfer") ? _transferSubject : "未知主题";
            //添加抄送对象
            string ccConfig = ConfigurationManager.AppSettings["holidayTransferMailCCObject"];
            string[] cc = ccConfig.IndexOf(",") > 0 ? ccConfig.Split(',') : null;
            var message = config.GetMailMessage(subject, account, content, cc == null ? null : cc.ToList());
            //配置收件人地址
            var address = config.GetMailAddress(userAddress);
            foreach (var mail in address)
            {
                message.To.Add(mail);
            }
            client.Send(message);
        }

        public string TransferDataConvert2Html(UserTransferList transfer)
        {
            StringBuilder result = new StringBuilder();
            string name = transfer.StaffName;
            string contentRespect = string.Format(_contentRespect, name);
            //剔除调休时间为0的数据记录
            transfer.UserTransferDetail = transfer.UserTransferDetail.AsEnumerable().Where(r => r.TransferRemainingTime != 0).ToList();
            double timeLeft = transfer.UserTransferDetail.Count == 0 ? 0 : transfer.UserTransferDetail.Sum(r => r.TransferRemainingTime);
            //数据结构转化为HTML表格
            DataTable detail = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(transfer.UserTransferDetail));
            string data = TransferTableConvertHtml(detail, transfer.StaffName);

            result.Append(contentRespect).Append(newLine).Append(newLine)
                .Append(string.Format(_transferContentComment, timeLeft)).Append(newLine).Append(newLine)
                .Append(data).Append(newLine)
                .Append(_mailSenderSignature);
            return result.ToString();
        }

        public string HolidayDataConvert2Html(HolidayDetail holiday)
        {
            StringBuilder result = new StringBuilder();
            string name = holiday.ChineseName;
            string contentRespect = string.Format(_contentRespect, name);
            //数据结构转化为HTML表格
            string data = HolidayTableConvertHtml(holiday);

            result.Append(contentRespect).Append(newLine).Append(newLine)
                .Append(string.Format(_holidayContentComment, holiday.HolidayEndDate, holiday.RemainingTotalDuration, 
                (holiday.PreviousRemainingDuration == 0 && holiday.CurrentAvailableDuration == 24) ? "半" : "一")).Append(newLine).Append(newLine)
                .Append(data).Append(newLine)
                .Append(_mailSenderSignature);
            return result.ToString();
        }

        /// <summary>
        /// Table数据格式转化为邮件HTML格式
        /// </summary>
        /// <param name="dt">数据项详情表</param>
        /// <param name="nameCol">只显示一次的姓名表头</param>
        /// <param name="name">只显示一次的姓名</param>
        /// <returns></returns>
        private string HolidayTableConvertHtml(HolidayDetail holiday)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<table style='border-collapse:collapse; text-align:center'>").Append("<tr>");
            sb.Append("<td style=\"border: 1px solid black; width: 80px;\">").Append("姓名").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 280px;\">").Append("年假区间").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 110px;\">").Append("应休年假(h)").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 110px;\">").Append("剩余年假(h)").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 120px;\">").Append("到期时间").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 120px;\">").Append("延期至").Append("</td>");
            sb.Append("</tr>");
            if (holiday.PreviousRemainingDuration == 0)
            {
                sb.Append("<tr>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(holiday.ChineseName).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append($"{holiday.HolidayStartDate}--{holiday.HolidayEndDate}").Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(holiday.CurrentAvailableDuration).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(holiday.RemainingTotalDuration).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(holiday.HolidayEndDate).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append("").Append("</td>");
                sb.Append("</tr>");
            }
            else //增加上一区间和本区间两条记录
            {
                double previousRemaining = 0.0, currentRemaining = 0.0;
                if (holiday.PreviousRemainingDuration + holiday.CurrentAvailableDuration - holiday.RemainingTotalDuration >= holiday.CurrentAvailableDuration)
                {
                    previousRemaining = holiday.RemainingTotalDuration;
                }
                else
                {
                    previousRemaining = holiday.PreviousRemainingDuration;
                    currentRemaining = holiday.RemainingTotalDuration - holiday.PreviousRemainingDuration;
                }
                sb.Append("<tr>");
                sb.Append("<td rowspan=\"2\" style=\"border: 1px solid black;\">").Append(holiday.ChineseName).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append($"{holiday.PreviousStartDate}--{holiday.PreviousEndDate}").Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(holiday.PreviousAvailableDuration).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(previousRemaining).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(holiday.PreviousEndDate).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(holiday.HolidayEndDate).Append("</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append($"{holiday.HolidayStartDate}--{holiday.HolidayEndDate}").Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(holiday.CurrentAvailableDuration).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(currentRemaining).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append(holiday.HolidayEndDate).Append("</td>");
                sb.Append("<td style=\"border: 1px solid black;\">").Append("").Append("</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            return
                sb.ToString();
        }

        private string TransferTableConvertHtml(DataTable dt, string name)
        {
            StringBuilder sb = new StringBuilder();
            int rows = dt.Rows.Count;
            int cols = dt.Columns.Count;
            bool flag = true;

            sb.Append("<table style='border-collapse:collapse; text-align:center'>").Append("<tr>");
            sb.Append("<td style=\"border: 1px solid black; width: 80px;\">").Append("姓名").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 120px;\">").Append("加班日期").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 80px;\">").Append("加班时间段").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 60px;\">").Append("可用调休时长").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 200px;\">").Append("调休明细").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 60px;\">").Append("剩余调休时间").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width: 120px;\">").Append("到期日期").Append("</td>");
            sb.Append("</tr>");
            foreach (DataRow dr in dt.Rows)
            {
                sb.Append("<tr>");
                if (flag)
                {
                    sb.Append("<td style=\"border: 1px solid black;\" rowspan=" + rows + ">").Append(name).Append("</td>");
                    flag = !flag;
                }
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sb.Append("<td style=\"border: 1px solid black;\">").Append(dr[i]).Append("</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            return
                sb.ToString();
        }
    }
}
