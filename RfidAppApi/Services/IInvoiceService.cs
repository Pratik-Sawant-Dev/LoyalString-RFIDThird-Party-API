using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto createDto, string clientCode);
        Task<InvoiceResponseDto?> GetInvoiceAsync(int invoiceId, string clientCode);
        Task<List<InvoiceResponseDto>> GetAllInvoicesAsync(string clientCode);
        Task<InvoiceResponseDto> UpdateInvoiceAsync(int invoiceId, UpdateInvoiceDto updateDto, string clientCode);
        Task<bool> DeleteInvoiceAsync(int invoiceId, string clientCode);
        Task<List<InvoiceResponseDto>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate, string clientCode);
        Task<List<InvoiceResponseDto>> GetInvoicesByProductAsync(int productId, string clientCode);
        Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(string clientCode);
        
        // Additional methods
        Task<List<InvoiceResponseDto>> GetInvoicesByCustomerAsync(string customerName, string clientCode);
        Task<List<InvoiceResponseDto>> GetInvoicesByPaymentMethodAsync(string paymentMethod, string clientCode);
        Task<InvoiceResponseDto?> GetInvoiceByNumberAsync(string invoiceNumber, string clientCode);
        Task<InvoiceCountDto> GetInvoiceCountByStatusAsync(string clientCode);
    }
}
