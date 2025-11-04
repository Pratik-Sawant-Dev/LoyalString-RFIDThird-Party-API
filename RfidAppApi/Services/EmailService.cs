using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Email service implementation using SMTP
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            var emailSettings = _configuration.GetSection("EmailSettings");
            _smtpHost = emailSettings["SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            _smtpUsername = emailSettings["SmtpUsername"] ?? "";
            _smtpPassword = emailSettings["SmtpPassword"] ?? "";
            _fromEmail = emailSettings["FromEmail"] ?? "noreply@rfidapp.com";
            _fromName = emailSettings["FromName"] ?? "RFID Jewelry Dashboard";
            _enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName, string userFullName, string organisationName, string loginUrl)
        {
            var subject = "Welcome to Sparkle RFID Dashboard! ✨";
            var body = GenerateWelcomeEmailTemplate(userName, userFullName, organisationName, loginUrl);
            
            return await SendEmailAsync(toEmail, subject, body, true);
        }

        public async Task<bool> SendUserRegistrationNotificationAsync(string adminEmail, string adminName, string newUserEmail, string newUserFullName, string newUserName, string organisationName)
        {
            var subject = $"New User Registered - {newUserFullName}";
            var body = GenerateUserRegistrationNotificationTemplate(adminName, newUserEmail, newUserFullName, newUserName, organisationName);
            
            return await SendEmailAsync(adminEmail, subject, body, true);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken, string resetUrl)
        {
            var subject = "Password Reset Request - RFID Dashboard";
            var body = GeneratePasswordResetEmailTemplate(userName, resetToken, resetUrl);
            
            return await SendEmailAsync(toEmail, subject, body, true);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Skip email sending if SMTP is not configured
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogWarning("Email not sent to {Email} - SMTP not configured. Subject: {Subject}", toEmail, subject);
                    return false;
                }

                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = _enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(new MailAddress(toEmail));

                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email sent successfully to {Email}. Subject: {Subject}", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}. Subject: {Subject}", toEmail, subject);
                return false;
            }
        }

        private string GenerateWelcomeEmailTemplate(string userName, string userFullName, string organisationName, string loginUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome to Sparkle RFID Dashboard</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: #ffffff;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 10px 30px rgba(0,0,0,0.2);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 40px 20px;
            text-align: center;
            color: white;
        }}
        .header h1 {{
            margin: 0;
            font-size: 32px;
            font-weight: 700;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
        }}
        .sparkle {{
            font-size: 48px;
            display: block;
            margin: 10px 0;
        }}
        .content {{
            padding: 40px 30px;
            color: #333333;
            line-height: 1.8;
        }}
        .greeting {{
            font-size: 24px;
            font-weight: 600;
            color: #667eea;
            margin-bottom: 20px;
        }}
        .message {{
            font-size: 16px;
            color: #555555;
            margin-bottom: 30px;
        }}
        .info-box {{
            background: #f8f9fa;
            border-left: 4px solid #667eea;
            padding: 20px;
            margin: 30px 0;
            border-radius: 5px;
        }}
        .info-box strong {{
            color: #667eea;
        }}
        .button {{
            display: inline-block;
            padding: 15px 40px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            border-radius: 30px;
            font-weight: 600;
            font-size: 16px;
            margin: 20px 0;
            text-align: center;
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
            transition: transform 0.2s;
        }}
        .button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
        }}
        .footer {{
            background: #f8f9fa;
            padding: 30px;
            text-align: center;
            color: #666666;
            font-size: 14px;
        }}
        .footer a {{
            color: #667eea;
            text-decoration: none;
        }}
        .features {{
            margin: 30px 0;
        }}
        .feature-item {{
            display: flex;
            align-items: center;
            margin: 15px 0;
            padding: 10px;
        }}
        .feature-icon {{
            font-size: 24px;
            margin-right: 15px;
        }}
        .feature-text {{
            font-size: 16px;
            color: #555555;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <span class=""sparkle"">✨</span>
            <h1>Welcome to Sparkle RFID Dashboard!</h1>
        </div>
        <div class=""content"">
            <div class=""greeting"">Hello {userFullName}!</div>
            <div class=""message"">
                We're thrilled to welcome you to the <strong>Sparkle RFID Dashboard</strong>! You've been successfully registered as a member of <strong>{organisationName}</strong>.
            </div>
            
            <div class=""info-box"">
                <strong>Your Account Details:</strong><br>
                Username: <strong>{userName}</strong><br>
                Organization: <strong>{organisationName}</strong>
            </div>

            <div class=""message"">
                You can now access the dashboard and start managing your RFID inventory with ease. Our platform offers:
            </div>

            <div class=""features"">
                <div class=""feature-item"">
                    <span class=""feature-icon"">📦</span>
                    <span class=""feature-text"">Product Management</span>
                </div>
                <div class=""feature-item"">
                    <span class=""feature-icon"">🏷️</span>
                    <span class=""feature-text"">RFID Tag Tracking</span>
                </div>
                <div class=""feature-item"">
                    <span class=""feature-icon"">📊</span>
                    <span class=""feature-text"">Real-time Inventory Reports</span>
                </div>
                <div class=""feature-item"">
                    <span class=""feature-icon"">💼</span>
                    <span class=""feature-text"">Invoice Management</span>
                </div>
                <div class=""feature-item"">
                    <span class=""feature-icon"">📈</span>
                    <span class=""feature-text"">Stock Transfer & Verification</span>
                </div>
            </div>

            <div style=""text-align: center; margin: 40px 0;"">
                <a href=""{loginUrl}"" class=""button"">Access Dashboard</a>
            </div>

            <div class=""message"">
                If you have any questions or need assistance, please don't hesitate to contact your administrator.
            </div>
        </div>
        <div class=""footer"">
            <p>This is an automated email from <strong>Sparkle RFID Dashboard</strong></p>
            <p>&copy; {DateTime.Now.Year} {organisationName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateUserRegistrationNotificationTemplate(string adminName, string newUserEmail, string newUserFullName, string newUserName, string organisationName)
        {
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New User Registration Notification</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            background: #f5f5f5;
        }}
        .email-container {{
            max-width: 600px;
            margin: 20px auto;
            background: #ffffff;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 30px 20px;
            text-align: center;
            color: white;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: 600;
        }}
        .content {{
            padding: 30px;
            color: #333333;
            line-height: 1.8;
        }}
        .greeting {{
            font-size: 18px;
            font-weight: 600;
            color: #667eea;
            margin-bottom: 15px;
        }}
        .message {{
            font-size: 16px;
            color: #555555;
            margin-bottom: 20px;
        }}
        .user-info-box {{
            background: #f8f9fa;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            padding: 20px;
            margin: 20px 0;
        }}
        .info-row {{
            display: flex;
            justify-content: space-between;
            padding: 10px 0;
            border-bottom: 1px solid #e0e0e0;
        }}
        .info-row:last-child {{
            border-bottom: none;
        }}
        .info-label {{
            font-weight: 600;
            color: #667eea;
        }}
        .info-value {{
            color: #333333;
        }}
        .footer {{
            background: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #666666;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>New User Registration Notification</h1>
        </div>
        <div class=""content"">
            <div class=""greeting"">Hello {adminName},</div>
            <div class=""message"">
                A new user has been successfully registered in your organization <strong>{organisationName}</strong>.
            </div>
            
            <div class=""user-info-box"">
                <div class=""info-row"">
                    <span class=""info-label"">Full Name:</span>
                    <span class=""info-value"">{newUserFullName}</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Username:</span>
                    <span class=""info-value"">{newUserName}</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Email:</span>
                    <span class=""info-value"">{newUserEmail}</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Organization:</span>
                    <span class=""info-value"">{organisationName}</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Registration Date:</span>
                    <span class=""info-value"">{DateTime.Now:MMMM dd, yyyy 'at' HH:mm}</span>
                </div>
            </div>

            <div class=""message"">
                The user has been sent a welcome email with login instructions. You can manage this user's permissions and access from the admin dashboard.
            </div>
        </div>
        <div class=""footer"">
            <p>This is an automated notification from <strong>Sparkle RFID Dashboard</strong></p>
            <p>&copy; {DateTime.Now.Year} {organisationName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePasswordResetEmailTemplate(string userName, string resetToken, string resetUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset Request</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            background: #f5f5f5;
        }}
        .email-container {{
            max-width: 600px;
            margin: 20px auto;
            background: #ffffff;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 30px 20px;
            text-align: center;
            color: white;
        }}
        .content {{
            padding: 30px;
            color: #333333;
            line-height: 1.8;
        }}
        .button {{
            display: inline-block;
            padding: 15px 40px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            border-radius: 30px;
            font-weight: 600;
            margin: 20px 0;
        }}
        .footer {{
            background: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #666666;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>Password Reset Request</h1>
        </div>
        <div class=""content"">
            <p>Hello {userName},</p>
            <p>You have requested to reset your password for Sparkle RFID Dashboard.</p>
            <p style=""text-align: center;"">
                <a href=""{resetUrl}?token={resetToken}"" class=""button"">Reset Password</a>
            </p>
            <p>This link will expire in 24 hours.</p>
            <p>If you did not request this password reset, please ignore this email.</p>
        </div>
        <div class=""footer"">
            <p>&copy; {DateTime.Now.Year} Sparkle RFID Dashboard. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}

