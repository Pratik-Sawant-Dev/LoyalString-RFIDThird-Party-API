namespace RfidAppApi.Services
{
    /// <summary>
    /// Email service interface for sending emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send welcome email to newly registered user
        /// </summary>
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName, string userFullName, string organisationName, string loginUrl);

        /// <summary>
        /// Send notification email to admin when a new user is registered
        /// </summary>
        Task<bool> SendUserRegistrationNotificationAsync(string adminEmail, string adminName, string newUserEmail, string newUserFullName, string newUserName, string organisationName);

        /// <summary>
        /// Send password reset email
        /// </summary>
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken, string resetUrl);

        /// <summary>
        /// Send generic email
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    }
}

