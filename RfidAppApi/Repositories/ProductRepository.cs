using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.Models;

namespace RfidAppApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ClientDbContextFactory _contextFactory;

        public ProductRepository(ClientDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        private async Task<ClientDbContext> GetContextAsync(string clientCode)
        {
            return await _contextFactory.CreateAsync(clientCode);
        }

        public async Task<IEnumerable<ProductDetails>> GetAllAsync(string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductDetails
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Include(p => p.Design)
                .Include(p => p.Purity)
                .Include(p => p.Branch)
                .Include(p => p.Counter)
                .ToListAsync();
        }

        public async Task<ProductDetails?> GetByIdAsync(int id, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductDetails
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Include(p => p.Design)
                .Include(p => p.Purity)
                .Include(p => p.Branch)
                .Include(p => p.Counter)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ProductDetails> AddAsync(ProductDetails product, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            context.ProductDetails.Add(product);
            await context.SaveChangesAsync();
            return product;
        }

        public async Task<ProductDetails> UpdateAsync(ProductDetails product, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            context.ProductDetails.Update(product);
            await context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(int id, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            var product = await context.ProductDetails.FindAsync(id);
            if (product == null) return false;

            context.ProductDetails.Remove(product);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProductDetails>> GetByClientCodeAsync(string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductDetails
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Include(p => p.Design)
                .Include(p => p.Purity)
                .Include(p => p.Branch)
                .Include(p => p.Counter)
                .ToListAsync();
        }

        public async Task<ProductDetails?> GetByItemCodeAsync(string itemCode, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductDetails
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Include(p => p.Design)
                .Include(p => p.Purity)
                .Include(p => p.Branch)
                .Include(p => p.Counter)
                .FirstOrDefaultAsync(p => p.ItemCode == itemCode);
        }

        public async Task<IEnumerable<ProductDetails>> GetByCategoryAsync(int categoryId, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductDetails
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Include(p => p.Design)
                .Include(p => p.Purity)
                .Include(p => p.Branch)
                .Include(p => p.Counter)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductDetails>> GetByBranchAsync(int branchId, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductDetails
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Include(p => p.Design)
                .Include(p => p.Purity)
                .Include(p => p.Branch)
                .Include(p => p.Counter)
                .Where(p => p.BranchId == branchId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductDetails>> GetByCounterAsync(int counterId, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductDetails
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Include(p => p.Design)
                .Include(p => p.Purity)
                .Include(p => p.Branch)
                .Include(p => p.Counter)
                .Where(p => p.CounterId == counterId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductRfidAssignment>> GetAssignmentsAsync(string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductRfidAssignments
                .Include(pr => pr.Product)
                .Include(pr => pr.Rfid)
                .ToListAsync();
        }

        public async Task<ProductRfidAssignment?> GetAssignmentByProductAsync(int productId, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductRfidAssignments
                .Include(pr => pr.Product)
                .Include(pr => pr.Rfid)
                .FirstOrDefaultAsync(pr => pr.ProductId == productId && pr.IsActive);
        }

        public async Task<ProductRfidAssignment?> GetAssignmentByRfidAsync(string rfidCode, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductRfidAssignments
                .Include(pr => pr.Product)
                .Include(pr => pr.Rfid)
                .FirstOrDefaultAsync(pr => pr.RFIDCode == rfidCode && pr.IsActive);
        }

        public async Task<ProductRfidAssignment> AddAssignmentAsync(ProductRfidAssignment assignment, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            context.ProductRfidAssignments.Add(assignment);
            await context.SaveChangesAsync();
            return assignment;
        }

        public async Task<ProductRfidAssignment> UpdateAssignmentAsync(ProductRfidAssignment assignment, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            context.ProductRfidAssignments.Update(assignment);
            await context.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> DeleteAssignmentAsync(int id, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            var assignment = await context.ProductRfidAssignments.FindAsync(id);
            if (assignment == null) return false;

            context.ProductRfidAssignments.Remove(assignment);
            await context.SaveChangesAsync();
            return true;
        }
    }
} 