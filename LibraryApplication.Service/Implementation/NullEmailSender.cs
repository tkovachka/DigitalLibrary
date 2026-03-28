using Microsoft.AspNetCore.Identity.UI.Services;


namespace LibraryApplication.Service.Implementation
{
    /// <summary>
    /// Placeholder email sender. Replace with SendGrid implementation before production.
    /// </summary>
    public class NullEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }
}
