using RfidAppApi.Data;
using RfidAppApi.Models;
using System.Text.Json;

namespace RfidAppApi.Services
{
    public class ActivityLoggingService : IActivityLoggingService
    {
        private readonly AppDbContext _context;

        public ActivityLoggingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(int userId, string clientCode, string activityType, string action, 
            string? description = null, string? tableName = null, int? recordId = null, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var activity = new UserActivity
                {
                    UserId = userId,
                    ClientCode = clientCode,
                    ActivityType = activityType,
                    Action = action,
                    Description = description,
                    TableName = tableName,
                    RecordId = recordId,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedOn = DateTime.UtcNow
                };

                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to avoid breaking the main operation
                Console.WriteLine($"Failed to log activity: {ex.Message}");
            }
        }

        public async Task LogProductActivityAsync(int userId, string clientCode, string action, int? productId = null, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            var description = action switch
            {
                "Create" => $"Created product",
                "Update" => $"Updated product {productId}",
                "Delete" => $"Deleted product {productId}",
                "View" => $"Viewed product {productId}",
                _ => $"Performed {action} on product {productId}"
            };

            await LogActivityAsync(userId, clientCode, "Product", action, description, "tblProductDetails", 
                productId, oldValues, newValues, ipAddress, userAgent);
        }

        public async Task LogRfidActivityAsync(int userId, string clientCode, string action, string? rfidCode = null, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            var description = action switch
            {
                "Create" => $"Created RFID tag {rfidCode}",
                "Update" => $"Updated RFID tag {rfidCode}",
                "Delete" => $"Deleted RFID tag {rfidCode}",
                "Assign" => $"Assigned RFID tag {rfidCode} to product",
                "Unassign" => $"Unassigned RFID tag {rfidCode} from product",
                "Scan" => $"Scanned RFID tag {rfidCode}",
                _ => $"Performed {action} on RFID tag {rfidCode}"
            };

            await LogActivityAsync(userId, clientCode, "RFID", action, description, "tblRFID", 
                null, oldValues, newValues, ipAddress, userAgent);
        }

        public async Task LogInvoiceActivityAsync(int userId, string clientCode, string action, int? invoiceId = null, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            var description = action switch
            {
                "Create" => $"Created invoice {invoiceId}",
                "Update" => $"Updated invoice {invoiceId}",
                "Delete" => $"Deleted invoice {invoiceId}",
                "Print" => $"Printed invoice {invoiceId}",
                "Email" => $"Emailed invoice {invoiceId}",
                _ => $"Performed {action} on invoice {invoiceId}"
            };

            await LogActivityAsync(userId, clientCode, "Invoice", action, description, "tblInvoice", 
                invoiceId, oldValues, newValues, ipAddress, userAgent);
        }
    }
}
