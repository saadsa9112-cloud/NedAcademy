using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Data;
using NedAcademyWebsite.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// Register Business CMS Services
builder.Services.AddScoped<ISettingService, DbSettingService>();
builder.Services.AddScoped<IProgramService, DbProgramService>();
builder.Services.AddScoped<IFacultyService, DbFacultyService>();
builder.Services.AddScoped<ILeadershipService, DbLeadershipService>();
builder.Services.AddScoped<IResearchService, DbResearchService>();
builder.Services.AddScoped<ICampusService, DbCampusService>();
builder.Services.AddScoped<INewsService, DbNewsService>();
builder.Services.AddScoped<IDownloadService, DbDownloadService>();
builder.Services.AddScoped<IFAQService, DbFAQService>();
builder.Services.AddScoped<IContactService, DbContactService>();
builder.Services.AddScoped<IStructureService, StructureService>();
builder.Services.AddScoped<IPageBuilderService, PageBuilderService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IAdmissionService, AdmissionService>();

// Register Cookie Authentication
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.LogoutPath = "/Admin/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Auto-migration and Database Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        DbSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
