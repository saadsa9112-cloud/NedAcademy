using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Data;

namespace NedAcademyWebsite.Services;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _context;

    public SearchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchResultItem>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<SearchResultItem>();

        query = query.ToLower();
        var results = new List<SearchResultItem>();

        // Search Degree Programs
        var programs = await _context.DegreePrograms
            .Where(p => !p.IsDeleted && (p.Name.ToLower().Contains(query) || p.Code.ToLower().Contains(query)))
            .Take(5)
            .AsNoTracking()
            .ToListAsync();

        foreach (var p in programs)
        {
            results.Add(new SearchResultItem
            {
                Title = p.Name,
                Category = "Degree Program",
                Url = "/Admin/Programs",
                Icon = "fa-graduation-cap"
            });
        }

        // Search Faculty
        var faculty = await _context.FacultyMembers
            .Where(f => !f.IsDeleted && (f.Name.ToLower().Contains(query) || f.Designation.ToLower().Contains(query)))
            .Take(5)
            .AsNoTracking()
            .ToListAsync();

        foreach (var f in faculty)
        {
            results.Add(new SearchResultItem
            {
                Title = f.Name,
                Category = "Faculty Profile",
                Url = "/Admin/Faculty",
                Icon = "fa-users-line"
            });
        }

        // Search News Items
        var news = await _context.NewsItems
            .Where(n => !n.IsDeleted && n.Title.ToLower().Contains(query))
            .Take(5)
            .AsNoTracking()
            .ToListAsync();

        foreach (var n in news)
        {
            results.Add(new SearchResultItem
            {
                Title = n.Title,
                Category = "News Post",
                Url = "/Admin/News",
                Icon = "fa-newspaper"
            });
        }

        // Search Contact Messages
        var msgs = await _context.ContactMessages
            .Where(m => m.Name.ToLower().Contains(query) || m.Subject.ToLower().Contains(query) || m.Email.ToLower().Contains(query))
            .Take(5)
            .AsNoTracking()
            .ToListAsync();

        foreach (var m in msgs)
        {
            results.Add(new SearchResultItem
            {
                Title = $"{m.Name} - {m.Subject}",
                Category = "User Inquiry",
                Url = "/Admin/Messages",
                Icon = "fa-envelope"
            });
        }

        return results;
    }
}
