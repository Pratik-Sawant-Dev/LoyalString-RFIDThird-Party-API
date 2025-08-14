using Microsoft.AspNetCore.Http;
using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    public interface IImageService
    {
        Task<ProductImageResponseDto> UploadImageAsync(IFormFile file, ProductImageUploadDto uploadDto, string clientCode);
        Task<List<ProductImageResponseDto>> UploadMultipleImagesAsync(List<IFormFile> files, List<ProductImageUploadDto> uploadDtos, string clientCode);
        Task<ProductImageResponseDto?> GetImageAsync(int imageId, string clientCode);
        Task<List<ProductImageResponseDto>> GetProductImagesAsync(int productId, string clientCode);
        Task<ProductImageResponseDto> UpdateImageAsync(int imageId, ProductImageUpdateDto updateDto, string clientCode);
        Task<bool> DeleteImageAsync(int imageId, string clientCode);
        Task<bool> DeleteProductImagesAsync(int productId, string clientCode);
        Task<bool> BulkUpdateImagesAsync(BulkImageOperationDto bulkDto, string clientCode);
        Task<bool> BulkDeleteImagesAsync(List<int> imageIds, string clientCode);
        Task<string> GetImageUrlAsync(string filePath);
        Task<bool> ValidateImageFileAsync(IFormFile file);
        Task<string> GenerateThumbnailAsync(string originalFilePath, string thumbnailPath);
    }
}
