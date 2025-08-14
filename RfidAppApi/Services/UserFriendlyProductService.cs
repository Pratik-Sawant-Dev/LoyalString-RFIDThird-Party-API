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

        /// <summary>
        /// HIGHLY OPTIMIZED bulk product creation for large datasets (20k+ products)
        /// Implements batch processing, master data caching, and bulk database operations
        /// Expected performance: 20k products in 2-5 minutes instead of 14+ minutes
        /// </summary>
        public async Task<BulkProductResponseDto> CreateBulkProductsAsync(BulkCreateProductsDto bulkDto, string clientCode)
        {
            var response = new BulkProductResponseDto
            {
                TotalProducts = bulkDto.Products.Count
            };

            if (bulkDto.Products.Count == 0)
                return response;

            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            try
            {
                // STEP 1: Pre-load and cache all master data to avoid repeated database calls
                var masterDataCache = await PreloadMasterDataAsync(context, bulkDto.Products, clientCode);
                
                // STEP 2: Validate all products before processing (fail fast approach)
                var validationResults = await ValidateBulkProductsAsync(context, bulkDto.Products, masterDataCache);
                
                if (validationResults.HasErrors)
                {
                    response.Failed = validationResults.Errors.Count;
                    response.Errors.AddRange(validationResults.Errors);
                    return response;
                }

                // STEP 3: Process products in smaller batches to avoid memory issues
                const int batchSize = 500; // Reduced batch size for better memory management
                var totalBatches = (int)Math.Ceiling((double)bulkDto.Products.Count / batchSize);
                
                for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
                {
                    List<UserFriendlyCreateProductDto> batchProducts = null;
                    try
                    {
                        batchProducts = bulkDto.Products
                            .Skip(batchIndex * batchSize)
                            .Take(batchSize)
                            .ToList();

                        await ProcessProductBatchAsync(context, batchProducts, masterDataCache, response, clientCode);
                        
                        // Clear change tracker after each batch to prevent memory buildup
                        context.ChangeTracker.Clear();
                        
                        // Log progress for large operations
                        if (totalBatches > 5)
                        {
                            var progress = ((batchIndex + 1) * 100.0 / totalBatches);
                            Console.WriteLine($"Bulk Product Creation Progress: {progress:F1}% ({batchIndex + 1}/{totalBatches} batches completed)");
                        }
                    }
                    catch (Exception batchEx)
                    {
                        // If a batch fails, log the error but continue with remaining batches
                        if (batchProducts != null)
                        {
                            response.Failed += batchProducts.Count;
                        }
                        response.Errors.Add($"Batch {batchIndex + 1} failed: {batchEx.Message}");
                        
                        // Clear the failed batch from change tracker
                        context.ChangeTracker.Clear();
                        
                        // Continue with next batch instead of failing completely
                        continue;
                    }
                }
                
                return response;
            }
            catch (Exception ex)
            {
                response.Failed = bulkDto.Products.Count;
                response.Errors.Add($"Bulk operation failed: {ex.Message}");
                
                // Log the full exception for debugging
                Console.WriteLine($"Bulk Product Creation Error: {ex}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException}");
                }
                
                throw;
            }
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

        /// <summary>
        /// Pre-loads all master data into memory to avoid repeated database calls
        /// </summary>
        private async Task<MasterDataCache> PreloadMasterDataAsync(ClientDbContext context, List<UserFriendlyCreateProductDto> products, string clientCode)
        {
            var cache = new MasterDataCache();

            // Extract unique values from all products
            var uniqueCategories = products.Select(p => p.CategoryName?.Trim().ToLower()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var uniqueBranches = products.Select(p => p.BranchName?.Trim().ToLower()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var uniqueCounters = products.Select(p => p.CounterName?.Trim().ToLower()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var uniqueProductNames = products.Select(p => p.ProductName?.Trim().ToLower()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var uniqueDesigns = products.Select(p => p.DesignName?.Trim().ToLower()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var uniquePurities = products.Select(p => p.PurityName?.Trim().ToLower()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var uniqueRfids = products.Select(p => p.RfidCode?.Trim()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            // Load existing master data
            var existingCategories = await context.CategoryMasters
                .Where(c => uniqueCategories.Contains(c.CategoryName.ToLower()))
                .ToListAsync();

            var existingBranches = await context.BranchMasters
                .Where(b => uniqueBranches.Contains(b.BranchName.ToLower()) && b.ClientCode == clientCode)
                .ToListAsync();

            var existingCounters = await context.CounterMasters
                .Where(c => uniqueCounters.Contains(c.CounterName.ToLower()))
                .ToListAsync();

            var existingProducts = await context.ProductMasters
                .Where(p => uniqueProductNames.Contains(p.ProductName.ToLower()))
                .ToListAsync();

            var existingDesigns = await context.DesignMasters
                .Where(d => uniqueDesigns.Contains(d.DesignName.ToLower()))
                .ToListAsync();

            var existingPurities = await context.PurityMasters
                .Where(p => uniquePurities.Contains(p.PurityName.ToLower()))
                .ToListAsync();

            var existingRfids = await context.Rfids
                .Where(r => uniqueRfids.Contains(r.RFIDCode) && r.ClientCode == clientCode)
                .ToListAsync();

            // Build cache dictionaries
            cache.Categories = existingCategories.ToDictionary(c => c.CategoryName.ToLower(), c => c.CategoryId);
            cache.Branches = existingBranches.ToDictionary(b => b.BranchName.ToLower(), b => b.BranchId);
            cache.Counters = existingCounters.ToDictionary(c => c.CounterName.ToLower(), c => c.CounterId);
            cache.Products = existingProducts.ToDictionary(p => p.ProductName.ToLower(), p => p.ProductId);
            cache.Designs = existingDesigns.ToDictionary(d => d.DesignName.ToLower(), d => d.DesignId);
            cache.Purities = existingPurities.ToDictionary(p => p.PurityName.ToLower(), p => p.PurityId);
            cache.Rfids = existingRfids.ToDictionary(r => r.RFIDCode, r => r.RFIDCode);

            return cache;
        }

        /// <summary>
        /// Validates all products before processing to fail fast
        /// </summary>
        private async Task<ValidationResult> ValidateBulkProductsAsync(ClientDbContext context, List<UserFriendlyCreateProductDto> products, MasterDataCache masterDataCache)
        {
            var result = new ValidationResult();
            var existingItemCodes = new HashSet<string>();

            // Check for duplicate ItemCodes within the batch
            foreach (var product in products)
            {
                if (string.IsNullOrWhiteSpace(product.ItemCode))
                {
                    result.Errors.Add($"Product at index {products.IndexOf(product)}: ItemCode is required");
                    continue;
                }

                if (existingItemCodes.Contains(product.ItemCode))
                {
                    result.Errors.Add($"Duplicate ItemCode '{product.ItemCode}' found in batch");
                    continue;
                }

                existingItemCodes.Add(product.ItemCode);

                // Check if ItemCode already exists in database
                var exists = await context.ProductDetails.AnyAsync(p => p.ItemCode == product.ItemCode);
                if (exists)
                {
                    result.Errors.Add($"Product with ItemCode '{product.ItemCode}' already exists in database");
                    continue;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(product.CategoryName))
                    result.Errors.Add($"Product '{product.ItemCode}': CategoryName is required");

                if (string.IsNullOrWhiteSpace(product.BranchName))
                    result.Errors.Add($"Product '{product.ItemCode}': BranchName is required");

                if (string.IsNullOrWhiteSpace(product.CounterName))
                    result.Errors.Add($"Product '{product.ItemCode}': CounterName is required");
            }

            result.HasErrors = result.Errors.Count > 0;
            return result;
        }

        /// <summary>
        /// Processes a batch of products efficiently
        /// </summary>
        private async Task ProcessProductBatchAsync(
            ClientDbContext context, 
            List<UserFriendlyCreateProductDto> batchProducts, 
            MasterDataCache masterDataCache, 
            BulkProductResponseDto response, 
            string clientCode)
        {
            var productsToAdd = new List<ProductDetails>();
            var rfidAssignmentsToAdd = new List<ProductRfidAssignment>();
            var newMasterDataToAdd = new List<object>();

            // Process each product in the batch
            foreach (var productDto in batchProducts)
            {
                try
                {
                    // Get or create master data IDs using cache
                    var categoryId = await GetOrCreateCategoryFromCacheAsync(context, productDto.CategoryName, masterDataCache, newMasterDataToAdd);
                    var branchId = await GetOrCreateBranchFromCacheAsync(context, productDto.BranchName, clientCode, masterDataCache, newMasterDataToAdd);
                    var counterId = await GetOrCreateCounterFromCacheAsync(context, productDto.CounterName, branchId, clientCode, masterDataCache, newMasterDataToAdd);
                    var productId = await GetOrCreateProductFromCacheAsync(context, productDto.ProductName, masterDataCache, newMasterDataToAdd);
                    var designId = await GetOrCreateDesignFromCacheAsync(context, productDto.DesignName, masterDataCache, newMasterDataToAdd);
                    var purityId = await GetOrCreatePurityFromCacheAsync(context, productDto.PurityName, masterDataCache, newMasterDataToAdd);

                    // Create product entity
                    var product = new ProductDetails
                    {
                        ClientCode = clientCode,
                        BranchId = branchId,
                        CounterId = counterId,
                        ItemCode = productDto.ItemCode,
                        CategoryId = categoryId,
                        ProductId = productId,
                        DesignId = designId,
                        PurityId = purityId,
                        GrossWeight = productDto.GrossWeight,
                        StoneWeight = productDto.StoneWeight,
                        DiamondHeight = productDto.DiamondHeight,
                        NetWeight = productDto.NetWeight,
                        BoxDetails = productDto.BoxDetails,
                        Size = productDto.Size,
                        StoneAmount = productDto.StoneAmount,
                        DiamondAmount = productDto.DiamondAmount,
                        HallmarkAmount = productDto.HallmarkAmount,
                        MakingPerGram = productDto.MakingPerGram,
                        MakingPercentage = productDto.MakingPercentage,
                        MakingFixedAmount = productDto.MakingFixedAmount,
                        Mrp = productDto.Mrp,
                        ImageUrl = productDto.ImageUrl,
                        Status = productDto.Status ?? "Active",
                        CreatedOn = DateTime.UtcNow
                    };

                    productsToAdd.Add(product);

                    // Handle RFID if provided
                    if (!string.IsNullOrWhiteSpace(productDto.RfidCode))
                    {
                        var rfidCode = await GetOrCreateRfidFromCacheAsync(context, productDto.RfidCode, clientCode, masterDataCache, newMasterDataToAdd);
                        
                        // We'll create RFID assignments after products are saved (to get their IDs)
                        rfidAssignmentsToAdd.Add(new ProductRfidAssignment
                        {
                            RFIDCode = rfidCode,
                            AssignedOn = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                    else
                    {
                        // Add null placeholder for products without RFID
                        rfidAssignmentsToAdd.Add(null);
                    }

                    response.SuccessfullyCreated++;
                }
                catch (Exception ex)
                {
                    response.Failed++;
                    response.Errors.Add($"Failed to process product '{productDto.ItemCode}': {ex.Message}");
                }
            }

            // STEP 1: Add all new master data first and save immediately
            if (newMasterDataToAdd.Any())
            {
                try
                {
                    foreach (var entity in newMasterDataToAdd)
                    {
                        context.Add(entity);
                    }
                    await context.SaveChangesAsync();
                    
                    // Update cache with new IDs
                    UpdateMasterDataCache(masterDataCache, newMasterDataToAdd);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to save master data: {ex.Message}", ex);
                }
            }

            // STEP 2: Add all products and save immediately
            if (productsToAdd.Any())
            {
                try
                {
                    context.ProductDetails.AddRange(productsToAdd);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to save products: {ex.Message}", ex);
                }
            }

            // STEP 3: Create RFID assignments for products that have them
            if (rfidAssignmentsToAdd.Any())
            {
                try
                {
                    var validRfidAssignments = new List<ProductRfidAssignment>();
                    
                    for (int i = 0; i < rfidAssignmentsToAdd.Count; i++)
                    {
                        if (rfidAssignmentsToAdd[i] != null && i < productsToAdd.Count)
                        {
                            var assignment = rfidAssignmentsToAdd[i];
                            assignment.ProductId = productsToAdd[i].Id;
                            validRfidAssignments.Add(assignment);
                        }
                    }

                    if (validRfidAssignments.Any())
                    {
                        context.ProductRfidAssignments.AddRange(validRfidAssignments);
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to save RFID assignments: {ex.Message}", ex);
                }
            }
        }

        // Cache-aware master data methods
        private async Task<int> GetOrCreateCategoryFromCacheAsync(ClientDbContext context, string categoryName, MasterDataCache cache, List<object> newEntities)
        {
            var key = categoryName?.Trim().ToLower();
            if (string.IsNullOrEmpty(key)) return 0;
            
            if (cache.Categories.TryGetValue(key, out int existingId) && existingId > 0)
                return existingId;

            var category = new CategoryMaster { CategoryName = categoryName.Trim() };
            newEntities.Add(category);
            cache.Categories[key] = -1; // Temporary negative ID to mark as new
            return -1;
        }

        private async Task<int> GetOrCreateBranchFromCacheAsync(ClientDbContext context, string branchName, string clientCode, MasterDataCache cache, List<object> newEntities)
        {
            var key = branchName?.Trim().ToLower();
            if (string.IsNullOrEmpty(key)) return 0;
            
            if (cache.Branches.TryGetValue(key, out int existingId) && existingId > 0)
                return existingId;

            var branch = new BranchMaster { BranchName = branchName.Trim(), ClientCode = clientCode };
            newEntities.Add(branch);
            cache.Branches[key] = -1; // Temporary negative ID to mark as new
            return -1;
        }

        private async Task<int> GetOrCreateCounterFromCacheAsync(ClientDbContext context, string counterName, int branchId, string clientCode, MasterDataCache cache, List<object> newEntities)
        {
            var key = counterName?.Trim().ToLower();
            if (string.IsNullOrEmpty(key)) return 0;
            
            if (cache.Counters.TryGetValue(key, out int existingId) && existingId > 0)
                return existingId;

            var counter = new CounterMaster { CounterName = counterName.Trim(), BranchId = branchId, ClientCode = clientCode };
            newEntities.Add(counter);
            cache.Counters[key] = -1; // Temporary negative ID to mark as new
            return -1;
        }

        private async Task<int> GetOrCreateProductFromCacheAsync(ClientDbContext context, string productName, MasterDataCache cache, List<object> newEntities)
        {
            var key = productName?.Trim().ToLower();
            if (string.IsNullOrEmpty(key)) return 0;
            
            if (cache.Products.TryGetValue(key, out int existingId) && existingId > 0)
                return existingId;

            var product = new ProductMaster { ProductName = productName.Trim() };
            newEntities.Add(product);
            cache.Products[key] = -1; // Temporary negative ID to mark as new
            return -1;
        }

        private async Task<int> GetOrCreateDesignFromCacheAsync(ClientDbContext context, string designName, MasterDataCache cache, List<object> newEntities)
        {
            var key = designName?.Trim().ToLower();
            if (string.IsNullOrEmpty(key)) return 0;
            
            if (cache.Designs.TryGetValue(key, out int existingId) && existingId > 0)
                return existingId;

            var design = new DesignMaster { DesignName = designName.Trim() };
            newEntities.Add(design);
            cache.Designs[key] = -1; // Temporary negative ID to mark as new
            return -1;
        }

        private async Task<int> GetOrCreatePurityFromCacheAsync(ClientDbContext context, string purityName, MasterDataCache cache, List<object> newEntities)
        {
            var key = purityName?.Trim().ToLower();
            if (string.IsNullOrEmpty(key)) return 0;
            
            if (cache.Purities.TryGetValue(key, out int existingId) && existingId > 0)
                return existingId;

            var purity = new PurityMaster { PurityName = purityName.Trim() };
            newEntities.Add(purity);
            cache.Purities[key] = -1; // Temporary negative ID to mark as new
            return -1;
        }

        private async Task<string> GetOrCreateRfidFromCacheAsync(ClientDbContext context, string rfidCode, string clientCode, MasterDataCache cache, List<object> newEntities)
        {
            if (string.IsNullOrEmpty(rfidCode)) return string.Empty;
            
            if (cache.Rfids.TryGetValue(rfidCode, out string existingCode))
                return existingCode;

            var rfid = new Rfid
            {
                RFIDCode = rfidCode.Trim(),
                EPCValue = rfidCode.Trim(),
                ClientCode = clientCode,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };
            newEntities.Add(rfid);
            cache.Rfids[rfidCode] = rfidCode;
            return rfidCode;
        }

        private void UpdateMasterDataCache(MasterDataCache cache, List<object> newEntities)
        {
            foreach (var entity in newEntities)
            {
                switch (entity)
                {
                    case CategoryMaster category:
                        var categoryKey = category.CategoryName.ToLower();
                        if (cache.Categories.ContainsKey(categoryKey) && cache.Categories[categoryKey] == -1)
                            cache.Categories[categoryKey] = category.CategoryId;
                        break;
                    case BranchMaster branch:
                        var branchKey = branch.BranchName.ToLower();
                        if (cache.Branches.ContainsKey(branchKey) && cache.Branches[branchKey] == -1)
                            cache.Branches[branchKey] = branch.BranchId;
                        break;
                    case CounterMaster counter:
                        var counterKey = counter.CounterName.ToLower();
                        if (cache.Counters.ContainsKey(counterKey) && cache.Counters[counterKey] == -1)
                            cache.Counters[counterKey] = counter.CounterId;
                        break;
                    case ProductMaster product:
                        var productKey = product.ProductName.ToLower();
                        if (cache.Products.ContainsKey(productKey) && cache.Products[productKey] == -1)
                            cache.Products[productKey] = product.ProductId;
                        break;
                    case DesignMaster design:
                        var designKey = design.DesignName.ToLower();
                        if (cache.Designs.ContainsKey(designKey) && cache.Designs[designKey] == -1)
                            cache.Designs[designKey] = design.DesignId;
                        break;
                    case PurityMaster purity:
                        var purityKey = purity.PurityName.ToLower();
                        if (cache.Purities.ContainsKey(purityKey) && cache.Purities[purityKey] == -1)
                            cache.Purities[purityKey] = purity.PurityId;
                        break;
                }
            }
        }

        // Legacy methods for single product operations
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
                .FirstOrDefaultAsync(b => b.CounterName.ToLower() == counterName.ToLower());

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
                .FirstOrDefaultAsync(p => p.DesignName.ToLower() == designName.ToLower());

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

    #region Helper Classes for Bulk Operations

    /// <summary>
    /// Cache for master data to avoid repeated database calls during bulk operations
    /// </summary>
    public class MasterDataCache
        {
            public Dictionary<string, int> Categories { get; set; } = new();
            public Dictionary<string, int> Branches { get; set; } = new();
            public Dictionary<string, int> Counters { get; set; } = new();
            public Dictionary<string, int> Products { get; set; } = new();
            public Dictionary<string, int> Designs { get; set; } = new();
        public Dictionary<string, int> Purities { get; set; } = new();
        public Dictionary<string, string> Rfids { get; set; } = new();
    }

    /// <summary>
    /// Result of bulk validation
    /// </summary>
    public class ValidationResult
    {
        public bool HasErrors { get; set; }
        public List<string> Errors { get; set; } = new();
        }

        #endregion
}
