using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ProductImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        /// <summary>
        /// Upload a single image for a product
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] ProductImageUploadDto uploadDto)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var result = await _imageService.UploadImageAsync(file, uploadDto, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = "Image uploaded successfully",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading the image",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Upload multiple images for products
        /// </summary>
        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleImages([FromForm] List<IFormFile> files, [FromForm] string uploadDtosJson)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                
                // Deserialize the JSON string to get upload DTOs
                var uploadDtos = System.Text.Json.JsonSerializer.Deserialize<List<ProductImageUploadDto>>(uploadDtosJson);
                
                if (uploadDtos == null || files.Count != uploadDtos.Count)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Number of files must match number of upload configurations"
                    });
                }

                var results = await _imageService.UploadMultipleImagesAsync(files, uploadDtos, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = $"Successfully uploaded {results.Count} images",
                    data = results
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading images",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a specific image by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetImage(int id)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var image = await _imageService.GetImageAsync(id, clientCode);
                
                if (image == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Image with ID {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = image
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the image",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all images for a specific product
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductImages(int productId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var images = await _imageService.GetProductImagesAsync(productId, clientCode);
                
                return Ok(new
                {
                    success = true,
                    data = images,
                    count = images.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving product images",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update image metadata
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImage(int id, [FromBody] ProductImageUpdateDto updateDto)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var result = await _imageService.UpdateImageAsync(id, updateDto, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = "Image updated successfully",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the image",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a specific image
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var deleted = await _imageService.DeleteImageAsync(id, clientCode);
                
                if (!deleted)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Image with ID {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Image deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the image",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete all images for a specific product
        /// </summary>
        [HttpDelete("product/{productId}")]
        public async Task<IActionResult> DeleteProductImages(int productId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var deleted = await _imageService.DeleteProductImagesAsync(productId, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = "Product images deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting product images",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Bulk update images
        /// </summary>
        [HttpPut("bulk-update")]
        public async Task<IActionResult> BulkUpdateImages([FromBody] BulkImageOperationDto bulkDto)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var result = await _imageService.BulkUpdateImagesAsync(bulkDto, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = "Images updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating images",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Bulk delete images
        /// </summary>
        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> BulkDeleteImages([FromBody] List<int> imageIds)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var result = await _imageService.BulkDeleteImagesAsync(imageIds, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = "Images deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting images",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Validate image file
        /// </summary>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateImage([FromForm] IFormFile file)
        {
            try
            {
                var isValid = await _imageService.ValidateImageFileAsync(file);
                
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        isValid,
                        fileName = file.FileName,
                        fileSize = file.Length,
                        contentType = file.ContentType
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while validating the image",
                    error = ex.Message
                });
            }
        }

        private string GetClientCodeFromToken()
        {
            var clientCodeClaim = User.FindFirst("ClientCode");
            if (clientCodeClaim == null)
            {
                throw new InvalidOperationException("Client code not found in token");
            }
            return clientCodeClaim.Value;
        }
    }
}
