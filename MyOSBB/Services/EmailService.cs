using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Net.Mail;
using System.Net;

namespace MyOSBB.Services
{
    public class EmailService
    {
        public void SendEmail(string email, string subject, string message)
        {
            //    var emailMessage = new MimeMessage();

            //    emailMessage.From.Add(new MailboxAddress("Адміністрація сайту", "insanefury31@gmail.com"));
            //    emailMessage.To.Add(new MailboxAddress("", email));
            //    emailMessage.Subject = subject;
            //    emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            //    {
            //        Text = message
            //    };

            //    using (var client = new SmtpClient())
            //    {
            //        client.ConnectAsync("smtp.gmail.com", 465, false);
            //        client.AuthenticateAsync("insanefury31@gmail.com", "batmanisgod");
            //        client.SendAsync(emailMessage);

            //        client.DisconnectAsync(true);
            //    }
            using (MailMessage mm = new MailMessage("insanefury31@gmail.com", email))
            {
                mm.Subject = subject;
                mm.Body = message;
                mm.IsBodyHtml = false;
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    NetworkCredential NetworkCred = new NetworkCredential("insanefury31@gmail.com", "batmanisgod");
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = 587;
                    smtp.Send(mm);
                }
            }
        }
    }
}