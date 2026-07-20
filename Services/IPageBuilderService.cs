using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Services;

public interface IPageBuilderService
{
    Task<List<PageSection>> GetPageSectionsAsync(string pageKey);
    Task UpdatePageSectionsAsync(string pageKey, List<PageSection> sections);
}
