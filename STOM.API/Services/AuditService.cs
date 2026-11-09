using STOM.API.Data;
using STOM.API.Models;

namespace STOM.API.Services;

public class AuditService
{
    private readonly StomDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(StomDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, int? entityId, string performedBy, string details = "")
    {
        var auditLog = new AuditLog
        {
            Action = action,
            EntityId = entityId,
            PerformedBy = performedBy,
            Details = details,
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
