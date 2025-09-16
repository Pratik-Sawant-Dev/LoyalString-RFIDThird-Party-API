using System.Security.Claims;
using RfidAppApi.Services;

namespace RfidAppApi.Middleware
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;

        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAdminService adminService)
        {
            // Skip permission check for certain paths
            var path = context.Request.Path.Value?.ToLower();
            if (ShouldSkipPermissionCheck(path))
            {
                await _next(context);
                return;
            }

            // Only check permissions for authenticated users
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Get user info from token
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            var userType = context.User.FindFirst("UserType")?.Value;
            var isAdmin = context.User.FindFirst("IsAdmin")?.Value == "True";

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                await _next(context);
                return;
            }

            // Main admins have full access
            if (userType == "MainAdmin")
            {
                await _next(context);
                return;
            }

            // Admin users have broad access but will be checked by business logic
            if (isAdmin)
            {
                await _next(context);
                return;
            }

            // For regular users, check permissions based on endpoint
            var (module, action) = GetModuleAndActionFromPath(path, context.Request.Method);
            
            if (!string.IsNullOrEmpty(module) && !string.IsNullOrEmpty(action))
            {
                var hasPermission = await adminService.HasPermissionAsync(userId, module, action);
                if (!hasPermission)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Access denied. Insufficient permissions.");
                    return;
                }
            }

            await _next(context);
        }

        private bool ShouldSkipPermissionCheck(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return true;

            var skipPaths = new[]
            {
                "/api/user/register",
                "/api/user/login",
                "/health",
                "/swagger",
                "/api/error",
                "/api/admin" // Admin endpoints have their own authorization logic
            };

            return skipPaths.Any(skip => path.StartsWith(skip));
        }

        private (string module, string action) GetModuleAndActionFromPath(string? path, string method)
        {
            if (string.IsNullOrEmpty(path))
                return ("", "");

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 3) // api/controller/action
                return ("", "");

            var controller = segments[1].ToLower();
            var action = GetActionFromMethod(method);

            var module = controller switch
            {
                "product" => "Product",
                "rfid" => "RFID",
                "invoice" => "Invoice",
                "reporting" => "Reports",
                "stocktransfer" => "StockTransfer",
                "stockverification" => "StockVerification",
                "productimage" => "ProductImage",
                _ => ""
            };

            return (module, action);
        }

        private string GetActionFromMethod(string method)
        {
            return method.ToUpper() switch
            {
                "GET" => "View",
                "POST" => "Create",
                "PUT" => "Update",
                "PATCH" => "Update",
                "DELETE" => "Delete",
                _ => "View"
            };
        }
    }
}
