namespace RfidAppApi.Services
{
    public interface IActivityLoggingService
    {
        Task LogActivityAsync(int userId, string clientCode, string activityType, string action, 
            string? description = null, string? tableName = null, int? recordId = null, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null);
        
        Task LogProductActivityAsync(int userId, string clientCode, string action, int? productId = null, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null);
        
        Task LogRfidActivityAsync(int userId, string clientCode, string action, string? rfidCode = null, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null);
        
        Task LogInvoiceActivityAsync(int userId, string clientCode, string action, int? invoiceId = null, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null);
    }
}
