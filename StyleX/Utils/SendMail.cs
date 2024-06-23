using System.Net.Mail;
using System.Net;

namespace StyleX.Utils
{
    public class SendMail
    {
        public bool SendEmailByGmail(string toEmail, string subject, string body)
        {
            string fromEmail = "kaioco09@gmail.com";
            string password = ""; //app password , 2auth
            try
            {
                MailMessage message = new MailMessage(fromEmail, toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;


                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.Credentials = new NetworkCredential(fromEmail, password);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Send(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
