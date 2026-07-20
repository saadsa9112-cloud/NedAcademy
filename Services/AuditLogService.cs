using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Data;
using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string action, string entityName, int? entityId, string details, string performedBy = "admin")
    {
        var log = new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            Timestamp = DateTime.Now,
            PerformedBy = performedBy
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetLogsAsync(int count = 50)
    {
        return await _context.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }
}
