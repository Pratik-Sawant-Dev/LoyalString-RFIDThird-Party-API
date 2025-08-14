using RfidAppApi.Models;

namespace RfidAppApi.Repositories
{
    public interface IRfidRepository
    {
        Task<IEnumerable<Rfid>> GetAllAsync(string clientCode);
        Task<Rfid?> GetByIdAsync(string rfidCode, string clientCode);
        Task<Rfid> AddAsync(Rfid rfid, string clientCode);
        Task<Rfid> UpdateAsync(Rfid rfid, string clientCode);
        Task<bool> DeleteAsync(string rfidCode, string clientCode);
        Task<IEnumerable<Rfid>> GetByClientCodeAsync(string clientCode);
        Task<IEnumerable<ProductRfidAssignment>> GetAssignmentsAsync(string clientCode);
        Task<IEnumerable<ProductDetails>> GetProductsAsync(string clientCode);
    }
} 