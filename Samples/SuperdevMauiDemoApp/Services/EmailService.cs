using SampleApp.Services;

namespace SuperdevMauiDemoApp.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmail(string subject, string body, List<string> recipients)
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    To = recipients,
                    //Cc = ccRecipients,
                    //Bcc = bccRecipients
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException fnsex)
            {
                // Email is not supported on this device
            }
            catch (Exception ex)
            {
                // Some other exception occurred
            }
        }


    }
}