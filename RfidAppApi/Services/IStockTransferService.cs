using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Interface for stock transfer operations
    /// </summary>
    public interface IStockTransferService
    {
        /// <summary>
        /// Create a new stock transfer
        /// </summary>
        Task<StockTransferResponseDto> CreateTransferAsync(CreateStockTransferDto createDto, string clientCode);

        /// <summary>
        /// Create multiple stock transfers in bulk
        /// </summary>
        Task<BulkTransferResponseDto> CreateBulkTransfersAsync(BulkStockTransferDto bulkDto, string clientCode);

        /// <summary>
        /// Get a specific transfer by ID
        /// </summary>
        Task<StockTransferResponseDto?> GetTransferAsync(int transferId, string clientCode);

        /// <summary>
        /// Get all transfers with optional filtering
        /// </summary>
        Task<List<StockTransferResponseDto>> GetTransfersAsync(TransferFilterDto filter, string clientCode);

        /// <summary>
        /// Update transfer status
        /// </summary>
        Task<StockTransferResponseDto> UpdateTransferStatusAsync(int transferId, UpdateTransferStatusDto updateDto, string clientCode);

        /// <summary>
        /// Approve a transfer
        /// </summary>
        Task<StockTransferResponseDto> ApproveTransferAsync(int transferId, ApproveTransferDto approveDto, string clientCode);

        /// <summary>
        /// Reject a transfer
        /// </summary>
        Task<StockTransferResponseDto> RejectTransferAsync(int transferId, RejectTransferDto rejectDto, string clientCode);

        /// <summary>
        /// Complete a transfer (move product to destination)
        /// </summary>
        Task<StockTransferResponseDto> CompleteTransferAsync(int transferId, string completedBy, string clientCode);

        /// <summary>
        /// Cancel a transfer
        /// </summary>
        Task<StockTransferResponseDto> CancelTransferAsync(int transferId, string cancelledBy, string clientCode);

        /// <summary>
        /// Get transfer summary and statistics
        /// </summary>
        Task<TransferSummaryDto> GetTransferSummaryAsync(string clientCode, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Get transfers by product
        /// </summary>
        Task<List<StockTransferResponseDto>> GetTransfersByProductAsync(int productId, string clientCode);

        /// <summary>
        /// Get transfers by RFID
        /// </summary>
        Task<List<StockTransferResponseDto>> GetTransfersByRfidAsync(string rfidCode, string clientCode);

        /// <summary>
        /// Get pending transfers for a specific location
        /// </summary>
        Task<List<StockTransferResponseDto>> GetPendingTransfersByLocationAsync(int branchId, int counterId, int? boxId, string clientCode);

        /// <summary>
        /// Validate if a transfer is possible
        /// </summary>
        Task<bool> ValidateTransferAsync(CreateStockTransferDto createDto, string clientCode);

        /// <summary>
        /// Get transfer history for a product
        /// </summary>
        Task<List<StockTransferResponseDto>> GetProductTransferHistoryAsync(int productId, string clientCode);
    }
}
