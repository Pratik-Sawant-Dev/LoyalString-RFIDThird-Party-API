using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service for managing user permissions and access control
    /// </summary>
    public class UserPermissionService : IUserPermissionService
    {
        private readonly AppDbContext _context;
        private readonly IAccessControlService _accessControlService;
        private readonly ClientDbContextFactory _clientDbContextFactory;

        public UserPermissionService(
            AppDbContext context, 
            IAccessControlService accessControlService,
            ClientDbContextFactory clientDbContextFactory)
        {
            _context = context;
            _accessControlService = accessControlService;
            _clientDbContextFactory = clientDbContextFactory;
        }

        public async Task<IEnumerable<UserPermissionDto>> GetUserPermissionsAsync(int userId)
        {
            var permissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();

            return permissions.Select(p => new UserPermissionDto
            {
                UserPermissionId = p.UserPermissionId,
                UserId = p.UserId,
                ClientCode = p.ClientCode,
                Module = p.Module,
                CanView = p.CanView,
                CanCreate = p.CanCreate,
                CanEdit = p.CanUpdate,
                CanDelete = p.CanDelete,
                CanExport = p.CanExport,
                CanImport = p.CanImport,
                CreatedOn = p.CreatedOn,
                CreatedBy = p.CreatedBy
            });
        }

        public async Task<UserPermissionSummaryDto> GetUserPermissionSummaryAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new InvalidOperationException("User not found.");

            var permissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();

            var moduleSummaries = permissions.Select(p => new ModulePermissionSummary
            {
                Module = p.Module,
                CanView = p.CanView,
                CanCreate = p.CanCreate,
                CanEdit = p.CanUpdate,
                CanDelete = p.CanDelete,
                CanExport = p.CanExport,
                CanImport = p.CanImport,
                PermissionCount = new[] { p.CanView, p.CanCreate, p.CanUpdate, p.CanDelete, p.CanExport, p.CanImport }.Count(x => x)
            }).ToList();

            var activePermissions = permissions.Sum(p => new[] { p.CanView, p.CanCreate, p.CanUpdate, p.CanDelete, p.CanExport, p.CanImport }.Count(x => x));

            return new UserPermissionSummaryDto
            {
                UserId = user.UserId,
                UserName = user.FullName ?? user.UserName,
                UserEmail = user.Email,
                TotalPermissions = permissions.Count * 6, // 6 permission types per module
                ActivePermissions = activePermissions,
                ModuleSummaries = moduleSummaries
            };
        }

        public async Task<UserAccessInfoDto> GetUserAccessInfoAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.AdminUser)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) throw new InvalidOperationException("User not found.");

            var accessInfo = new UserAccessInfoDto
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

            // Get accessible branch and counter IDs
            accessInfo.AccessibleBranchIds = await _accessControlService.GetAccessibleBranchIdsAsync(userId);
            accessInfo.AccessibleCounterIds = await _accessControlService.GetAccessibleCounterIdsAsync(userId);

            return accessInfo;
        }

        public async Task<bool> HasPermissionAsync(int userId, string module, string action)
        {
            var permission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.Module == module);

            if (permission == null) return false;

            return action.ToLower() switch
            {
                "view" => permission.CanView,
                "create" => permission.CanCreate,
                "edit" => permission.CanUpdate,
                "update" => permission.CanUpdate,
                "delete" => permission.CanDelete,
                "export" => permission.CanExport,
                "import" => permission.CanImport,
                _ => false
            };
        }

        public async Task<IEnumerable<string>> GetAvailableModulesAsync()
        {
            return await Task.FromResult(new[]
            {
                "Product",
                "RFID", 
                "Invoice",
                "Reports",
                "StockTransfer",
                "StockVerification",
                "ProductImage",
                "User",
                "Admin"
            });
        }

        public async Task<List<int>> GetAccessibleBranchIdsAsync(int userId)
        {
            return await _accessControlService.GetAccessibleBranchIdsAsync(userId);
        }

        public async Task<List<int>> GetAccessibleCounterIdsAsync(int userId)
        {
            return await _accessControlService.GetAccessibleCounterIdsAsync(userId);
        }

        public async Task<UserPermissionDetailsDto> GetUserPermissionDetailsAsync(int userId)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            var permissionSummary = await GetUserPermissionSummaryAsync(userId);
            var accessInfo = await GetUserAccessInfoAsync(userId);
            var availableModules = await GetAvailableModulesAsync();

            return new UserPermissionDetailsDto
            {
                Permissions = permissions.ToList(),
                PermissionSummary = permissionSummary,
                AccessInfo = accessInfo,
                AvailableModules = availableModules.ToList()
            };
        }
    }
}
