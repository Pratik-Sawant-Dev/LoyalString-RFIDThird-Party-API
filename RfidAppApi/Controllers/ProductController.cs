using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

namespace RfidAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IUserFriendlyProductService _productService;
        private readonly IWebHostEnvironment _environment;

        public ProductController(IUserFriendlyProductService productService, IWebHostEnvironment environment)
        {
            _productService = productService;
            _environment = environment;
        }

        /// <summary>
        /// Create a new product with user-friendly inputs
        /// Users can simply enter text values like "Gold", "Pune", "Counter1" instead of knowing IDs
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] UserFriendlyCreateProductDto createDto)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var result = await _productService.CreateProductAsync(createDto, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = "Product created successfully",
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new
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
                    message = "An error occurred while creating the product",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Create multiple products in bulk with user-friendly inputs
        /// </summary>
        [HttpPost("bulk-create")]
        public async Task<IActionResult> CreateBulkProducts([FromBody] BulkCreateProductsDto bulkDto)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var result = await _productService.CreateBulkProductsAsync(bulkDto, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = $"Bulk product creation completed. {result.SuccessfullyCreated} created, {result.Failed} failed.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating products in bulk",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new product with images
        /// </summary>
        [HttpPost("create-with-images")]
        public async Task<IActionResult> CreateProductWithImages([FromForm] UserFriendlyCreateProductWithImagesDto createDto, [FromForm] List<IFormFile>? images)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var result = await _productService.CreateProductWithImagesAsync(createDto, images, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = "Product created successfully with images",
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new
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
                    message = "An error occurred while creating the product with images",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a product by ID with user-friendly response
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var product = await _productService.GetProductAsync(id, clientCode);
                
                if (product == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Product with ID {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = product
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the product",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all products with user-friendly response
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var products = await _productService.GetAllProductsAsync(clientCode);
                
                return Ok(new
                {
                    success = true,
                    data = products,
                    count = products.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving products",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update a product with user-friendly inputs
        /// Users can update master data using text values instead of IDs
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UserFriendlyUpdateProductDto updateDto)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var result = await _productService.UpdateProductAsync(id, updateDto, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = "Product updated successfully",
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
                    message = "An error occurred while updating the product",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update a product with images
        /// </summary>
        [HttpPut("{id}/with-images")]
        public async Task<IActionResult> UpdateProductWithImages(int id, [FromForm] UserFriendlyUpdateProductWithImagesDto updateDto, [FromForm] List<IFormFile>? newImages)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var result = await _productService.UpdateProductWithImagesAsync(id, updateDto, newImages, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = "Product updated successfully with images",
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
                    message = "An error occurred while updating the product with images",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a product by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var deleted = await _productService.DeleteProductAsync(id, clientCode);
                
                if (!deleted)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Product with ID {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Product deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the product",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Search products by various criteria
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string? categoryName = null,
            [FromQuery] string? branchName = null,
            [FromQuery] string? counterName = null,
            [FromQuery] string? productName = null,
            [FromQuery] string? designName = null,
            [FromQuery] string? purityName = null,
            [FromQuery] string? status = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var allProducts = await _productService.GetAllProductsAsync(clientCode);
                
                // Apply filters
                var filteredProducts = allProducts.AsQueryable();

                if (!string.IsNullOrWhiteSpace(categoryName))
                    filteredProducts = filteredProducts.Where(p => p.CategoryName.ToLower().Contains(categoryName.ToLower()));

                if (!string.IsNullOrWhiteSpace(branchName))
                    filteredProducts = filteredProducts.Where(p => p.BranchName.ToLower().Contains(branchName.ToLower()));

                if (!string.IsNullOrWhiteSpace(counterName))
                    filteredProducts = filteredProducts.Where(p => p.CounterName.ToLower().Contains(counterName.ToLower()));

                if (!string.IsNullOrWhiteSpace(productName))
                    filteredProducts = filteredProducts.Where(p => p.ProductName.ToLower().Contains(productName.ToLower()));

                if (!string.IsNullOrWhiteSpace(designName))
                    filteredProducts = filteredProducts.Where(p => p.DesignName.ToLower().Contains(designName.ToLower()));

                if (!string.IsNullOrWhiteSpace(purityName))
                    filteredProducts = filteredProducts.Where(p => p.PurityName.ToLower().Contains(purityName.ToLower()));

                if (!string.IsNullOrWhiteSpace(status))
                    filteredProducts = filteredProducts.Where(p => p.Status != null && p.Status.ToLower() == status.ToLower());

                if (minPrice.HasValue)
                    filteredProducts = filteredProducts.Where(p => p.Mrp != null && p.Mrp >= minPrice.Value);

                if (maxPrice.HasValue)
                    filteredProducts = filteredProducts.Where(p => p.Mrp != null && p.Mrp <= maxPrice.Value);

                var results = filteredProducts.ToList();

                return Ok(new
                {
                    success = true,
                    data = results,
                    count = results.Count,
                    filters = new
                    {
                        categoryName,
                        branchName,
                        counterName,
                        productName,
                        designName,
                        purityName,
                        status,
                        minPrice,
                        maxPrice
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while searching products",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get product statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetProductStats()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                var allProducts = await _productService.GetAllProductsAsync(clientCode);
                
                var stats = new
                {
                    totalProducts = allProducts.Count,
                    activeProducts = allProducts.Count(p => p.Status != null && p.Status.ToLower() == "active"),
                    inactiveProducts = allProducts.Count(p => p.Status == null || p.Status.ToLower() != "active"),
                    totalValue = allProducts.Sum(p => p.Mrp ?? 0),
                    averagePrice = allProducts.Any() ? allProducts.Average(p => p.Mrp ?? 0) : 0,
                    categories = allProducts.GroupBy(p => p.CategoryName)
                        .Select(g => new { category = g.Key, count = g.Count() })
                        .OrderByDescending(x => x.count),
                    branches = allProducts.GroupBy(p => p.BranchName)
                        .Select(g => new { branch = g.Key, count = g.Count() })
                        .OrderByDescending(x => x.count)
                };

                return Ok(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving product statistics",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Serve product image by file path
        /// </summary>
        [HttpGet("image/{*filePath}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductImage(string filePath)
        {
            try
            {
                // Decode the file path from URL
                var decodedFilePath = Uri.UnescapeDataString(filePath);
                
                // Construct the full path to the image
                var webRootPath = _environment.WebRootPath;
                var fullPath = Path.Combine(webRootPath, "uploads", "products", decodedFilePath);
                
                // Security check: ensure the path is within the uploads directory
                var uploadsPath = Path.Combine(webRootPath, "uploads", "products");
                if (!fullPath.StartsWith(uploadsPath))
                {
                    return BadRequest("Invalid file path");
                }
                
                // Check if file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound("Image not found");
                }
                
                // Get file extension to determine content type
                var extension = Path.GetExtension(fullPath).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };
                
                // Return the file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while serving the image",
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
