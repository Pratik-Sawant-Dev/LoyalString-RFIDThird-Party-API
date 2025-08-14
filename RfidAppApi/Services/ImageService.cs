using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace RfidAppApi.Services
{
    public class ImageService : IImageService
    {
        private readonly IClientService _clientService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly int _maxImageDimension = 4096; // 4K resolution

        public ImageService(IClientService clientService, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _clientService = clientService;
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<ProductImageResponseDto> UploadImageAsync(IFormFile file, ProductImageUploadDto uploadDto, string clientCode)
        {
            // Validate file
            if (!await ValidateImageFileAsync(file))
                throw new ArgumentException("Invalid image file");

            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Create upload directory
            var uploadPath = GetUploadPath(clientCode);
            Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            var fileName = GenerateUniqueFileName(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Generate thumbnail if needed
            string? thumbnailPath = null;
            if (uploadDto.ImageType == "Thumbnail")
            {
                thumbnailPath = await GenerateThumbnailAsync(filePath, uploadPath);
            }

            // Create database record
            var productImage = new ProductImage
            {
                ClientCode = clientCode,
                ProductId = uploadDto.ProductId,
                FileName = fileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                OriginalFileName = file.FileName,
                ImageType = uploadDto.ImageType ?? "Secondary",
                DisplayOrder = uploadDto.DisplayOrder,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            context.ProductImages.Add(productImage);
            await context.SaveChangesAsync();

            return await MapToResponseDtoAsync(productImage);
        }

        public async Task<List<ProductImageResponseDto>> UploadMultipleImagesAsync(List<IFormFile> files, List<ProductImageUploadDto> uploadDtos, string clientCode)
        {
            if (files.Count != uploadDtos.Count)
                throw new ArgumentException("Number of files must match number of upload DTOs");

            var results = new List<ProductImageResponseDto>();

            for (int i = 0; i < files.Count; i++)
            {
                try
                {
                    var result = await UploadImageAsync(files[i], uploadDtos[i], clientCode);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other files
                    // In production, you might want to use a proper logging framework
                    Console.WriteLine($"Error uploading file {files[i].FileName}: {ex.Message}");
                }
            }

            return results;
        }

        public async Task<ProductImageResponseDto?> GetImageAsync(int imageId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var image = await context.ProductImages
                .FirstOrDefaultAsync(i => i.Id == imageId && i.ClientCode == clientCode);

            return image != null ? await MapToResponseDtoAsync(image) : null;
        }

        public async Task<List<ProductImageResponseDto>> GetProductImagesAsync(int productId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var images = await context.ProductImages
                .Where(i => i.ProductId == productId && i.ClientCode == clientCode && i.IsActive)
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.CreatedOn)
                .ToListAsync();

            var results = new List<ProductImageResponseDto>();
            foreach (var image in images)
            {
                results.Add(await MapToResponseDtoAsync(image));
            }

            return results;
        }

        public async Task<ProductImageResponseDto> UpdateImageAsync(int imageId, ProductImageUpdateDto updateDto, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var image = await context.ProductImages
                .FirstOrDefaultAsync(i => i.Id == imageId && i.ClientCode == clientCode);

            if (image == null)
                throw new InvalidOperationException($"Image with ID {imageId} not found");

            if (updateDto.ImageType != null)
                image.ImageType = updateDto.ImageType;

            if (updateDto.DisplayOrder.HasValue)
                image.DisplayOrder = updateDto.DisplayOrder.Value;

            if (updateDto.IsActive.HasValue)
                image.IsActive = updateDto.IsActive.Value;

            image.UpdatedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return await MapToResponseDtoAsync(image);
        }

        public async Task<bool> DeleteImageAsync(int imageId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var image = await context.ProductImages
                .FirstOrDefaultAsync(i => i.Id == imageId && i.ClientCode == clientCode);

            if (image == null)
                return false;

            // Delete physical file
            try
            {
                if (File.Exists(image.FilePath))
                    File.Delete(image.FilePath);
            }
            catch (Exception ex)
            {
                // Log error but continue with database deletion
                Console.WriteLine($"Error deleting file {image.FilePath}: {ex.Message}");
            }

            // Delete database record
            context.ProductImages.Remove(image);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteProductImagesAsync(int productId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var images = await context.ProductImages
                .Where(i => i.ProductId == productId && i.ClientCode == clientCode)
                .ToListAsync();

            foreach (var image in images)
            {
                // Delete physical file
                try
                {
                    if (File.Exists(image.FilePath))
                        File.Delete(image.FilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file {image.FilePath}: {ex.Message}");
                }
            }

            context.ProductImages.RemoveRange(images);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> BulkUpdateImagesAsync(BulkImageOperationDto bulkDto, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var images = await context.ProductImages
                .Where(i => bulkDto.ImageIds.Contains(i.Id) && i.ClientCode == clientCode)
                .ToListAsync();

            foreach (var image in images)
            {
                if (bulkDto.ImageType != null)
                    image.ImageType = bulkDto.ImageType;

                if (bulkDto.DisplayOrder.HasValue)
                    image.DisplayOrder = bulkDto.DisplayOrder.Value;

                if (bulkDto.IsActive.HasValue)
                    image.IsActive = bulkDto.IsActive.Value;

                image.UpdatedOn = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkDeleteImagesAsync(List<int> imageIds, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var images = await context.ProductImages
                .Where(i => imageIds.Contains(i.Id) && i.ClientCode == clientCode)
                .ToListAsync();

            foreach (var image in images)
            {
                // Delete physical file
                try
                {
                    if (File.Exists(image.FilePath))
                        File.Delete(image.FilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file {image.FilePath}: {ex.Message}");
                }
            }

            context.ProductImages.RemoveRange(images);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<string> GetImageUrlAsync(string filePath)
        {
            // In a real application, you might want to use a CDN or cloud storage
            // For now, we'll return a URL that points to our image serving endpoint
            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7107";
            
            // Extract the relative path from the full file path
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "products");
            var relativePath = filePath.Replace(uploadsPath, "").Replace("\\", "/").TrimStart('/');
            
            // URL encode the path for safe transmission
            var encodedPath = Uri.EscapeDataString(relativePath);
            
            return $"{baseUrl}/api/Product/image/{encodedPath}";
        }

        public async Task<bool> ValidateImageFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Validate image format by trying to load it
            try
            {
                using var stream = file.OpenReadStream();
                using var image = Image.FromStream(stream);
                
                if (image.Width > _maxImageDimension || image.Height > _maxImageDimension)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GenerateThumbnailAsync(string originalFilePath, string thumbnailPath)
        {
            var thumbnailFileName = $"thumb_{Path.GetFileName(originalFilePath)}";
            var thumbnailFilePath = Path.Combine(thumbnailPath, thumbnailFileName);

            using var originalImage = Image.FromFile(originalFilePath);
            
            // Calculate thumbnail size (maintain aspect ratio)
            int maxThumbnailSize = 300;
            int thumbnailWidth, thumbnailHeight;
            
            if (originalImage.Width > originalImage.Height)
            {
                thumbnailWidth = maxThumbnailSize;
                thumbnailHeight = (int)((float)originalImage.Height / originalImage.Width * maxThumbnailSize);
            }
            else
            {
                thumbnailHeight = maxThumbnailSize;
                thumbnailWidth = (int)((float)originalImage.Width / originalImage.Height * maxThumbnailSize);
            }

            using var thumbnail = new Bitmap(thumbnailWidth, thumbnailHeight);
            using var graphics = Graphics.FromImage(thumbnail);
            
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(originalImage, 0, 0, thumbnailWidth, thumbnailHeight);
            
            thumbnail.Save(thumbnailFilePath, ImageFormat.Jpeg);

            return thumbnailFilePath;
        }

        #region Private Helper Methods

        private string GetUploadPath(string clientCode)
        {
            var basePath = Path.Combine(_environment.WebRootPath, "uploads", "products", clientCode);
            return basePath;
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            return fileName;
        }

        private async Task<ProductImageResponseDto> MapToResponseDtoAsync(ProductImage image)
        {
            return new ProductImageResponseDto
            {
                Id = image.Id,
                ProductId = image.ProductId,
                FileName = image.FileName,
                FilePath = image.FilePath,
                ContentType = image.ContentType,
                FileSize = image.FileSize,
                OriginalFileName = image.OriginalFileName,
                ImageType = image.ImageType,
                DisplayOrder = image.DisplayOrder,
                IsActive = image.IsActive,
                CreatedOn = image.CreatedOn,
                UpdatedOn = image.UpdatedOn,
                FullImageUrl = await GetImageUrlAsync(image.FilePath)
            };
        }

        #endregion
    }
}
