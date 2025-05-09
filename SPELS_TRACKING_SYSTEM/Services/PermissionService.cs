using Microsoft.EntityFrameworkCore;
using SPELS_TRACKING_SYSTEM.Data;

namespace SPELS_TRACKING_SYSTEM.Services
{
    public class PermissionService
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionService(SPELS_TRACKING_SYSTEMContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool HasAccessToStage(string stageName)
        {
            // Retrieve the logged-in user's username from session
            string username = _httpContextAccessor.HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username)) return false; // No user is logged in

            // Get the user's role
            var userRoleId = _context.User
                .Where(u => u.Username == username)
                .Select(u => u.RoleID)
                .FirstOrDefault();

            // Check if the role has access to the requested stage
            return _context.RolePermission
                .Any(rp => rp.RoleID == userRoleId && rp.StageName == stageName && rp.CanAccess);
        }
    }
}
