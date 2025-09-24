using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;
using System.Security.Claims;

namespace RfidAppApi.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly ClientDbContextFactory _contextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MasterDataService(ClientDbContextFactory contextFactory, IHttpContextAccessor httpContextAccessor)
        {
            _contextFactory = contextFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<ClientDbContext> GetClientContextAsync()
        {
            var clientCode = _httpContextAccessor.HttpContext?.User?.FindFirst("ClientCode")?.Value;
            if (string.IsNullOrEmpty(clientCode))
                throw new UnauthorizedAccessException("Client code not found in user claims");

            return await _contextFactory.CreateAsync(clientCode);
        }

        #region Category Master Operations

        public async Task<IEnumerable<CategoryMasterDto>> GetAllCategoriesAsync()
        {
            using var context = await GetClientContextAsync();
            var categories = await context.CategoryMasters.ToListAsync();
            return categories.Select(c => new CategoryMasterDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            });
        }

        public async Task<CategoryMasterDto?> GetCategoryByIdAsync(int categoryId)
        {
            using var context = await GetClientContextAsync();
            var category = await context.CategoryMasters.FindAsync(categoryId);
            if (category == null) return null;

            return new CategoryMasterDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName
            };
        }

        public async Task<CategoryMasterDto> CreateCategoryAsync(CreateCategoryMasterDto createDto)
        {
            using var context = await GetClientContextAsync();
            var category = new CategoryMaster
            {
                CategoryName = createDto.CategoryName
            };

            context.CategoryMasters.Add(category);
            await context.SaveChangesAsync();

            return new CategoryMasterDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName
            };
        }

        public async Task<CategoryMasterDto> UpdateCategoryAsync(UpdateCategoryMasterDto updateDto)
        {
            using var context = await GetClientContextAsync();
            var category = await context.CategoryMasters.FindAsync(updateDto.CategoryId);
            if (category == null)
                throw new ArgumentException("Category not found");

            category.CategoryName = updateDto.CategoryName;
            await context.SaveChangesAsync();

            return new CategoryMasterDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName
            };
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            using var context = await GetClientContextAsync();
            var category = await context.CategoryMasters.FindAsync(categoryId);
            if (category == null) return false;

            context.CategoryMasters.Remove(category);
            await context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Purity Master Operations

        public async Task<IEnumerable<PurityMasterDto>> GetAllPuritiesAsync()
        {
            using var context = await GetClientContextAsync();
            var purities = await context.PurityMasters.ToListAsync();
            return purities.Select(p => new PurityMasterDto
            {
                PurityId = p.PurityId,
                PurityName = p.PurityName
            });
        }

        public async Task<PurityMasterDto?> GetPurityByIdAsync(int purityId)
        {
            using var context = await GetClientContextAsync();
            var purity = await context.PurityMasters.FindAsync(purityId);
            if (purity == null) return null;

            return new PurityMasterDto
            {
                PurityId = purity.PurityId,
                PurityName = purity.PurityName
            };
        }

        public async Task<PurityMasterDto> CreatePurityAsync(CreatePurityMasterDto createDto)
        {
            using var context = await GetClientContextAsync();
            var purity = new PurityMaster
            {
                PurityName = createDto.PurityName
            };

            context.PurityMasters.Add(purity);
            await context.SaveChangesAsync();

            return new PurityMasterDto
            {
                PurityId = purity.PurityId,
                PurityName = purity.PurityName
            };
        }

        public async Task<PurityMasterDto> UpdatePurityAsync(UpdatePurityMasterDto updateDto)
        {
            using var context = await GetClientContextAsync();
            var purity = await context.PurityMasters.FindAsync(updateDto.PurityId);
            if (purity == null)
                throw new ArgumentException("Purity not found");

            purity.PurityName = updateDto.PurityName;
            await context.SaveChangesAsync();

            return new PurityMasterDto
            {
                PurityId = purity.PurityId,
                PurityName = purity.PurityName
            };
        }

        public async Task<bool> DeletePurityAsync(int purityId)
        {
            using var context = await GetClientContextAsync();
            var purity = await context.PurityMasters.FindAsync(purityId);
            if (purity == null) return false;

            context.PurityMasters.Remove(purity);
            await context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Design Master Operations

        public async Task<IEnumerable<DesignMasterDto>> GetAllDesignsAsync()
        {
            using var context = await GetClientContextAsync();
            var designs = await context.DesignMasters.ToListAsync();
            return designs.Select(d => new DesignMasterDto
            {
                DesignId = d.DesignId,
                DesignName = d.DesignName
            });
        }

        public async Task<DesignMasterDto?> GetDesignByIdAsync(int designId)
        {
            using var context = await GetClientContextAsync();
            var design = await context.DesignMasters.FindAsync(designId);
            if (design == null) return null;

            return new DesignMasterDto
            {
                DesignId = design.DesignId,
                DesignName = design.DesignName
            };
        }

        public async Task<DesignMasterDto> CreateDesignAsync(CreateDesignMasterDto createDto)
        {
            using var context = await GetClientContextAsync();
            var design = new DesignMaster
            {
                DesignName = createDto.DesignName
            };

            context.DesignMasters.Add(design);
            await context.SaveChangesAsync();

            return new DesignMasterDto
            {
                DesignId = design.DesignId,
                DesignName = design.DesignName
            };
        }

        public async Task<DesignMasterDto> UpdateDesignAsync(UpdateDesignMasterDto updateDto)
        {
            using var context = await GetClientContextAsync();
            var design = await context.DesignMasters.FindAsync(updateDto.DesignId);
            if (design == null)
                throw new ArgumentException("Design not found");

            design.DesignName = updateDto.DesignName;
            await context.SaveChangesAsync();

            return new DesignMasterDto
            {
                DesignId = design.DesignId,
                DesignName = design.DesignName
            };
        }

        public async Task<bool> DeleteDesignAsync(int designId)
        {
            using var context = await GetClientContextAsync();
            var design = await context.DesignMasters.FindAsync(designId);
            if (design == null) return false;

            context.DesignMasters.Remove(design);
            await context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Box Master Operations

        public async Task<IEnumerable<BoxMasterDto>> GetAllBoxesAsync()
        {
            using var context = await GetClientContextAsync();
            var boxes = await context.BoxMasters.ToListAsync();
            return boxes.Select(b => new BoxMasterDto
            {
                BoxId = b.BoxId,
                BoxName = b.BoxName,
                Description = b.Description,
                BoxType = b.BoxType,
                Size = b.Size,
                Color = b.Color,
                Material = b.Material,
                IsActive = b.IsActive,
                CreatedOn = b.CreatedOn,
                UpdatedOn = b.UpdatedOn
            });
        }

        public async Task<BoxMasterDto?> GetBoxByIdAsync(int boxId)
        {
            using var context = await GetClientContextAsync();
            var box = await context.BoxMasters.FindAsync(boxId);
            if (box == null) return null;

            return new BoxMasterDto
            {
                BoxId = box.BoxId,
                BoxName = box.BoxName,
                Description = box.Description,
                BoxType = box.BoxType,
                Size = box.Size,
                Color = box.Color,
                Material = box.Material,
                IsActive = box.IsActive,
                CreatedOn = box.CreatedOn,
                UpdatedOn = box.UpdatedOn
            };
        }

        public async Task<BoxMasterDto> CreateBoxAsync(CreateBoxMasterDto createDto)
        {
            using var context = await GetClientContextAsync();
            var box = new BoxMaster
            {
                BoxName = createDto.BoxName,
                Description = createDto.Description,
                BoxType = createDto.BoxType,
                Size = createDto.Size,
                Color = createDto.Color,
                Material = createDto.Material,
                IsActive = createDto.IsActive,
                CreatedOn = DateTime.UtcNow
            };

            context.BoxMasters.Add(box);
            await context.SaveChangesAsync();

            return new BoxMasterDto
            {
                BoxId = box.BoxId,
                BoxName = box.BoxName,
                Description = box.Description,
                BoxType = box.BoxType,
                Size = box.Size,
                Color = box.Color,
                Material = box.Material,
                IsActive = box.IsActive,
                CreatedOn = box.CreatedOn,
                UpdatedOn = box.UpdatedOn
            };
        }

        public async Task<BoxMasterDto> UpdateBoxAsync(UpdateBoxMasterDto updateDto)
        {
            using var context = await GetClientContextAsync();
            var box = await context.BoxMasters.FindAsync(updateDto.BoxId);
            if (box == null)
                throw new ArgumentException("Box not found");

            box.BoxName = updateDto.BoxName;
            box.Description = updateDto.Description;
            box.BoxType = updateDto.BoxType;
            box.Size = updateDto.Size;
            box.Color = updateDto.Color;
            box.Material = updateDto.Material;
            box.IsActive = updateDto.IsActive;
            box.UpdatedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return new BoxMasterDto
            {
                BoxId = box.BoxId,
                BoxName = box.BoxName,
                Description = box.Description,
                BoxType = box.BoxType,
                Size = box.Size,
                Color = box.Color,
                Material = box.Material,
                IsActive = box.IsActive,
                CreatedOn = box.CreatedOn,
                UpdatedOn = box.UpdatedOn
            };
        }

        public async Task<bool> DeleteBoxAsync(int boxId)
        {
            using var context = await GetClientContextAsync();
            var box = await context.BoxMasters.FindAsync(boxId);
            if (box == null) return false;

            context.BoxMasters.Remove(box);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BoxMasterDto>> GetActiveBoxesAsync()
        {
            using var context = await GetClientContextAsync();
            var boxes = await context.BoxMasters.Where(b => b.IsActive).ToListAsync();
            return boxes.Select(b => new BoxMasterDto
            {
                BoxId = b.BoxId,
                BoxName = b.BoxName,
                Description = b.Description,
                BoxType = b.BoxType,
                Size = b.Size,
                Color = b.Color,
                Material = b.Material,
                IsActive = b.IsActive,
                CreatedOn = b.CreatedOn,
                UpdatedOn = b.UpdatedOn
            });
        }

        public async Task<IEnumerable<BoxMasterDto>> GetBoxesByTypeAsync(string boxType)
        {
            using var context = await GetClientContextAsync();
            var boxes = await context.BoxMasters.Where(b => b.BoxType == boxType).ToListAsync();
            return boxes.Select(b => new BoxMasterDto
            {
                BoxId = b.BoxId,
                BoxName = b.BoxName,
                Description = b.Description,
                BoxType = b.BoxType,
                Size = b.Size,
                Color = b.Color,
                Material = b.Material,
                IsActive = b.IsActive,
                CreatedOn = b.CreatedOn,
                UpdatedOn = b.UpdatedOn
            });
        }

        #endregion

        #region Counter Master Operations

        public async Task<IEnumerable<CounterMasterDto>> GetAllCountersAsync()
        {
            using var context = await GetClientContextAsync();
            var counters = await context.CounterMasters
                .Include(c => c.Branch)
                .ToListAsync();
            
            return counters.Select(c => new CounterMasterDto
            {
                CounterId = c.CounterId,
                CounterName = c.CounterName,
                BranchId = c.BranchId,
                ClientCode = c.ClientCode,
                BranchName = c.Branch.BranchName
            });
        }

        public async Task<IEnumerable<CounterMasterDto>> GetCountersByClientAsync(string clientCode)
        {
            using var context = await GetClientContextAsync();
            var counters = await context.CounterMasters
                .Include(c => c.Branch)
                .Where(c => c.ClientCode == clientCode)
                .ToListAsync();
            
            return counters.Select(c => new CounterMasterDto
            {
                CounterId = c.CounterId,
                CounterName = c.CounterName,
                BranchId = c.BranchId,
                ClientCode = c.ClientCode,
                BranchName = c.Branch.BranchName
            });
        }

        public async Task<IEnumerable<CounterMasterDto>> GetCountersByBranchAsync(int branchId)
        {
            using var context = await GetClientContextAsync();
            var counters = await context.CounterMasters
                .Include(c => c.Branch)
                .Where(c => c.BranchId == branchId)
                .ToListAsync();
            
            return counters.Select(c => new CounterMasterDto
            {
                CounterId = c.CounterId,
                CounterName = c.CounterName,
                BranchId = c.BranchId,
                ClientCode = c.ClientCode,
                BranchName = c.Branch.BranchName
            });
        }

        public async Task<CounterMasterDto?> GetCounterByIdAsync(int counterId)
        {
            using var context = await GetClientContextAsync();
            var counter = await context.CounterMasters
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.CounterId == counterId);
            
            if (counter == null) return null;

            return new CounterMasterDto
            {
                CounterId = counter.CounterId,
                CounterName = counter.CounterName,
                BranchId = counter.BranchId,
                ClientCode = counter.ClientCode,
                BranchName = counter.Branch.BranchName
            };
        }

        public async Task<CounterMasterDto> CreateCounterAsync(CreateCounterMasterDto createDto)
        {
            using var context = await GetClientContextAsync();
            var counter = new CounterMaster
            {
                CounterName = createDto.CounterName,
                BranchId = createDto.BranchId,
                ClientCode = createDto.ClientCode
            };

            context.CounterMasters.Add(counter);
            await context.SaveChangesAsync();

            // Get the created counter with branch info
            var createdCounter = await context.CounterMasters
                .Include(c => c.Branch)
                .FirstAsync(c => c.CounterId == counter.CounterId);

            return new CounterMasterDto
            {
                CounterId = createdCounter.CounterId,
                CounterName = createdCounter.CounterName,
                BranchId = createdCounter.BranchId,
                ClientCode = createdCounter.ClientCode,
                BranchName = createdCounter.Branch.BranchName
            };
        }

        public async Task<CounterMasterDto> UpdateCounterAsync(UpdateCounterMasterDto updateDto)
        {
            using var context = await GetClientContextAsync();
            var counter = await context.CounterMasters.FindAsync(updateDto.CounterId);
            if (counter == null)
                throw new ArgumentException("Counter not found");

            counter.CounterName = updateDto.CounterName;
            counter.BranchId = updateDto.BranchId;
            counter.ClientCode = updateDto.ClientCode;

            await context.SaveChangesAsync();

            // Get the updated counter with branch info
            var updatedCounter = await context.CounterMasters
                .Include(c => c.Branch)
                .FirstAsync(c => c.CounterId == counter.CounterId);

            return new CounterMasterDto
            {
                CounterId = updatedCounter.CounterId,
                CounterName = updatedCounter.CounterName,
                BranchId = updatedCounter.BranchId,
                ClientCode = updatedCounter.ClientCode,
                BranchName = updatedCounter.Branch.BranchName
            };
        }

        public async Task<bool> DeleteCounterAsync(int counterId)
        {
            using var context = await GetClientContextAsync();
            var counter = await context.CounterMasters.FindAsync(counterId);
            if (counter == null) return false;

            context.CounterMasters.Remove(counter);
            await context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Branch Master Operations

        public async Task<IEnumerable<BranchMasterDto>> GetAllBranchesAsync()
        {
            using var context = await GetClientContextAsync();
            var branches = await context.BranchMasters.ToListAsync();
            return branches.Select(b => new BranchMasterDto
            {
                BranchId = b.BranchId,
                BranchName = b.BranchName,
                ClientCode = b.ClientCode,
                CounterCount = context.CounterMasters.Count(c => c.BranchId == b.BranchId)
            });
        }

        public async Task<IEnumerable<BranchMasterDto>> GetBranchesByClientAsync(string clientCode)
        {
            using var context = await GetClientContextAsync();
            var branches = await context.BranchMasters
                .Where(b => b.ClientCode == clientCode)
                .ToListAsync();
            
            return branches.Select(b => new BranchMasterDto
            {
                BranchId = b.BranchId,
                BranchName = b.BranchName,
                ClientCode = b.ClientCode,
                CounterCount = context.CounterMasters.Count(c => c.BranchId == b.BranchId)
            });
        }

        public async Task<BranchMasterDto?> GetBranchByIdAsync(int branchId)
        {
            using var context = await GetClientContextAsync();
            var branch = await context.BranchMasters.FindAsync(branchId);
            if (branch == null) return null;

            return new BranchMasterDto
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName,
                ClientCode = branch.ClientCode,
                CounterCount = context.CounterMasters.Count(c => c.BranchId == branch.BranchId)
            };
        }

        public async Task<BranchMasterDto> CreateBranchAsync(CreateBranchMasterDto createDto)
        {
            using var context = await GetClientContextAsync();
            var branch = new BranchMaster
            {
                BranchName = createDto.BranchName,
                ClientCode = createDto.ClientCode
            };

            context.BranchMasters.Add(branch);
            await context.SaveChangesAsync();

            return new BranchMasterDto
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName,
                ClientCode = branch.ClientCode,
                CounterCount = 0
            };
        }

        public async Task<BranchMasterDto> UpdateBranchAsync(UpdateBranchMasterDto updateDto)
        {
            using var context = await GetClientContextAsync();
            var branch = await context.BranchMasters.FindAsync(updateDto.BranchId);
            if (branch == null)
                throw new ArgumentException("Branch not found");

            branch.BranchName = updateDto.BranchName;
            branch.ClientCode = updateDto.ClientCode;

            await context.SaveChangesAsync();

            return new BranchMasterDto
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName,
                ClientCode = branch.ClientCode,
                CounterCount = context.CounterMasters.Count(c => c.BranchId == branch.BranchId)
            };
        }

        public async Task<bool> DeleteBranchAsync(int branchId)
        {
            using var context = await GetClientContextAsync();
            var branch = await context.BranchMasters.FindAsync(branchId);
            if (branch == null) return false;

            // Check if branch has counters
            var hasCounters = await context.CounterMasters.AnyAsync(c => c.BranchId == branchId);
            if (hasCounters)
                throw new InvalidOperationException("Cannot delete branch that has counters");

            context.BranchMasters.Remove(branch);
            await context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Product Master Operations

        public async Task<IEnumerable<ProductMasterDto>> GetAllProductsAsync()
        {
            using var context = await GetClientContextAsync();
            var products = await context.ProductMasters.ToListAsync();
            return products.Select(p => new ProductMasterDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName
            });
        }

        public async Task<ProductMasterDto?> GetProductByIdAsync(int productId)
        {
            using var context = await GetClientContextAsync();
            var product = await context.ProductMasters.FindAsync(productId);
            if (product == null) return null;

            return new ProductMasterDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName
            };
        }

        public async Task<ProductMasterDto> CreateProductAsync(CreateProductMasterDto createDto)
        {
            using var context = await GetClientContextAsync();
            var product = new ProductMaster
            {
                ProductName = createDto.ProductName
            };

            context.ProductMasters.Add(product);
            await context.SaveChangesAsync();

            return new ProductMasterDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName
            };
        }

        public async Task<ProductMasterDto> UpdateProductAsync(UpdateProductMasterDto updateDto)
        {
            using var context = await GetClientContextAsync();
            var product = await context.ProductMasters.FindAsync(updateDto.ProductId);
            if (product == null)
                throw new ArgumentException("Product not found");

            product.ProductName = updateDto.ProductName;
            await context.SaveChangesAsync();

            return new ProductMasterDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName
            };
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            using var context = await GetClientContextAsync();
            var product = await context.ProductMasters.FindAsync(productId);
            if (product == null) return false;

            context.ProductMasters.Remove(product);
            await context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Master Data Summary Operations

        public async Task<MasterDataSummaryDto> GetMasterDataSummaryAsync()
        {
            using var context = await GetClientContextAsync();
            
            return new MasterDataSummaryDto
            {
                TotalCategories = await context.CategoryMasters.CountAsync(),
                TotalPurities = await context.PurityMasters.CountAsync(),
                TotalDesigns = await context.DesignMasters.CountAsync(),
                TotalBoxes = await context.BoxMasters.CountAsync(),
                TotalCounters = await context.CounterMasters.CountAsync(),
                TotalBranches = await context.BranchMasters.CountAsync(),
                TotalProducts = await context.ProductMasters.CountAsync()
            };
        }

        public async Task<IEnumerable<MasterDataCountsDto>> GetMasterDataCountsAsync()
        {
            using var context = await GetClientContextAsync();
            
            var counts = new List<MasterDataCountsDto>
            {
                new MasterDataCountsDto
                {
                    EntityName = "Categories",
                    Count = await context.CategoryMasters.CountAsync(),
                    LastUpdated = DateTime.UtcNow
                },
                new MasterDataCountsDto
                {
                    EntityName = "Purities",
                    Count = await context.PurityMasters.CountAsync(),
                    LastUpdated = DateTime.UtcNow
                },
                new MasterDataCountsDto
                {
                    EntityName = "Designs",
                    Count = await context.DesignMasters.CountAsync(),
                    LastUpdated = DateTime.UtcNow
                },
                new MasterDataCountsDto
                {
                    EntityName = "Boxes",
                    Count = await context.BoxMasters.CountAsync(),
                    LastUpdated = DateTime.UtcNow
                },
                new MasterDataCountsDto
                {
                    EntityName = "Counters",
                    Count = await context.CounterMasters.CountAsync(),
                    LastUpdated = DateTime.UtcNow
                },
                new MasterDataCountsDto
                {
                    EntityName = "Branches",
                    Count = await context.BranchMasters.CountAsync(),
                    LastUpdated = DateTime.UtcNow
                },
                new MasterDataCountsDto
                {
                    EntityName = "Products",
                    Count = await context.ProductMasters.CountAsync(),
                    LastUpdated = DateTime.UtcNow
                }
            };

            return counts;
        }

        public async Task<MasterDataSummaryDto> GetMasterDataSummaryByClientAsync(string clientCode)
        {
            using var context = await GetClientContextAsync();
            
            return new MasterDataSummaryDto
            {
                TotalCategories = await context.CategoryMasters.CountAsync(),
                TotalPurities = await context.PurityMasters.CountAsync(),
                TotalDesigns = await context.DesignMasters.CountAsync(),
                TotalBoxes = await context.BoxMasters.CountAsync(),
                TotalCounters = await context.CounterMasters.CountAsync(c => c.ClientCode == clientCode),
                TotalBranches = await context.BranchMasters.CountAsync(b => b.ClientCode == clientCode),
                TotalProducts = await context.ProductMasters.CountAsync()
            };
        }

        #endregion
    }
}
