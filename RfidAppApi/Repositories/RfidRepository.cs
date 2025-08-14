using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.Models;

namespace RfidAppApi.Repositories
{
    public class RfidRepository : IRfidRepository
    {
        private readonly ClientDbContextFactory _contextFactory;

        public RfidRepository(ClientDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        private async Task<ClientDbContext> GetContextAsync(string clientCode)
        {
            return await _contextFactory.CreateAsync(clientCode);
        }

        public async Task<IEnumerable<Rfid>> GetAllAsync(string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.Rfids.ToListAsync();
        }

        public async Task<Rfid?> GetByIdAsync(string rfidCode, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.Rfids.FirstOrDefaultAsync(r => r.RFIDCode == rfidCode);
        }

        public async Task<Rfid> AddAsync(Rfid rfid, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            context.Rfids.Add(rfid);
            await context.SaveChangesAsync();
            return rfid;
        }

        public async Task<Rfid> UpdateAsync(Rfid rfid, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            context.Rfids.Update(rfid);
            await context.SaveChangesAsync();
            return rfid;
        }

        public async Task<bool> DeleteAsync(string rfidCode, string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            var rfid = await context.Rfids.FirstOrDefaultAsync(r => r.RFIDCode == rfidCode);
            if (rfid == null) return false;

            context.Rfids.Remove(rfid);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Rfid>> GetByClientCodeAsync(string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.Rfids.ToListAsync();
        }

        public async Task<IEnumerable<ProductRfidAssignment>> GetAssignmentsAsync(string clientCode)
        {
            using var context = await GetContextAsync(clientCode);
            return await context.ProductRfidAssignments
                .Include(pr => pr.Product)
                .Include(pr => pr.Rfid)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductDetails>> GetProductsAsync(string clientCode)
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
    }
} 