using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Services;

public interface ISettingService
{
    Task<SystemSetting> GetSettingsAsync();
    Task UpdateSettingsAsync(SystemSetting setting);
}

public interface IProgramService
{
    Task<List<DegreeProgram>> GetProgramsAsync();
    Task<DegreeProgram?> GetProgramByIdAsync(int id);
    Task<List<Department>> GetDepartmentsAsync();
    Task SaveProgramAsync(DegreeProgram program);
    Task DeleteProgramAsync(int id);
}

public interface IFacultyService
{
    Task<List<Faculty>> GetFacultyAsync();
    Task<Faculty?> GetFacultyByIdAsync(int id);
    Task SaveFacultyAsync(Faculty faculty);
    Task DeleteFacultyAsync(int id);
}

public interface ILeadershipService
{
    Task<List<Leadership>> GetLeadershipAsync();
    Task<Leadership?> GetLeadershipByIdAsync(int id);
    Task SaveLeadershipAsync(Leadership leader);
    Task DeleteLeadershipAsync(int id);
}

public interface IResearchService
{
    Task<List<ResearchItem>> GetResearchItemsAsync();
    Task<ResearchItem?> GetResearchItemByIdAsync(int id);
    Task SaveResearchItemAsync(ResearchItem item);
    Task DeleteResearchItemAsync(int id);
}

public interface ICampusService
{
    Task<List<CampusFacility>> GetFacilitiesAsync();
    Task<CampusFacility?> GetFacilityByIdAsync(int id);
    Task SaveFacilityAsync(CampusFacility facility);
    Task DeleteFacilityAsync(int id);
}

public interface INewsService
{
    Task<List<NewsItem>> GetNewsAsync();
    Task<NewsItem?> GetNewsByIdAsync(int id);
    Task SaveNewsAsync(NewsItem item);
    Task DeleteNewsAsync(int id);

    Task<List<EventItem>> GetEventsAsync();
    Task<EventItem?> GetEventByIdAsync(int id);
    Task SaveEventAsync(EventItem item);
    Task DeleteEventAsync(int id);
}

public interface IDownloadService
{
    Task<List<DownloadFile>> GetFilesAsync();
    Task<DownloadFile?> GetFileByIdAsync(int id);
    Task SaveFileAsync(DownloadFile file);
    Task DeleteFileAsync(int id);
}

public interface IFAQService
{
    Task<List<FAQItem>> GetFAQsAsync();
    Task<FAQItem?> GetFAQByIdAsync(int id);
    Task SaveFAQAsync(FAQItem item);
    Task DeleteFAQAsync(int id);
}

public interface IContactService
{
    Task<List<ContactMessage>> GetMessagesAsync();
    Task SaveMessageAsync(ContactMessage msg);
    Task MarkAsReadAsync(int id);
    Task ArchiveMessageAsync(int id);
    Task DeleteMessageAsync(int id);
}
