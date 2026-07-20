namespace NedAcademyWebsite.Services;

public class SearchResultItem
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-search";
}

public interface ISearchService
{
    Task<List<SearchResultItem>> SearchAsync(string query);
}
