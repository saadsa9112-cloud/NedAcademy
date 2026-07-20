using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Data;
using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Services;

public class StructureService : IStructureService
{
    private readonly ApplicationDbContext _context;

    public StructureService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NavMenuItem>> GetNavMenuItemsAsync()
    {
        return await _context.NavMenuItems
            .Where(m => m.IsVisible)
            .OrderBy(m => m.SortOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SectionSetting>> GetHomepageSectionsAsync()
    {
        var list = await _context.SectionSettings
            .OrderBy(s => s.SortOrder)
            .AsNoTracking()
            .ToListAsync();

        if (!list.Any())
        {
            list = new List<SectionSetting>
            {
                new SectionSetting { SectionKey = "Hero", DisplayName = "Hero Section Banner", IsVisible = true, SortOrder = 1 },
                new SectionSetting { SectionKey = "Programs", DisplayName = "Featured Academic Programs", IsVisible = true, SortOrder = 2 },
                new SectionSetting { SectionKey = "Research", DisplayName = "Research & Innovation Labs", IsVisible = true, SortOrder = 3 },
                new SectionSetting { SectionKey = "Facilities", DisplayName = "Campus Facilities Preview", IsVisible = true, SortOrder = 4 },
                new SectionSetting { SectionKey = "News", DisplayName = "News Feed & Events Hub", IsVisible = true, SortOrder = 5 },
                new SectionSetting { SectionKey = "Gallery", DisplayName = "Visual Campus Gallery Grid", IsVisible = true, SortOrder = 6 },
                new SectionSetting { SectionKey = "CTA", DisplayName = "Admissions Conversion Call", IsVisible = true, SortOrder = 7 }
            };
            _context.SectionSettings.AddRange(list);
            await _context.SaveChangesAsync();
        }

        return list;
    }

    public async Task UpdateSectionOrderAsync(List<SectionSetting> sections)
    {
        foreach (var item in sections)
        {
            var existing = await _context.SectionSettings.FindAsync(item.Id);
            if (existing != null)
            {
                existing.SortOrder = item.SortOrder;
                existing.IsVisible = item.IsVisible;
                existing.BackgroundStyle = item.BackgroundStyle;
            }
        }
        await _context.SaveChangesAsync();
    }
}
