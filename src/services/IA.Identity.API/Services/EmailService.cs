using IA.WebAPI.Core.Email;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IA.Identity.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSetting _emailSetting;

        public EmailService(IOptions<EmailSetting> emailSetting)
        {
            _emailSetting = emailSetting.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (_emailSetting == null)
                return;

            var toEmail = string.IsNullOrEmpty(email) ? _emailSetting.ToEmail : email;

            var mail = new MailMessage()
            {
                From = new MailAddress(_emailSetting.UserEmail, _emailSetting.UserEmailName),
                To = { new MailAddress(toEmail) },
                //CC = { new MailAddress(_emailSetting.CcEmail) },
                Subject = "Identity Asymmetric - " + subject,
                Body = message,
                IsBodyHtml = true,
                Priority = MailPriority.High
            };

            using var smtp = new SmtpClient(_emailSetting.Domain, _emailSetting.Port)
            {
                Credentials = new NetworkCredential(_emailSetting.UserSMTP, _emailSetting.UserEmailPassword),
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
    }
}