using RfidAppApi.Models;

namespace RfidAppApi.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductDetails>> GetAllAsync(string clientCode);
        Task<ProductDetails?> GetByIdAsync(int id, string clientCode);
        Task<ProductDetails> AddAsync(ProductDetails product, string clientCode);
        Task<ProductDetails> UpdateAsync(ProductDetails product, string clientCode);
        Task<bool> DeleteAsync(int id, string clientCode);
        Task<IEnumerable<ProductDetails>> GetByClientCodeAsync(string clientCode);
        Task<ProductDetails?> GetByItemCodeAsync(string itemCode, string clientCode);
        Task<IEnumerable<ProductDetails>> GetByCategoryAsync(int categoryId, string clientCode);
        Task<IEnumerable<ProductDetails>> GetByBranchAsync(int branchId, string clientCode);
        Task<IEnumerable<ProductDetails>> GetByCounterAsync(int counterId, string clientCode);
        Task<IEnumerable<ProductRfidAssignment>> GetAssignmentsAsync(string clientCode);
        Task<ProductRfidAssignment?> GetAssignmentByProductAsync(int productId, string clientCode);
        Task<ProductRfidAssignment?> GetAssignmentByRfidAsync(string rfidCode, string clientCode);
        Task<ProductRfidAssignment> AddAssignmentAsync(ProductRfidAssignment assignment, string clientCode);
        Task<ProductRfidAssignment> UpdateAssignmentAsync(ProductRfidAssignment assignment, string clientCode);
        Task<bool> DeleteAssignmentAsync(int id, string clientCode);
    }
} 