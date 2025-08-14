using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;
using System.Security.Claims;

namespace RfidAppApi.Services
{
    public interface IUserFriendlyProductService
    {
        Task<UserFriendlyProductResponseDto> CreateProductAsync(UserFriendlyCreateProductDto createDto, string clientCode);
        Task<UserFriendlyProductResponseDto> CreateProductWithImagesAsync(UserFriendlyCreateProductWithImagesDto createDto, List<IFormFile>? images, string clientCode);
        Task<UserFriendlyProductResponseDto> UpdateProductAsync(int productId, UserFriendlyUpdateProductDto updateDto, string clientCode);
        Task<UserFriendlyProductResponseDto> UpdateProductWithImagesAsync(int productId, UserFriendlyUpdateProductWithImagesDto updateDto, List<IFormFile>? newImages, string clientCode);
        Task<UserFriendlyProductResponseDto?> GetProductAsync(int productId, string clientCode);
        Task<List<UserFriendlyProductResponseDto>> GetAllProductsAsync(string clientCode);
        Task<BulkProductResponseDto> CreateBulkProductsAsync(BulkCreateProductsDto bulkDto, string clientCode);
        Task<bool> DeleteProductAsync(int productId, string clientCode);
    }

    public class UserFriendlyProductService : IUserFriendlyProductService
    {
        private readonly IClientService _clientService;
        private readonly IImageService _imageService;

        public UserFriendlyProductService(IClientService clientService, IImageService imageService)
        {
            _clientService = clientService;
            _imageService = imageService;
        }

        public async Task<UserFriendlyProductResponseDto> CreateProductAsync(UserFriendlyCreateProductDto createDto, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(createDto.ItemCode))
                throw new ArgumentException("ItemCode is required");

            if (string.IsNullOrWhiteSpace(createDto.CategoryName))
                throw new ArgumentException("CategoryName is required");

            if (string.IsNullOrWhiteSpace(createDto.BranchName))
                throw new ArgumentException("BranchName is required");

            if (string.IsNullOrWhiteSpace(createDto.CounterName))
                throw new ArgumentException("CounterName is required");

            // Check if product with same ItemCode already exists
            var existingProduct = await context.ProductDetails
                .FirstOrDefaultAsync(p => p.ItemCode == createDto.ItemCode);

            if (existingProduct != null)
                throw new InvalidOperationException($"Product with ItemCode '{createDto.ItemCode}' already exists");

            // Get or create master data entries
            var categoryId = await GetOrCreateCategoryAsync(context, createDto.CategoryName);
            var branchId = await GetOrCreateBranchAsync(context, createDto.BranchName, clientCode);
            var counterId = await GetOrCreateCounterAsync(context, createDto.CounterName, branchId, clientCode);
            var productId = await GetOrCreateProductAsync(context, createDto.ProductName);
            var designId = await GetOrCreateDesignAsync(context, createDto.DesignName);
            var purityId = await GetOrCreatePurityAsync(context, createDto.PurityName);

            // Handle RFID code if provided
            string? rfidCode = null;
            if (!string.IsNullOrWhiteSpace(createDto.RfidCode))
            {
                rfidCode = await GetOrCreateRfidAsync(context, createDto.RfidCode, clientCode);
            }

            // Create the product
            var product = new ProductDetails
            {
                ClientCode = clientCode,
                BranchId = branchId,
                CounterId = counterId,
                ItemCode = createDto.ItemCode,
                CategoryId = categoryId,
                ProductId = productId,
                DesignId = designId,
                PurityId = purityId,
                GrossWeight = createDto.GrossWeight,
                StoneWeight = createDto.StoneWeight,
                DiamondHeight = createDto.DiamondHeight,
                NetWeight = createDto.NetWeight,
                BoxDetails = createDto.BoxDetails,
                Size = createDto.Size,
                StoneAmount = createDto.StoneAmount,
                DiamondAmount = createDto.DiamondAmount,
                HallmarkAmount = createDto.HallmarkAmount,
                MakingPerGram = createDto.MakingPerGram,
                MakingPercentage = createDto.MakingPercentage,
                MakingFixedAmount = createDto.MakingFixedAmount,
                Mrp = createDto.Mrp,
                ImageUrl = createDto.ImageUrl,
                Status = createDto.Status ?? "Active",
                CreatedOn = DateTime.UtcNow
            };

            context.ProductDetails.Add(product);
            await context.SaveChangesAsync();

            // Create RFID assignment if RFID code was provided
            if (!string.IsNullOrWhiteSpace(rfidCode))
            {
                var rfidAssignment = new ProductRfidAssignment
                {
                    ProductId = product.Id,
                    RFIDCode = rfidCode,
                    AssignedOn = DateTime.UtcNow,
                    IsActive = true
                };
                context.ProductRfidAssignments.Add(rfidAssignment);
                await context.SaveChangesAsync();
            }

            // Return the created product with user-friendly response
            return await MapToUserFriendlyResponseAsync(context, product);
        }

        public async Task<UserFriendlyProductResponseDto> CreateProductWithImagesAsync(UserFriendlyCreateProductWithImagesDto createDto, List<IFormFile>? images, string clientCode)
        {
            // First create the product
            var productDto = new UserFriendlyCreateProductDto
            {
                ItemCode = createDto.ItemCode,
                CategoryName = createDto.CategoryName,
                BranchName = createDto.BranchName,
                CounterName = createDto.CounterName,
                ProductName = createDto.ProductName,
                DesignName = createDto.DesignName,
                PurityName = createDto.PurityName,
                RfidCode = createDto.RfidCode,
                GrossWeight = createDto.GrossWeight,
                StoneWeight = createDto.StoneWeight,
                DiamondHeight = createDto.DiamondHeight,
                NetWeight = createDto.NetWeight,
                BoxDetails = createDto.BoxDetails,
                Size = createDto.Size,
                StoneAmount = createDto.StoneAmount,
                DiamondAmount = createDto.DiamondAmount,
                HallmarkAmount = createDto.HallmarkAmount,
                MakingPerGram = createDto.MakingPerGram,
                MakingPercentage = createDto.MakingPercentage,
                MakingFixedAmount = createDto.MakingFixedAmount,
                Mrp = createDto.Mrp,
                ImageUrl = createDto.ImageUrl,
                Status = createDto.Status
            };

            var product = await CreateProductAsync(productDto, clientCode);

            // Then handle image uploads if provided
            if (images != null && images.Any() && createDto.Images != null)
            {
                var uploadDtos = new List<ProductImageUploadDto>();
                for (int i = 0; i < Math.Min(images.Count, createDto.Images.Count); i++)
                {
                    uploadDtos.Add(new ProductImageUploadDto
                    {
                        ProductId = product.Id,
                        ImageType = createDto.Images[i].ImageType,
                        DisplayOrder = createDto.Images[i].DisplayOrder
                    });
                }

                await _imageService.UploadMultipleImagesAsync(images, uploadDtos, clientCode);
            }

            return product;
        }

        public async Task<UserFriendlyProductResponseDto> UpdateProductAsync(int productId, UserFriendlyUpdateProductDto updateDto, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var product = await context.ProductDetails
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            // Update master data if provided
            if (!string.IsNullOrWhiteSpace(updateDto.CategoryName))
                product.CategoryId = await GetOrCreateCategoryAsync(context, updateDto.CategoryName);

            if (!string.IsNullOrWhiteSpace(updateDto.BranchName))
                product.BranchId = await GetOrCreateBranchAsync(context, updateDto.BranchName, clientCode);

            if (!string.IsNullOrWhiteSpace(updateDto.CounterName))
                product.CounterId = await GetOrCreateCounterAsync(context, updateDto.CounterName, product.BranchId, clientCode);

            if (!string.IsNullOrWhiteSpace(updateDto.ProductName))
                product.ProductId = await GetOrCreateProductAsync(context, updateDto.ProductName);

            if (!string.IsNullOrWhiteSpace(updateDto.DesignName))
                product.DesignId = await GetOrCreateDesignAsync(context, updateDto.DesignName);

            if (!string.IsNullOrWhiteSpace(updateDto.PurityName))
                product.PurityId = await GetOrCreatePurityAsync(context, updateDto.PurityName);

            // Handle RFID code update if provided
            if (!string.IsNullOrWhiteSpace(updateDto.RfidCode))
            {
                // Check if this RFID is already assigned to another active product
                var existingActiveAssignment = await context.ProductRfidAssignments
                    .Include(pa => pa.Product)
                    .FirstOrDefaultAsync(pa => pa.RFIDCode == updateDto.RfidCode && pa.IsActive && pa.Product.Status.ToLower() == "active");

                if (existingActiveAssignment != null && existingActiveAssignment.ProductId != productId)
                {
                    throw new InvalidOperationException($"RFID code '{updateDto.RfidCode}' is already assigned to active product '{existingActiveAssignment.Product.ItemCode}'. An RFID code can only be assigned to one active product at a time.");
                }

                var rfidCode = await GetOrCreateRfidAsync(context, updateDto.RfidCode, clientCode);
                
                // Deactivate existing RFID assignment for this product if any
                var existingAssignment = await context.ProductRfidAssignments
                    .FirstOrDefaultAsync(pa => pa.ProductId == productId && pa.IsActive);
                
                if (existingAssignment != null)
                {
                    existingAssignment.IsActive = false;
                    existingAssignment.UnassignedOn = DateTime.UtcNow;
                }

                // Create new RFID assignment
                var newAssignment = new ProductRfidAssignment
                {
                    ProductId = productId,
                    RFIDCode = rfidCode,
                    AssignedOn = DateTime.UtcNow,
                    IsActive = true
                };
                context.ProductRfidAssignments.Add(newAssignment);
            }

            // Update other fields
            if (updateDto.GrossWeight.HasValue) product.GrossWeight = updateDto.GrossWeight;
            if (updateDto.StoneWeight.HasValue) product.StoneWeight = updateDto.StoneWeight;
            if (updateDto.DiamondHeight.HasValue) product.DiamondHeight = updateDto.DiamondHeight;
            if (updateDto.NetWeight.HasValue) product.NetWeight = updateDto.NetWeight;
            if (updateDto.BoxDetails != null) product.BoxDetails = updateDto.BoxDetails;
            if (updateDto.Size.HasValue) product.Size = updateDto.Size;
            if (updateDto.StoneAmount.HasValue) product.StoneAmount = updateDto.StoneAmount;
            if (updateDto.DiamondAmount.HasValue) product.DiamondAmount = updateDto.DiamondAmount;
            if (updateDto.HallmarkAmount.HasValue) product.HallmarkAmount = updateDto.HallmarkAmount;
            if (updateDto.MakingPerGram.HasValue) product.MakingPerGram = updateDto.MakingPerGram;
            if (updateDto.MakingPercentage.HasValue) product.MakingPercentage = updateDto.MakingPercentage;
            if (updateDto.MakingFixedAmount.HasValue) product.MakingFixedAmount = updateDto.MakingFixedAmount;
            if (updateDto.Mrp.HasValue) product.Mrp = updateDto.Mrp;
            if (updateDto.ImageUrl != null) product.ImageUrl = updateDto.ImageUrl;
            if (updateDto.Status != null) product.Status = updateDto.Status;

            await context.SaveChangesAsync();

            return await MapToUserFriendlyResponseAsync(context, product);
        }

        public async Task<UserFriendlyProductResponseDto> UpdateProductWithImagesAsync(int productId, UserFriendlyUpdateProductWithImagesDto updateDto, List<IFormFile>? newImages, string clientCode)
        {
            // First update the product
            var productDto = new UserFriendlyUpdateProductDto
            {
                CategoryName = updateDto.CategoryName,
                BranchName = updateDto.BranchName,
                CounterName = updateDto.CounterName,
                ProductName = updateDto.ProductName,
                DesignName = updateDto.DesignName,
                PurityName = updateDto.PurityName,
                RfidCode = updateDto.RfidCode,
                GrossWeight = updateDto.GrossWeight,
                StoneWeight = updateDto.StoneWeight,
                DiamondHeight = updateDto.DiamondHeight,
                NetWeight = updateDto.NetWeight,
                BoxDetails = updateDto.BoxDetails,
                Size = updateDto.Size,
                StoneAmount = updateDto.StoneAmount,
                DiamondAmount = updateDto.DiamondAmount,
                HallmarkAmount = updateDto.HallmarkAmount,
                MakingPerGram = updateDto.MakingPerGram,
                MakingPercentage = updateDto.MakingPercentage,
                MakingFixedAmount = updateDto.MakingFixedAmount,
                Mrp = updateDto.Mrp,
                ImageUrl = updateDto.ImageUrl,
                Status = updateDto.Status
            };

            var product = await UpdateProductAsync(productId, productDto, clientCode);

            // Handle image removals if specified
            if (updateDto.ImageIdsToRemove != null && updateDto.ImageIdsToRemove.Any())
            {
                await _imageService.BulkDeleteImagesAsync(updateDto.ImageIdsToRemove, clientCode);
            }

            // Handle new image uploads if provided
            if (newImages != null && newImages.Any() && updateDto.ImagesToAdd != null)
            {
                var uploadDtos = new List<ProductImageUploadDto>();
                for (int i = 0; i < Math.Min(newImages.Count, updateDto.ImagesToAdd.Count); i++)
                {
                    uploadDtos.Add(new ProductImageUploadDto
                    {
                        ProductId = productId,
                        ImageType = updateDto.ImagesToAdd[i].ImageType,
                        DisplayOrder = updateDto.ImagesToAdd[i].DisplayOrder
                    });
                }

                await _imageService.UploadMultipleImagesAsync(newImages, uploadDtos, clientCode);
            }

            return product;
        }

        public async Task<UserFriendlyProductResponseDto?> GetProductAsync(int productId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var product = await context.ProductDetails
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return null;

            return await MapToUserFriendlyResponseAsync(context, product);
        }

        public async Task<List<UserFriendlyProductResponseDto>> GetAllProductsAsync(string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var products = await context.ProductDetails.ToListAsync();
            var result = new List<UserFriendlyProductResponseDto>();

            foreach (var product in products)
            {
                result.Add(await MapToUserFriendlyResponseAsync(context, product));
            }

            return result;
        }

        public async Task<BulkProductResponseDto> CreateBulkProductsAsync(BulkCreateProductsDto bulkDto, string clientCode)
        {
            var response = new BulkProductResponseDto
            {
                TotalProducts = bulkDto.Products.Count
            };

            foreach (var productDto in bulkDto.Products)
            {
                try
                {
                    var createdProduct = await CreateProductAsync(productDto, clientCode);
                    response.CreatedProducts.Add(createdProduct);
                    response.SuccessfullyCreated++;
                }
                catch (Exception ex)
                {
                    response.Failed++;
                    response.Errors.Add($"Failed to create product '{productDto.ItemCode}': {ex.Message}");
                }
            }

            return response;
        }

        public async Task<bool> DeleteProductAsync(int productId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var product = await context.ProductDetails
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return false;

            context.ProductDetails.Remove(product);
            await context.SaveChangesAsync();

            return true;
        }

        #region Private Helper Methods

        private async Task<int> GetOrCreateCategoryAsync(ClientDbContext context, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new ArgumentException("CategoryName cannot be empty");

            var category = await context.CategoryMasters
                .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == categoryName.ToLower());

            if (category == null)
            {
                category = new CategoryMaster
                {
                    CategoryName = categoryName.Trim()
                };
                context.CategoryMasters.Add(category);
                await context.SaveChangesAsync();
            }

            return category.CategoryId;
        }

        private async Task<int> GetOrCreateBranchAsync(ClientDbContext context, string branchName, string clientCode)
        {
            if (string.IsNullOrWhiteSpace(branchName))
                throw new ArgumentException("BranchName cannot be empty");

            var branch = await context.BranchMasters
                .FirstOrDefaultAsync(b => b.BranchName.ToLower() == branchName.ToLower());

            if (branch == null)
            {
                branch = new BranchMaster
                {
                    BranchName = branchName.Trim(),
                    ClientCode = clientCode
                };
                context.BranchMasters.Add(branch);
                await context.SaveChangesAsync();
            }

            return branch.BranchId;
        }

        private async Task<int> GetOrCreateCounterAsync(ClientDbContext context, string counterName, int branchId, string clientCode)
        {
            if (string.IsNullOrWhiteSpace(counterName))
                throw new ArgumentException("CounterName cannot be empty");

            var counter = await context.CounterMasters
                .FirstOrDefaultAsync(c => c.CounterName.ToLower() == counterName.ToLower());

            if (counter == null)
            {
                counter = new CounterMaster
                {
                    CounterName = counterName.Trim(),
                    BranchId = branchId,
                    ClientCode = clientCode
                };
                context.CounterMasters.Add(counter);
                await context.SaveChangesAsync();
            }

            return counter.CounterId;
        }

        private async Task<int> GetOrCreateProductAsync(ClientDbContext context, string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("ProductName cannot be empty");

            var product = await context.ProductMasters
                .FirstOrDefaultAsync(p => p.ProductName.ToLower() == productName.ToLower());

            if (product == null)
            {
                product = new ProductMaster
                {
                    ProductName = productName.Trim()
                };
                context.ProductMasters.Add(product);
                await context.SaveChangesAsync();
            }

            return product.ProductId;
        }

        private async Task<int> GetOrCreateDesignAsync(ClientDbContext context, string designName)
        {
            if (string.IsNullOrWhiteSpace(designName))
                throw new ArgumentException("DesignName cannot be empty");

            var design = await context.DesignMasters
                .FirstOrDefaultAsync(d => d.DesignName.ToLower() == designName.ToLower());

            if (design == null)
            {
                design = new DesignMaster
                {
                    DesignName = designName.Trim()
                };
                context.DesignMasters.Add(design);
                await context.SaveChangesAsync();
            }

            return design.DesignId;
        }

        private async Task<int> GetOrCreatePurityAsync(ClientDbContext context, string purityName)
        {
            if (string.IsNullOrWhiteSpace(purityName))
                throw new ArgumentException("PurityName cannot be empty");

            var purity = await context.PurityMasters
                .FirstOrDefaultAsync(p => p.PurityName.ToLower() == purityName.ToLower());

            if (purity == null)
            {
                purity = new PurityMaster
                {
                    PurityName = purityName.Trim()
                };
                context.PurityMasters.Add(purity);
                await context.SaveChangesAsync();
            }

            return purity.PurityId;
        }

        private async Task<string> GetOrCreateRfidAsync(ClientDbContext context, string rfidCode, string clientCode)
        {
            if (string.IsNullOrWhiteSpace(rfidCode))
                throw new ArgumentException("RfidCode cannot be empty");

            var rfid = await context.Rfids
                .FirstOrDefaultAsync(r => r.RFIDCode == rfidCode);

            if (rfid == null)
            {
                // Create new RFID entry
                rfid = new Rfid
                {
                    RFIDCode = rfidCode.Trim(),
                    EPCValue = rfidCode.Trim(), // Using RFID code as EPC value for simplicity
                    ClientCode = clientCode,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };
                context.Rfids.Add(rfid);
                await context.SaveChangesAsync();
            }

            // Check if this RFID is already assigned to an active product
            var existingActiveAssignment = await context.ProductRfidAssignments
                .Include(pa => pa.Product)
                .FirstOrDefaultAsync(pa => pa.RFIDCode == rfidCode && pa.IsActive && pa.Product.Status.ToLower() == "active");

            if (existingActiveAssignment != null)
            {
                throw new InvalidOperationException($"RFID code '{rfidCode}' is already assigned to active product '{existingActiveAssignment.Product.ItemCode}'. An RFID code can only be assigned to one active product at a time.");
            }

            return rfid.RFIDCode;
        }

        private async Task<UserFriendlyProductResponseDto> MapToUserFriendlyResponseAsync(ClientDbContext context, ProductDetails product)
        {
            // Get master data names
            var category = await context.CategoryMasters.FirstOrDefaultAsync(c => c.CategoryId == product.CategoryId);
            var branch = await context.BranchMasters.FirstOrDefaultAsync(b => b.BranchId == product.BranchId);
            var counter = await context.CounterMasters.FirstOrDefaultAsync(c => c.CounterId == product.CounterId);
            var productMaster = await context.ProductMasters.FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
            var design = await context.DesignMasters.FirstOrDefaultAsync(d => d.DesignId == product.DesignId);
            var purity = await context.PurityMasters.FirstOrDefaultAsync(p => p.PurityId == product.PurityId);

            // Get RFID code if assigned
            var rfidAssignment = await context.ProductRfidAssignments
                .FirstOrDefaultAsync(pa => pa.ProductId == product.Id && pa.IsActive);

            // Get primary image URL if available
            string? imageUrl = null;
            var primaryImage = await context.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductId == product.Id && pi.ImageType == "Primary" && pi.IsActive);
            
            if (primaryImage != null)
            {
                imageUrl = await _imageService.GetImageUrlAsync(primaryImage.FilePath);
            }

            return new UserFriendlyProductResponseDto
            {
                Id = product.Id,
                ItemCode = product.ItemCode,
                ClientCode = product.ClientCode,
                CategoryName = category?.CategoryName ?? "Unknown",
                BranchName = branch?.BranchName ?? "Unknown",
                CounterName = counter?.CounterName ?? "Unknown",
                ProductName = productMaster?.ProductName ?? "Unknown",
                DesignName = design?.DesignName ?? "Unknown",
                PurityName = purity?.PurityName ?? "Unknown",
                RfidCode = rfidAssignment?.RFIDCode,
                GrossWeight = product.GrossWeight,
                StoneWeight = product.StoneWeight,
                DiamondHeight = product.DiamondHeight,
                NetWeight = product.NetWeight,
                BoxDetails = product.BoxDetails,
                Size = product.Size,
                StoneAmount = product.StoneAmount,
                DiamondAmount = product.DiamondAmount,
                HallmarkAmount = product.HallmarkAmount,
                MakingPerGram = product.MakingPerGram,
                MakingPercentage = product.MakingPercentage,
                MakingFixedAmount = product.MakingFixedAmount,
                Mrp = product.Mrp,
                ImageUrl = imageUrl,
                Status = product.Status,
                CreatedOn = product.CreatedOn,
                CategoryId = product.CategoryId,
                BranchId = product.BranchId,
                CounterId = product.CounterId,
                ProductId = product.ProductId,
                DesignId = product.DesignId,
                PurityId = product.PurityId
            };
        }

        #endregion
    }
}
