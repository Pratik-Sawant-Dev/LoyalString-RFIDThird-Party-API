using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RfidAppApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly IClientDatabaseService _clientDatabaseService;

        public AdminService(AppDbContext context, IClientDatabaseService clientDatabaseService)
        {
            _context = context;
            _clientDatabaseService = clientDatabaseService;
        }

        public async Task<AdminUserDto> CreateUserAsync(CreateUserByAdminDto createUserDto, int adminUserId)
        {
            try
            {
                // Get admin user to validate and get client code
                var adminUser = await _context.Users.FindAsync(adminUserId);
                if (adminUser == null || (!adminUser.IsAdmin && adminUser.UserType != "MainAdmin"))
                {
                    throw new UnauthorizedAccessException("Only admins can create users.");
                }

                // Validate that admin user has required organization information
                if (string.IsNullOrEmpty(adminUser.OrganisationName))
                {
                    throw new InvalidOperationException("Admin user must have an organization name.");
                }

                if (string.IsNullOrEmpty(adminUser.ClientCode))
                {
                    throw new InvalidOperationException("Admin user must have a client code.");
                }

                // Validate that admin user has required database information
                if (string.IsNullOrEmpty(adminUser.DatabaseName))
                {
                    throw new InvalidOperationException("Admin user must have a database name.");
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
                {
                    throw new InvalidOperationException("Email already registered.");
                }

                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.UserName == createUserDto.UserName))
                {
                    throw new InvalidOperationException("Username already taken.");
                }

                // Hash password
                var passwordHash = HashPassword(createUserDto.Password);

                // Create user
                var user = new User
                {
                    UserName = createUserDto.UserName,
                    Email = createUserDto.Email,
                    PasswordHash = passwordHash,
                    FullName = createUserDto.FullName ?? string.Empty,
                    MobileNumber = createUserDto.MobileNumber ?? string.Empty,
                    City = createUserDto.City ?? string.Empty,
                    Address = createUserDto.Address ?? string.Empty,
                    OrganisationName = adminUser.OrganisationName, // Same organization as admin
                    ShowroomType = createUserDto.ShowroomType ?? string.Empty,
                    ClientCode = adminUser.ClientCode, // Same client code as admin
                    DatabaseName = adminUser.DatabaseName, // Same database as admin
                    IsAdmin = createUserDto.IsAdmin,
                    AdminUserId = adminUserId,
                    UserType = createUserDto.IsAdmin ? "Admin" : "User",
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                // Use a transaction to ensure all operations succeed or fail together
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Create permissions
                    await CreateUserPermissionsAsync(user.UserId, createUserDto.Permissions, adminUserId);

                    // Log activity
                    await LogActivityAsync(adminUserId, adminUser.ClientCode, "User", "Create", 
                        $"Created user: {user.Email}", "tblUser", user.UserId);

                    // Save all changes
                    await _context.SaveChangesAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();
                }
                catch
                {
                    // Rollback the transaction on any error
                    await transaction.RollbackAsync();
                    throw;
                }

                return await GetUserByIdAsync(user.UserId, adminUserId) ?? throw new InvalidOperationException("Failed to retrieve created user.");
            }
            catch (Exception ex)
            {
                // Log the detailed error for debugging
                Console.WriteLine($"Error in CreateUserAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw; // Re-throw the exception to be handled by the controller
            }
        }

        public async Task<AdminUserDto> UpdateUserAsync(int userId, UpdateUserByAdminDto updateUserDto, int adminUserId)
        {
            // Validate admin access
            if (!await CanUserAccessUserAsync(adminUserId, userId))
            {
                throw new UnauthorizedAccessException("Access denied.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var oldValues = new
            {
                user.FullName,
                user.MobileNumber,
                user.City,
                user.Address,
                user.ShowroomType,
                user.IsActive,
                user.IsAdmin
            };

            // Update properties
            if (!string.IsNullOrEmpty(updateUserDto.FullName))
                user.FullName = updateUserDto.FullName;
            
            if (!string.IsNullOrEmpty(updateUserDto.MobileNumber))
                user.MobileNumber = updateUserDto.MobileNumber;
            
            if (!string.IsNullOrEmpty(updateUserDto.City))
                user.City = updateUserDto.City;
            
            if (!string.IsNullOrEmpty(updateUserDto.Address))
                user.Address = updateUserDto.Address;
            
            if (!string.IsNullOrEmpty(updateUserDto.ShowroomType))
                user.ShowroomType = updateUserDto.ShowroomType;
            
            if (updateUserDto.IsActive.HasValue)
                user.IsActive = updateUserDto.IsActive.Value;

            if (updateUserDto.IsAdmin.HasValue)
            {
                user.IsAdmin = updateUserDto.IsAdmin.Value;
                user.UserType = updateUserDto.IsAdmin.Value ? "Admin" : "User";
            }

            await _context.SaveChangesAsync();

            // Update permissions if provided
            if (updateUserDto.Permissions != null && updateUserDto.Permissions.Any())
            {
                await UpdateUserPermissionsAsync(userId, updateUserDto.Permissions, adminUserId);
            }

            var newValues = new
            {
                user.FullName,
                user.MobileNumber,
                user.City,
                user.Address,
                user.ShowroomType,
                user.IsActive,
                user.IsAdmin
            };

            // Log activity
            await LogActivityAsync(adminUserId, user.ClientCode, "User", "Update", 
                $"Updated user: {user.Email}", "tblUser", user.UserId, oldValues, newValues);

            return await GetUserByIdAsync(userId, adminUserId) ?? throw new InvalidOperationException("Failed to retrieve updated user.");
        }

        public async Task<bool> DeleteUserAsync(int userId, int adminUserId)
        {
            // Validate admin access
            if (!await CanUserAccessUserAsync(adminUserId, userId))
            {
                throw new UnauthorizedAccessException("Access denied.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Soft delete
            user.IsActive = false;
            await _context.SaveChangesAsync();

            // Log activity
            await LogActivityAsync(adminUserId, user.ClientCode, "User", "Delete", 
                $"Deleted user: {user.Email}", "tblUser", user.UserId);

            return true;
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(int userId, int adminUserId)
        {
            // Validate admin access
            if (!await CanUserAccessUserAsync(adminUserId, userId))
            {
                return null;
            }

            var user = await _context.Users
                .Include(u => u.AdminUser)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return null;

            var permissions = await GetUserPermissionsAsync(userId);

            return new AdminUserDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                MobileNumber = user.MobileNumber,
                City = user.City,
                Address = user.Address,
                OrganisationName = user.OrganisationName,
                ShowroomType = user.ShowroomType,
                ClientCode = user.ClientCode,
                DatabaseName = user.DatabaseName,
                IsAdmin = user.IsAdmin,
                AdminUserId = user.AdminUserId,
                UserType = user.UserType,
                IsActive = user.IsActive,
                CreatedOn = user.CreatedOn,
                LastLoginDate = user.LastLoginDate,
                Permissions = permissions.ToList()
            };
        }

        public async Task<IEnumerable<AdminUserDto>> GetUsersByAdminAsync(int adminUserId)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            if (adminUser == null) return new List<AdminUserDto>();

            var users = await _context.Users
                .Include(u => u.AdminUser)
                .Where(u => (u.AdminUserId == adminUserId || u.UserId == adminUserId) && u.IsActive)
                .ToListAsync();

            var result = new List<AdminUserDto>();
            foreach (var user in users)
            {
                var permissions = await GetUserPermissionsAsync(user.UserId);
                result.Add(new AdminUserDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    MobileNumber = user.MobileNumber,
                    City = user.City,
                    Address = user.Address,
                    OrganisationName = user.OrganisationName,
                    ShowroomType = user.ShowroomType,
                    ClientCode = user.ClientCode,
                    DatabaseName = user.DatabaseName,
                    IsAdmin = user.IsAdmin,
                    AdminUserId = user.AdminUserId,
                    UserType = user.UserType,
                    IsActive = user.IsActive,
                    CreatedOn = user.CreatedOn,
                    LastLoginDate = user.LastLoginDate,
                    Permissions = permissions.ToList()
                });
            }

            return result;
        }

        public async Task<IEnumerable<AdminUserDto>> GetAllUsersInOrganizationAsync(string clientCode)
        {
            var users = await _context.Users
                .Include(u => u.AdminUser)
                .Where(u => u.ClientCode == clientCode && u.IsActive)
                .ToListAsync();

            var result = new List<AdminUserDto>();
            foreach (var user in users)
            {
                var permissions = await GetUserPermissionsAsync(user.UserId);
                result.Add(new AdminUserDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    MobileNumber = user.MobileNumber,
                    City = user.City,
                    Address = user.Address,
                    OrganisationName = user.OrganisationName,
                    ShowroomType = user.ShowroomType,
                    ClientCode = user.ClientCode,
                    DatabaseName = user.DatabaseName,
                    IsAdmin = user.IsAdmin,
                    AdminUserId = user.AdminUserId,
                    UserType = user.UserType,
                    IsActive = user.IsActive,
                    CreatedOn = user.CreatedOn,
                    LastLoginDate = user.LastLoginDate,
                    Permissions = permissions.ToList()
                });
            }

            return result;
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

        public async Task<bool> UpdateUserPermissionsAsync(int userId, List<UserPermissionCreateDto> permissions, int adminUserId)
        {
            // Remove existing permissions
            var existingPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();

            _context.UserPermissions.RemoveRange(existingPermissions);

            // Add new permissions
            await CreateUserPermissionsAsync(userId, permissions, adminUserId);

            return true;
        }

        public async Task<bool> BulkUpdatePermissionsAsync(BulkPermissionUpdateDto bulkUpdate, int adminUserId)
        {
            foreach (var userId in bulkUpdate.UserIds)
            {
                if (await CanUserAccessUserAsync(adminUserId, userId))
                {
                    await UpdateUserPermissionsAsync(userId, bulkUpdate.Permissions, adminUserId);
                }
            }

            return true;
        }

        public async Task LogActivityAsync(int userId, string clientCode, string activityType, string action, 
            string? description = null, string? tableName = null, int? recordId = null, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            var activity = new UserActivity
            {
                UserId = userId,
                ClientCode = clientCode,
                ActivityType = activityType,
                Action = action,
                Description = description,
                TableName = tableName,
                RecordId = recordId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedOn = DateTime.UtcNow
            };

            _context.UserActivities.Add(activity);
            // Note: SaveChangesAsync is now handled by the calling method's transaction
        }

        public async Task<IEnumerable<UserActivityDto>> GetUserActivitiesAsync(ActivityFilterDto filter, string clientCode)
        {
            var query = _context.UserActivities
                .Include(ua => ua.User)
                .Where(ua => ua.ClientCode == clientCode);

            if (filter.UserId.HasValue)
                query = query.Where(ua => ua.UserId == filter.UserId.Value);

            if (!string.IsNullOrEmpty(filter.ActivityType))
                query = query.Where(ua => ua.ActivityType == filter.ActivityType);

            if (!string.IsNullOrEmpty(filter.Action))
                query = query.Where(ua => ua.Action == filter.Action);

            if (filter.StartDate.HasValue)
                query = query.Where(ua => ua.CreatedOn >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(ua => ua.CreatedOn <= filter.EndDate.Value);

            var activities = await query
                .OrderByDescending(ua => ua.CreatedOn)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return activities.Select(a => new UserActivityDto
            {
                ActivityId = a.ActivityId,
                UserId = a.UserId,
                ClientCode = a.ClientCode,
                ActivityType = a.ActivityType,
                Action = a.Action,
                Description = a.Description,
                TableName = a.TableName,
                RecordId = a.RecordId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CreatedOn = a.CreatedOn,
                UserName = a.User.UserName,
                UserEmail = a.User.Email
            });
        }

        public async Task<IEnumerable<UserActivityDto>> GetAllActivitiesAsync(ActivityFilterDto filter, int adminUserId)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            if (adminUser == null) return new List<UserActivityDto>();

            return await GetUserActivitiesAsync(filter, adminUser.ClientCode);
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync(int adminUserId)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            if (adminUser == null) return new AdminDashboardDto();

            return await GetOrganizationDashboardAsync(adminUser.ClientCode);
        }

        public async Task<AdminDashboardDto> GetOrganizationDashboardAsync(string clientCode)
        {
            var today = DateTime.Today;
            var totalUsers = await _context.Users.CountAsync(u => u.ClientCode == clientCode);
            var activeUsers = await _context.Users.CountAsync(u => u.ClientCode == clientCode && u.IsActive);
            var totalAdmins = await _context.Users.CountAsync(u => u.ClientCode == clientCode && u.IsAdmin && u.IsActive);
            var todayActivities = await _context.UserActivities.CountAsync(ua => ua.ClientCode == clientCode && ua.CreatedOn >= today);

            var recentActivities = await _context.UserActivities
                .Include(ua => ua.User)
                .Where(ua => ua.ClientCode == clientCode)
                .OrderByDescending(ua => ua.CreatedOn)
                .Take(10)
                .Select(a => new UserActivityDto
                {
                    ActivityId = a.ActivityId,
                    UserId = a.UserId,
                    ClientCode = a.ClientCode,
                    ActivityType = a.ActivityType,
                    Action = a.Action,
                    Description = a.Description,
                    CreatedOn = a.CreatedOn,
                    UserName = a.User.UserName,
                    UserEmail = a.User.Email
                })
                .ToListAsync();

            var recentUsers = await _context.Users
                .Where(u => u.ClientCode == clientCode && u.IsActive)
                .OrderByDescending(u => u.CreatedOn)
                .Take(5)
                .Select(u => new AdminUserDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    UserType = u.UserType,
                    CreatedOn = u.CreatedOn,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            // Get counts from client database (products, RFIDs, invoices)
            var (totalProducts, totalRFIDs, totalInvoices) = await GetClientDatabaseCounts(clientCode);

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalAdmins = totalAdmins,
                TotalProducts = totalProducts,
                TotalRFIDs = totalRFIDs,
                TotalInvoices = totalInvoices,
                TodayActivities = todayActivities,
                RecentActivities = recentActivities,
                RecentUsers = recentUsers
            };
        }

        public async Task<bool> ActivateUserAsync(int userId, int adminUserId)
        {
            if (!await CanUserAccessUserAsync(adminUserId, userId))
                return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = true;
            await _context.SaveChangesAsync();

            await LogActivityAsync(adminUserId, user.ClientCode, "User", "Activate", 
                $"Activated user: {user.Email}", "tblUser", user.UserId);

            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId, int adminUserId)
        {
            if (!await CanUserAccessUserAsync(adminUserId, userId))
                return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            await _context.SaveChangesAsync();

            await LogActivityAsync(adminUserId, user.ClientCode, "User", "Deactivate", 
                $"Deactivated user: {user.Email}", "tblUser", user.UserId);

            return true;
        }

        public async Task<bool> ResetUserPasswordAsync(int userId, string newPassword, int adminUserId)
        {
            if (!await CanUserAccessUserAsync(adminUserId, userId))
                return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            await LogActivityAsync(adminUserId, user.ClientCode, "User", "PasswordReset", 
                $"Reset password for user: {user.Email}", "tblUser", user.UserId);

            return true;
        }

        public async Task<bool> CanUserAccessUserAsync(int adminUserId, int targetUserId)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            var targetUser = await _context.Users.FindAsync(targetUserId);

            if (adminUser == null || targetUser == null) return false;

            // Main admin can access anyone in same organization
            if (adminUser.UserType == "MainAdmin" && adminUser.ClientCode == targetUser.ClientCode)
                return true;

            // Admin can access users they created or themselves
            if (adminUser.IsAdmin && (targetUser.AdminUserId == adminUserId || targetUser.UserId == adminUserId))
                return true;

            // User can only access themselves
            if (adminUserId == targetUserId)
                return true;

            return false;
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

        public async Task<IEnumerable<UserPermissionDto>> GetAllUserPermissionsAsync(string clientCode)
        {
            var permissions = await _context.UserPermissions
                .Include(up => up.User)
                .Where(up => up.User.ClientCode == clientCode)
                .Select(up => new UserPermissionDto
                {
                    UserPermissionId = up.UserPermissionId,
                    UserId = up.UserId,
                    ClientCode = up.ClientCode,
                    Module = up.Module,
                    CanView = up.CanView,
                    CanCreate = up.CanCreate,
                    CanEdit = up.CanUpdate,
                    CanDelete = up.CanDelete,
                    CanExport = up.CanExport,
                    CanImport = up.CanImport,
                    CreatedBy = up.CreatedBy,
                    CreatedOn = up.CreatedOn,
                    UserName = up.User.FullName,
                    UserEmail = up.User.Email
                })
                .ToListAsync();

            return permissions;
        }

        public async Task<ActivitySummaryDto> GetActivitySummaryAsync(int adminUserId)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            if (adminUser == null) throw new InvalidOperationException("Admin user not found.");

            var activities = await _context.UserActivities
                .Where(ua => ua.ClientCode == adminUser.ClientCode)
                .ToListAsync();

            var summary = new ActivitySummaryDto
            {
                TotalActivities = activities.Count,
                ActivitiesByModule = activities.GroupBy(ua => ua.ActivityType)
                    .Select(g => new ModuleActivitySummary
                    {
                        Module = g.Key,
                        Count = g.Count(),
                        LastActivity = g.Max(ua => ua.CreatedOn)
                    }).ToList(),
                ActivitiesByUser = activities.GroupBy(ua => ua.UserId)
                    .Select(g => new UserActivitySummary
                    {
                        UserId = g.Key,
                        Count = g.Count(),
                        LastActivity = g.Max(ua => ua.CreatedOn)
                    }).ToList(),
                RecentActivities = activities.OrderByDescending(ua => ua.CreatedOn)
                    .Take(10)
                    .Select(ua => new RecentActivity
                    {
                        UserId = ua.UserId,
                        ActivityType = ua.ActivityType,
                        Action = ua.Action,
                        Description = ua.Description,
                        CreatedOn = ua.CreatedOn
                    }).ToList()
            };

            return summary;
        }

        public async Task<ActivitySummaryDto> GetActivitySummaryByDateRangeAsync(int adminUserId, DateTime startDate, DateTime endDate)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            if (adminUser == null) throw new InvalidOperationException("Admin user not found.");

            var activities = await _context.UserActivities
                .Where(ua => ua.ClientCode == adminUser.ClientCode && 
                            ua.CreatedOn >= startDate && ua.CreatedOn <= endDate)
                .ToListAsync();

            var summary = new ActivitySummaryDto
            {
                TotalActivities = activities.Count,
                ActivitiesByModule = activities.GroupBy(ua => ua.ActivityType)
                    .Select(g => new ModuleActivitySummary
                    {
                        Module = g.Key,
                        Count = g.Count(),
                        LastActivity = g.Max(ua => ua.CreatedOn)
                    }).ToList(),
                ActivitiesByUser = activities.GroupBy(ua => ua.UserId)
                    .Select(g => new UserActivitySummary
                    {
                        UserId = g.Key,
                        Count = g.Count(),
                        LastActivity = g.Max(ua => ua.CreatedOn)
                    }).ToList(),
                RecentActivities = activities.OrderByDescending(ua => ua.CreatedOn)
                    .Take(10)
                    .Select(ua => new RecentActivity
                    {
                        UserId = ua.UserId,
                        ActivityType = ua.ActivityType,
                        Action = ua.Action,
                        Description = ua.Description,
                        CreatedOn = ua.CreatedOn
                    }).ToList()
            };

            return summary;
        }

        public async Task<ActivitySummaryDto> GetActivitySummaryByUserAsync(int adminUserId, int userId)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            if (adminUser == null) throw new InvalidOperationException("Admin user not found.");

            var activities = await _context.UserActivities
                .Where(ua => ua.ClientCode == adminUser.ClientCode && ua.UserId == userId)
                .ToListAsync();

            var summary = new ActivitySummaryDto
            {
                TotalActivities = activities.Count,
                ActivitiesByModule = activities.GroupBy(ua => ua.ActivityType)
                    .Select(g => new ModuleActivitySummary
                    {
                        Module = g.Key,
                        Count = g.Count(),
                        LastActivity = g.Max(ua => ua.CreatedOn)
                    }).ToList(),
                RecentActivities = activities.OrderByDescending(ua => ua.CreatedOn)
                    .Take(10)
                    .Select(ua => new RecentActivity
                    {
                        UserId = ua.UserId,
                        ActivityType = ua.ActivityType,
                        Action = ua.Action,
                        Description = ua.Description,
                        CreatedOn = ua.CreatedOn
                    }).ToList()
            };

            return summary;
        }

        public async Task<ActivitySummaryDto> GetActivitySummaryByModuleAsync(int adminUserId, string module)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            if (adminUser == null) throw new InvalidOperationException("Admin user not found.");

            var activities = await _context.UserActivities
                .Where(ua => ua.ClientCode == adminUser.ClientCode && ua.ActivityType == module)
                .ToListAsync();

            var summary = new ActivitySummaryDto
            {
                TotalActivities = activities.Count,
                ActivitiesByUser = activities.GroupBy(ua => ua.UserId)
                    .Select(g => new UserActivitySummary
                    {
                        UserId = g.Key,
                        Count = g.Count(),
                        LastActivity = g.Max(ua => ua.CreatedOn)
                    }).ToList(),
                RecentActivities = activities.OrderByDescending(ua => ua.CreatedOn)
                    .Take(10)
                    .Select(ua => new RecentActivity
                    {
                        UserId = ua.UserId,
                        ActivityType = ua.ActivityType,
                        Action = ua.Action,
                        Description = ua.Description,
                        CreatedOn = ua.CreatedOn
                    }).ToList()
            };

            return summary;
        }

        public async Task<UserHierarchyDto> GetUserHierarchyAsync(int adminUserId)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            if (adminUser == null) throw new InvalidOperationException("Admin user not found.");

            var users = await _context.Users
                .Where(u => u.ClientCode == adminUser.ClientCode)
                .ToListAsync();

            var hierarchy = new UserHierarchyDto
            {
                AdminUserId = adminUserId,
                AdminUserName = adminUser.FullName,
                Users = users.Where(u => u.AdminUserId == adminUserId)
                    .Select(u => new UserHierarchyItem
                    {
                        UserId = u.UserId,
                        UserName = u.FullName,
                        UserType = u.UserType,
                        IsActive = u.IsActive,
                        CreatedOn = u.CreatedOn
                    }).ToList()
            };

            return hierarchy;
        }

        public async Task<UserHierarchyDto> GetUserHierarchyByAdminAsync(int adminUserId)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            if (adminUser == null) throw new InvalidOperationException("Admin user not found.");

            var users = await _context.Users
                .Where(u => u.ClientCode == adminUser.ClientCode)
                .ToListAsync();

            var hierarchy = new UserHierarchyDto
            {
                AdminUserId = adminUserId,
                AdminUserName = adminUser.FullName,
                Users = users.Where(u => u.AdminUserId == adminUserId)
                    .Select(u => new UserHierarchyItem
                    {
                        UserId = u.UserId,
                        UserName = u.FullName,
                        UserType = u.UserType,
                        IsActive = u.IsActive,
                        CreatedOn = u.CreatedOn
                    }).ToList()
            };

            return hierarchy;
        }

        public async Task<string> ExportActivitiesToCsvAsync(ActivityFilterDto filter, int adminUserId)
        {
            var adminUser = await _context.Users.FindAsync(adminUserId);
            if (adminUser == null) throw new InvalidOperationException("Admin user not found.");

            var query = _context.UserActivities
                .Include(ua => ua.User)
                .Where(ua => ua.ClientCode == adminUser.ClientCode);

            if (filter.StartDate.HasValue)
                query = query.Where(ua => ua.CreatedOn >= filter.StartDate.Value);
            if (filter.EndDate.HasValue)
                query = query.Where(ua => ua.CreatedOn <= filter.EndDate.Value);
            if (filter.UserId.HasValue)
                query = query.Where(ua => ua.UserId == filter.UserId.Value);
            if (!string.IsNullOrEmpty(filter.ActivityType))
                query = query.Where(ua => ua.ActivityType == filter.ActivityType);

            var activities = await query
                .OrderByDescending(ua => ua.CreatedOn)
                .ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("User,Activity Type,Action,Description,IP Address,User Agent,Created On");

            foreach (var activity in activities)
            {
                csvBuilder.AppendLine($"\"{activity.User?.FullName ?? "Unknown"}\",\"{activity.ActivityType}\",\"{activity.Action}\",\"{activity.Description}\",\"{activity.IpAddress}\",\"{activity.UserAgent}\",\"{activity.CreatedOn:yyyy-MM-dd HH:mm:ss}\"");
            }

            return csvBuilder.ToString();
        }

        public async Task<string> ExportPermissionsToCsvAsync(string clientCode)
        {
            var permissions = await _context.UserPermissions
                .Include(up => up.User)
                .Where(up => up.User.ClientCode == clientCode)
                .ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("User,Module,Can View,Can Create,Can Edit,Can Delete,Can Export,Can Import,Created On");

            foreach (var permission in permissions)
            {
                csvBuilder.AppendLine($"\"{permission.User?.FullName ?? "Unknown"}\",\"{permission.Module}\",\"{permission.CanView}\",\"{permission.CanCreate}\",\"{permission.CanUpdate}\",\"{permission.CanDelete}\",\"{permission.CanExport}\",\"{permission.CanImport}\",\"{permission.CreatedOn:yyyy-MM-dd HH:mm:ss}\"");
            }

            return csvBuilder.ToString();
        }

        private async Task CreateUserPermissionsAsync(int userId, List<UserPermissionCreateDto> permissions, int createdBy)
        {
            // Get the user to get the client code
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // If no permissions provided, create default permissions
            if (permissions == null || !permissions.Any())
            {
                // Create default permissions for common modules
                var defaultPermissions = new List<UserPermissionCreateDto>
                {
                    new UserPermissionCreateDto { Module = "Product", CanView = true, CanCreate = false, CanEdit = false, CanDelete = false, CanExport = false, CanImport = false },
                    new UserPermissionCreateDto { Module = "RFID", CanView = true, CanCreate = false, CanEdit = false, CanDelete = false, CanExport = false, CanImport = false },
                    new UserPermissionCreateDto { Module = "Invoice", CanView = true, CanCreate = false, CanEdit = false, CanDelete = false, CanExport = false, CanImport = false }
                };
                permissions = defaultPermissions;
            }

            var userPermissions = permissions.Select(p => new UserPermission
            {
                UserId = userId,
                ClientCode = user.ClientCode,
                Module = p.Module,
                CanView = p.CanView,
                CanCreate = p.CanCreate,
                CanUpdate = p.CanEdit,
                CanDelete = p.CanDelete,
                CanExport = p.CanExport,
                CanImport = p.CanImport,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy
            }).ToList();

            _context.UserPermissions.AddRange(userPermissions);
            // Note: SaveChangesAsync is now handled by the calling method's transaction
        }

        private async Task<(int products, int rfids, int invoices)> GetClientDatabaseCounts(string clientCode)
        {
            try
            {
                // This would require access to client database
                // For now, return 0 - this can be implemented later with client database service
                await Task.CompletedTask; // Placeholder
                return (0, 0, 0);
            }
            catch
            {
                return (0, 0, 0);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
