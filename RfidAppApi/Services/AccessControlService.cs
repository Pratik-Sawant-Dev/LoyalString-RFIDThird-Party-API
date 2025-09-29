using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.Models;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service for managing user access control based on branch and counter assignments
    /// </summary>
    public class AccessControlService : IAccessControlService
    {
        private readonly AppDbContext _context;
        private readonly ClientDbContextFactory _clientDbContextFactory;

        public AccessControlService(AppDbContext context, ClientDbContextFactory clientDbContextFactory)
        {
            _context = context;
            _clientDbContextFactory = clientDbContextFactory;
        }

        public async Task<UserAccessInfo?> GetUserAccessInfoAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.AdminUser)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return null;

            var accessInfo = new UserAccessInfo
            {
                UserId = user.UserId,
                UserName = user.UserName,
                IsAdmin = user.IsAdmin,
                BranchId = user.BranchId,
                CounterId = user.CounterId,
                ClientCode = user.ClientCode
            };

            // Get branch and counter names if assigned
            if (user.BranchId.HasValue || user.CounterId.HasValue)
            {
                try
                {
                    using var clientContext = await _clientDbContextFactory.CreateAsync(user.ClientCode);

                    if (user.BranchId.HasValue)
                    {
                        var branch = await clientContext.BranchMasters
                            .FirstOrDefaultAsync(b => b.BranchId == user.BranchId.Value);
                        accessInfo.BranchName = branch?.BranchName;
                    }

                    if (user.CounterId.HasValue)
                    {
                        var counter = await clientContext.CounterMasters
                            .Include(c => c.Branch)
                            .FirstOrDefaultAsync(c => c.CounterId == user.CounterId.Value);
                        accessInfo.CounterName = counter?.CounterName;
                    }
                }
                catch
                {
                    // If there's an error accessing the client database, continue without names
                }
            }

            return accessInfo;
        }

        public async Task<bool> CanAccessBranchAsync(int userId, int branchId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Admin users can access all branches
            if (user.IsAdmin)
                return true;

            // Regular users can only access their assigned branch
            return user.BranchId == branchId;
        }

        public async Task<bool> CanAccessCounterAsync(int userId, int counterId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Admin users can access all counters
            if (user.IsAdmin)
                return true;

            // Regular users can only access their assigned counter
            return user.CounterId == counterId;
        }

        public async Task<bool> CanAccessBranchAndCounterAsync(int userId, int branchId, int counterId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Admin users can access all branches and counters
            if (user.IsAdmin)
                return true;

            // Regular users can only access their assigned branch and counter
            return user.BranchId == branchId && user.CounterId == counterId;
        }

        public async Task<bool> IsAdminUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.IsAdmin ?? false;
        }

        public async Task<List<int>> GetAccessibleBranchIdsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new List<int>();

            // Admin users can access all branches
            if (user.IsAdmin)
            {
                try
                {
                    using var clientContext = await _clientDbContextFactory.CreateAsync(user.ClientCode);
                    return await clientContext.BranchMasters
                        .Where(b => b.ClientCode == user.ClientCode)
                        .Select(b => b.BranchId)
                        .ToListAsync();
                }
                catch
                {
                    return new List<int>();
                }
            }

            // Regular users can only access their assigned branch
            return user.BranchId.HasValue ? new List<int> { user.BranchId.Value } : new List<int>();
        }

        public async Task<List<int>> GetAccessibleCounterIdsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new List<int>();

            // Admin users can access all counters
            if (user.IsAdmin)
            {
                try
                {
                    using var clientContext = await _clientDbContextFactory.CreateAsync(user.ClientCode);
                    return await clientContext.CounterMasters
                        .Where(c => c.ClientCode == user.ClientCode)
                        .Select(c => c.CounterId)
                        .ToListAsync();
                }
                catch
                {
                    return new List<int>();
                }
            }

            // Regular users can only access their assigned counter
            return user.CounterId.HasValue ? new List<int> { user.CounterId.Value } : new List<int>();
        }
    }
}
