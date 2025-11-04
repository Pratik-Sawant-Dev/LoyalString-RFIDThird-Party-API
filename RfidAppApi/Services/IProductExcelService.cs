using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service interface for Product Excel operations
    /// </summary>
    public interface IProductExcelService
    {
        /// <summary>
        /// Generate Excel template for product upload
        /// </summary>
        Task<byte[]> GenerateExcelTemplateAsync();

        /// <summary>
        /// Upload products from Excel file
        /// </summary>
        Task<ProductExcelUploadResponseDto> UploadProductsFromExcelAsync(ProductExcelUploadDto uploadDto, string clientCode);

        /// <summary>
        /// Read and parse Excel file to extract product data
        /// </summary>
        Task<List<ProductExcelRowDto>> ReadExcelFileAsync(IFormFile file);
    }
}

