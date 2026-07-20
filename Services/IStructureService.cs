using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Services;

public interface IStructureService
{
    Task<List<NavMenuItem>> GetNavMenuItemsAsync();
    Task<List<SectionSetting>> GetHomepageSectionsAsync();
    Task UpdateSectionOrderAsync(List<SectionSetting> sections);
}
