using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Data;

namespace QY.Admin.Logic
{
    public class MailService
    {
        protected string sender;
        protected string date;
        public MailService(string sender, string password, string date)
        {
            smtpClient = new SmtpClient();
            smtpConfig(sender, password);
            this.sender = sender;
            this.date = date;
        }

        public MailResult MailSender(DataTable salaryToSend, string description)
        {
            MailResult mr = new MailResult();
            List<string> success = new List<string>();
            List<Failure> failure = new List<Failure>();
            StringBuilder sbMailContent = new StringBuilder();
            Regex pattern = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            foreach (DataRow dr in salaryToSend.Rows)
            {
                mailMessage = new MailMessage();
                mailConfig(this.sender, this.date);
                string receiver = null;
                string officer = null;
                sbMailContent.Clear();
                sbMailContent.Append(description).Replace("\n", "<br />");
                sbMailContent.Append("<table border='1' style = 'border-collapse:collapse'>");
                foreach (DataColumn dc in salaryToSend.Columns)
                {
                    sbMailContent.Append("<tr>");
                    sbMailContent.Append("<td>");
                    sbMailContent.Append(dc.ColumnName);
                    sbMailContent.Append("</td>");
                    sbMailContent.Append("<td style='text-align: right'>");
                    sbMailContent.Append(dr[dc.ColumnName]);
                    sbMailContent.Append("</td>");
                    sbMailContent.Append("</tr>");
                    if (dc.ColumnName.Contains("姓名"))
                        officer = dr[dc.ColumnName].ToString();
                    if (dc.ColumnName.Contains("邮箱"))
                    {
                        receiver = dr[dc.ColumnName].ToString();
                        if (!pattern.IsMatch(receiver))
                        {
                            Failure fail = new Failure();
                            fail.name = officer;
                            fail.errorMsg = "wrong email format with name '" + officer + "'";
                            failure.Add(fail);
                            break;
                        }
                    }

                }
                if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(officer))
                {
                    Failure fail = new Failure();
                    fail.name = officer;
                    fail.errorMsg = "not found email address or chinese name with name '" + officer + "'";
                    failure.Add(fail);
                    continue;
                }
                sbMailContent.Append("</table><p>Br, </p><p>QY-Admin Office<p>");

                //send message to officer
                mailMessage.Body = sbMailContent.ToString();
                mailMessage.To.Add(new MailAddress(receiver));
                
                try
                {
                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    Failure fail = new Failure();
                    fail.name = officer;
                    fail.errorMsg = ex.Message;
                    failure.Add(fail);
                    continue;
                }
                success.Add(officer);
            }
            mr.success = success;
            mr.failure = failure;
            return mr;
        }

        protected void mailConfig(string sender, string date)
        {
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.From = new MailAddress(sender, @"QY-Admin Office");
            string[] arr = date.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            mailMessage.Subject = string.Format("{0}年{1}月发放工资明细", arr[1], arr[0]);
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.Normal;
            mailMessage.Headers.Add("X-MimeOLE", "Produced By Microsoft MimeOLE V6.00.2900.2869");
            mailMessage.Headers.Add("ReturnReceipt", "1");
        }

        protected void smtpConfig(string sender, string password)
        {
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential(sender, password);
            smtpClient.Host = "smtp.263.net";
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        }

        private MailMessage mailMessage { get; set; }

        private SmtpClient smtpClient { get; set; }

        public class MailResult
        {
            public List<string> success { get; set; }

            public List<Failure> failure { get; set; }
        }

        public class Failure
        {
            public string name { get; set; }

            public string errorMsg { get; set; }
        }
    }
}
