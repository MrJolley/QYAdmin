using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace QY.Admin.Logic.MailConfig
{
    public class MailConfiguration
    {
        /// <summary>
        /// 配置邮件客户端，默认使用263邮箱发送邮件
        /// </summary>
        /// <param name="sender">发件人账号</param>
        /// <param name="password">发件人密码</param>
        /// <returns></returns>
        public SmtpClient GetMailClient(string sender, string password)
        {
            SmtpClient mailClient = new SmtpClient()
            {
                UseDefaultCredentials = false,
                Host = "smtp.263.net",
                Credentials = new System.Net.NetworkCredential(sender, password),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            return mailClient;
        }

        /// <summary>
        /// 配置邮件的地址邮箱
        /// </summary>
        /// <param name="address">参数只支持string,string[],List<string>类型</param>
        /// <returns></returns>
        public List<MailAddress> GetMailAddress(object address)
        {
            Type type = address.GetType();
            List<MailAddress> lma = new List<MailAddress>();

            if (type.Name.StartsWith("List"))
            {
                List<string> temp;
                try
                {
                    temp = (List<string>)address;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidCastException("类型转化错误，不支持的List泛型");
                }
                foreach (string name in temp)
                {
                    lma.Add(new MailAddress(name));
                }
                return lma;
            }

            switch (type.Name)
            {
                case "String":
                    MailAddress mailAddress = new MailAddress(address.ToString());
                    lma.Add(mailAddress);
                    break;
                case "String[]":
                    string[] arrayAddress = (string[])address;
                    foreach (string name in arrayAddress)
                    {
                        lma.Add(new MailAddress(name));
                    }
                    break;
                default:
                    throw new InvalidOperationException("类型转化错误，不支持的List泛型");
            }
            return lma;
        }

        /// <summary>
        /// 配置邮件正文内容
        /// </summary>
        /// <param name="subject">邮件主题</param>
        /// <param name="fromAddress">发件人邮箱</param>
        /// <param name="mailBoday">邮件正文，包括HTML标签，样式等</param>
        /// <returns></returns>
        public MailMessage GetMailMessage(string subject, string fromAddress, string mailBoday, List<string> cc = null)
        {
            MailMessage mailMessage = new MailMessage()
            {
                Subject = subject,
                Body = mailBoday,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Priority = MailPriority.Normal,
                From = new MailAddress(fromAddress),
            };
            if (cc != null && cc.Count > 0)
            {
                cc.ForEach(r => mailMessage.CC.Add(new MailAddress(r)));
            }
            mailMessage.Headers.Add("X-MimeOLE", "Produced By Microsoft MimeOLE V6.00.2900.2869");
            mailMessage.Headers.Add("ReturnReceipt", "1");
            return mailMessage;
        }
    }
}
