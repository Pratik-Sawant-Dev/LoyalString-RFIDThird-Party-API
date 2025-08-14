namespace RfidAppApi.DTOs
{
    /// <summary>
    /// DTO for uploading product images
    /// </summary>
    public class ProductImageUploadDto
    {
        public int ProductId { get; set; }
        public string? ImageType { get; set; } = "Secondary"; // "Primary", "Secondary", "Thumbnail"
        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// DTO for product image response
    /// </summary>
    public class ProductImageResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public string? OriginalFileName { get; set; }
        public string? ImageType { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string FullImageUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for updating product image
    /// </summary>
    public class ProductImageUpdateDto
    {
        public string? ImageType { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for bulk image operations
    /// </summary>
    public class BulkImageOperationDto
    {
        public List<int> ImageIds { get; set; } = new List<int>();
        public string? ImageType { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for product creation with images
    /// </summary>
    public class UserFriendlyCreateProductWithImagesDto : UserFriendlyCreateProductDto
    {
        public List<ProductImageUploadDto>? Images { get; set; }
    }

    /// <summary>
    /// DTO for product update with images
    /// </summary>
    public class UserFriendlyUpdateProductWithImagesDto : UserFriendlyUpdateProductDto
    {
        public List<ProductImageUploadDto>? ImagesToAdd { get; set; }
        public List<int>? ImageIdsToRemove { get; set; }
    }
}
