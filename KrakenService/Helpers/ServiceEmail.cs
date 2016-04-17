using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.Helpers
{
    public static class ServiceEmail
    {

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Send(string to, string subject, string body, bool isBodyHtml )
        {
            // set the smtp client
            try
            {                
                SmtpClient client = new SmtpClient();
                client.Port = int.Parse(ConfigurationManager.AppSettings["port"]);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = ConfigurationManager.AppSettings["host"];
                client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
                

                MailMessage mail = new MailMessage(ConfigurationManager.AppSettings["from"], to);
                mail.Subject = subject;
                mail.IsBodyHtml = isBodyHtml;
                mail.Body = body;
                client.Send(mail);
            }
            catch(Exception ex)
            {
                log.Error( "EmailService.Send - " + ex.Message + " - "  + ex.InnerException);
            }
            
        }
    }
}
