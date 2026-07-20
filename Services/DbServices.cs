using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Data;
using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Services;

public class DbSettingService : ISettingService
{
    private readonly ApplicationDbContext _context;

    public DbSettingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSetting> GetSettingsAsync()
    {
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new SystemSetting();
            _context.SystemSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task UpdateSettingsAsync(SystemSetting setting)
    {
        _context.SystemSettings.Update(setting);
        await _context.SaveChangesAsync();
    }
}

public class DbProgramService : IProgramService
{
    private readonly ApplicationDbContext _context;

    public DbProgramService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DegreeProgram>> GetProgramsAsync()
    {
        return await _context.DegreePrograms.Include(p => p.Department).ToListAsync();
    }

    public async Task<DegreeProgram?> GetProgramByIdAsync(int id)
    {
        return await _context.DegreePrograms.Include(p => p.Department).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Department>> GetDepartmentsAsync()
    {
        return await _context.Departments.ToListAsync();
    }

    public async Task SaveProgramAsync(DegreeProgram program)
    {
        if (program.Id == 0)
        {
            _context.DegreePrograms.Add(program);
        }
        else
        {
            _context.DegreePrograms.Update(program);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProgramAsync(int id)
    {
        var program = await _context.DegreePrograms.FindAsync(id);
        if (program != null)
        {
            _context.DegreePrograms.Remove(program);
            await _context.SaveChangesAsync();
        }
    }
}

public class DbFacultyService : IFacultyService
{
    private readonly ApplicationDbContext _context;

    public DbFacultyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Faculty>> GetFacultyAsync()
    {
        return await _context.FacultyMembers.Include(f => f.Department).ToListAsync();
    }

    public async Task<Faculty?> GetFacultyByIdAsync(int id)
    {
        return await _context.FacultyMembers.Include(f => f.Department).FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task SaveFacultyAsync(Faculty faculty)
    {
        if (faculty.Id == 0)
        {
            _context.FacultyMembers.Add(faculty);
        }
        else
        {
            _context.FacultyMembers.Update(faculty);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFacultyAsync(int id)
    {
        var faculty = await _context.FacultyMembers.FindAsync(id);
        if (faculty != null)
        {
            _context.FacultyMembers.Remove(faculty);
            await _context.SaveChangesAsync();
        }
    }
}

public class DbLeadershipService : ILeadershipService
{
    private readonly ApplicationDbContext _context;

    public DbLeadershipService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Leadership>> GetLeadershipAsync()
    {
        return await _context.LeadershipRegistry.OrderBy(l => l.Rank).ToListAsync();
    }

    public async Task<Leadership?> GetLeadershipByIdAsync(int id)
    {
        return await _context.LeadershipRegistry.FindAsync(id);
    }

    public async Task SaveLeadershipAsync(Leadership leader)
    {
        if (leader.Id == 0)
        {
            _context.LeadershipRegistry.Add(leader);
        }
        else
        {
            _context.LeadershipRegistry.Update(leader);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLeadershipAsync(int id)
    {
        var leader = await _context.LeadershipRegistry.FindAsync(id);
        if (leader != null)
        {
            _context.LeadershipRegistry.Remove(leader);
            await _context.SaveChangesAsync();
        }
    }
}

public class DbResearchService : IResearchService
{
    private readonly ApplicationDbContext _context;

    public DbResearchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ResearchItem>> GetResearchItemsAsync()
    {
        return await _context.ResearchItems.ToListAsync();
    }

    public async Task<ResearchItem?> GetResearchItemByIdAsync(int id)
    {
        return await _context.ResearchItems.FindAsync(id);
    }

    public async Task SaveResearchItemAsync(ResearchItem item)
    {
        if (item.Id == 0)
        {
            _context.ResearchItems.Add(item);
        }
        else
        {
            _context.ResearchItems.Update(item);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteResearchItemAsync(int id)
    {
        var item = await _context.ResearchItems.FindAsync(id);
        if (item != null)
        {
            _context.ResearchItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}

public class DbCampusService : ICampusService
{
    private readonly ApplicationDbContext _context;

    public DbCampusService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CampusFacility>> GetFacilitiesAsync()
    {
        return await _context.CampusFacilities.ToListAsync();
    }

    public async Task<CampusFacility?> GetFacilityByIdAsync(int id)
    {
        return await _context.CampusFacilities.FindAsync(id);
    }

    public async Task SaveFacilityAsync(CampusFacility facility)
    {
        if (facility.Id == 0)
        {
            _context.CampusFacilities.Add(facility);
        }
        else
        {
            _context.CampusFacilities.Update(facility);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFacilityAsync(int id)
    {
        var facility = await _context.CampusFacilities.FindAsync(id);
        if (facility != null)
        {
            _context.CampusFacilities.Remove(facility);
            await _context.SaveChangesAsync();
        }
    }
}

public class DbNewsService : INewsService
{
    private readonly ApplicationDbContext _context;

    public DbNewsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NewsItem>> GetNewsAsync()
    {
        return await _context.NewsItems.OrderByDescending(n => n.PublishDate).ToListAsync();
    }

    public async Task<NewsItem?> GetNewsByIdAsync(int id)
    {
        return await _context.NewsItems.FindAsync(id);
    }

    public async Task SaveNewsAsync(NewsItem item)
    {
        if (item.Id == 0)
        {
            _context.NewsItems.Add(item);
        }
        else
        {
            _context.NewsItems.Update(item);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteNewsAsync(int id)
    {
        var item = await _context.NewsItems.FindAsync(id);
        if (item != null)
        {
            _context.NewsItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<EventItem>> GetEventsAsync()
    {
        return await _context.EventItems.OrderBy(e => e.Date).ToListAsync();
    }

    public async Task<EventItem?> GetEventByIdAsync(int id)
    {
        return await _context.EventItems.FindAsync(id);
    }

    public async Task SaveEventAsync(EventItem item)
    {
        if (item.Id == 0)
        {
            _context.EventItems.Add(item);
        }
        else
        {
            _context.EventItems.Update(item);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEventAsync(int id)
    {
        var item = await _context.EventItems.FindAsync(id);
        if (item != null)
        {
            _context.EventItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}

public class DbDownloadService : IDownloadService
{
    private readonly ApplicationDbContext _context;

    public DbDownloadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DownloadFile>> GetFilesAsync()
    {
        return await _context.DownloadFiles.ToListAsync();
    }

    public async Task<DownloadFile?> GetFileByIdAsync(int id)
    {
        return await _context.DownloadFiles.FindAsync(id);
    }

    public async Task SaveFileAsync(DownloadFile file)
    {
        if (file.Id == 0)
        {
            _context.DownloadFiles.Add(file);
        }
        else
        {
            _context.DownloadFiles.Update(file);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFileAsync(int id)
    {
        var file = await _context.DownloadFiles.FindAsync(id);
        if (file != null)
        {
            _context.DownloadFiles.Remove(file);
            await _context.SaveChangesAsync();
        }
    }
}

public class DbFAQService : IFAQService
{
    private readonly ApplicationDbContext _context;

    public DbFAQService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FAQItem>> GetFAQsAsync()
    {
        return await _context.FAQItems.ToListAsync();
    }

    public async Task<FAQItem?> GetFAQByIdAsync(int id)
    {
        return await _context.FAQItems.FindAsync(id);
    }

    public async Task SaveFAQAsync(FAQItem item)
    {
        if (item.Id == 0)
        {
            _context.FAQItems.Add(item);
        }
        else
        {
            _context.FAQItems.Update(item);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFAQAsync(int id)
    {
        var item = await _context.FAQItems.FindAsync(id);
        if (item != null)
        {
            _context.FAQItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}

public class DbContactService : IContactService
{
    private readonly ApplicationDbContext _context;

    public DbContactService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContactMessage>> GetMessagesAsync()
    {
        return await _context.ContactMessages.OrderByDescending(m => m.ReceivedDate).ToListAsync();
    }

    public async Task SaveMessageAsync(ContactMessage msg)
    {
        _context.ContactMessages.Add(msg);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int id)
    {
        var msg = await _context.ContactMessages.FindAsync(id);
        if (msg != null)
        {
            msg.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ArchiveMessageAsync(int id)
    {
        var msg = await _context.ContactMessages.FindAsync(id);
        if (msg != null)
        {
            msg.IsArchived = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteMessageAsync(int id)
    {
        var msg = await _context.ContactMessages.FindAsync(id);
        if (msg != null)
        {
            _context.ContactMessages.Remove(msg);
            await _context.SaveChangesAsync();
        }
    }
}
