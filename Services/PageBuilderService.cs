using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Data;
using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Services;

public class PageBuilderService : IPageBuilderService
{
    private readonly ApplicationDbContext _context;

    public PageBuilderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PageSection>> GetPageSectionsAsync(string pageKey)
    {
        var sections = await _context.PageSections
            .Where(s => s.PageKey == pageKey)
            .OrderBy(s => s.SortOrder)
            .AsNoTracking()
            .ToListAsync();

        if (!sections.Any())
        {
            // Seed default page sections for target page
            sections = new List<PageSection>
            {
                new PageSection { PageKey = pageKey, SectionKey = "HeaderBanner", SectionTitle = $"{pageKey} Overview Banner", IsVisible = true, SortOrder = 1, BackgroundStyle = "Dark" },
                new PageSection { PageKey = pageKey, SectionKey = "MainContent", SectionTitle = $"{pageKey} Primary Details", IsVisible = true, SortOrder = 2, BackgroundStyle = "Glass" },
                new PageSection { PageKey = pageKey, SectionKey = "FooterCta", SectionTitle = $"{pageKey} Conversion CTA", IsVisible = true, SortOrder = 3, BackgroundStyle = "Dark" }
            };
            _context.PageSections.AddRange(sections);
            await _context.SaveChangesAsync();
        }

        return sections;
    }

    public async Task UpdatePageSectionsAsync(string pageKey, List<PageSection> sections)
    {
        foreach (var sec in sections)
        {
            var item = await _context.PageSections.FindAsync(sec.Id);
            if (item != null)
            {
                item.SectionTitle = sec.SectionTitle;
                item.SectionSubtitle = sec.SectionSubtitle;
                item.IsVisible = sec.IsVisible;
                item.SortOrder = sec.SortOrder;
                item.BackgroundStyle = sec.BackgroundStyle;
                item.ContainerWidth = sec.ContainerWidth;
                item.PaddingStyle = sec.PaddingStyle;
                item.AnimationType = sec.AnimationType;
            }
        }
        await _context.SaveChangesAsync();
    }
}
