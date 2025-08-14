using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDetailsDto>> GetAllProductsAsync();
        Task<ProductDetailsDto?> GetProductByIdAsync(int id);
        Task<ProductDetailsDto?> GetProductByItemCodeAsync(string itemCode, string clientCode);
        Task<ProductDetailsDto> CreateProductAsync(CreateProductDetailsDto createProductDto);
        Task<ProductDetailsDto> UpdateProductAsync(int id, UpdateProductDetailsDto updateProductDto);
        Task DeleteProductAsync(int id);
        Task<IEnumerable<ProductDetailsDto>> GetProductsByClientCodeAsync(string clientCode);
        Task<IEnumerable<ProductDetailsDto>> GetProductsByBranchAsync(int branchId);
        Task<IEnumerable<ProductDetailsDto>> GetProductsByCounterAsync(int counterId);
        Task<IEnumerable<ProductDetailsDto>> SearchProductsAsync(string searchTerm, string clientCode);
        Task<IEnumerable<ProductDetailsDto>> GetProductsWithRfidAsync(string clientCode);
    }
} 