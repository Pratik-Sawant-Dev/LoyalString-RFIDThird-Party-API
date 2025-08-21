using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Interface for stock verification service
    /// </summary>
    public interface IStockVerificationService
    {
        /// <summary>
        /// Create a new stock verification session
        /// </summary>
        Task<StockVerificationResponseDto> CreateStockVerificationAsync(CreateStockVerificationDto request, string clientCode);

        /// <summary>
        /// Submit scanned items for verification (matched and unmatched)
        /// </summary>
        Task<StockVerificationResponseDto> SubmitStockVerificationAsync(SubmitStockVerificationDto request, string clientCode);

        /// <summary>
        /// Get stock verification by ID
        /// </summary>
        Task<StockVerificationResponseDto> GetStockVerificationByIdAsync(int verificationId, string clientCode);

        /// <summary>
        /// Get all stock verifications with filters
        /// </summary>
        Task<StockVerificationReportResponseDto> GetStockVerificationsAsync(StockVerificationReportFilterDto filter, string clientCode);

        /// <summary>
        /// Get stock verification summary
        /// </summary>
        Task<StockVerificationSummaryDto> GetStockVerificationSummaryAsync(string clientCode);

        /// <summary>
        /// Get date-wise stock verification report
        /// </summary>
        Task<List<DateWiseStockVerificationReportDto>> GetDateWiseStockVerificationReportAsync(DateTime startDate, DateTime endDate, string clientCode);

        /// <summary>
        /// Complete a stock verification session
        /// </summary>
        Task<StockVerificationResponseDto> CompleteStockVerificationAsync(int verificationId, string clientCode);

        /// <summary>
        /// Cancel a stock verification session
        /// </summary>
        Task<StockVerificationResponseDto> CancelStockVerificationAsync(int verificationId, string clientCode);

        /// <summary>
        /// Get stock verification details by status (matched, unmatched, missing)
        /// </summary>
        Task<List<StockVerificationDetailDto>> GetVerificationDetailsByStatusAsync(int verificationId, string status, string clientCode);
    }
}
