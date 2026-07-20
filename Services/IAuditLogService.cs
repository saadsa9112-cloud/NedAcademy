using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Services;

public interface IAuditLogService
{
    Task LogAsync(string action, string entityName, int? entityId, string details, string performedBy = "admin");
    Task<List<AuditLog>> GetLogsAsync(int count = 50);
}
