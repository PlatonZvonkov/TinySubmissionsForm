using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TinyForm.Application.Services;
using TinyForm.Core.Interfaces;
using TinyForm.Infrastructure;
using TinyForm.Infrastructure.Repositories;
using TinyForm.WebAPI.Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();
builder.WebHost.UseIISIntegration();


// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        }
    );
// Chose what to use for DB in appconfig.json
bool useEfInMemory = builder.Configuration.GetValue<bool>("UseEfInMemory");
if (useEfInMemory)
{
    builder.Services.AddDbContext<SubmissionDbContext>(context =>
        context.UseInMemoryDatabase("SubmissionsDb"));

    builder.Services.AddScoped<ISubmissionRepository, EFSubmissionRepository>();
}
else
{    
    builder.Services.AddSingleton<ISubmissionRepository>(repository =>
    {
        var env = repository.GetRequiredService<IWebHostEnvironment>();
        var logger = repository.GetRequiredService<ILogger<FileSubmissionRepository>>();

        var dataDirectory = Path.Combine(env.ContentRootPath, "data");
        Directory.CreateDirectory(dataDirectory);
        string filePath = Path.Combine(dataDirectory, "submissions.json");

        return new FileSubmissionRepository(filePath, logger);
    });
}

builder.Services.AddTransient<ISubmissionService>(service =>
{
    var repo = service.GetRequiredService<ISubmissionRepository>();
    ISubmissionService core = new SubmissionService(repo);
    var logger = service.GetRequiredService<ILogger<SubmissionServiceLoggingDecorator>>();
    return new SubmissionServiceLoggingDecorator(core, logger);
});

builder.Services.AddCors(p => p.AddDefaultPolicy(b =>
{
    b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromSeconds(1);
    });
});

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var repo = scope.ServiceProvider.GetRequiredService<ISubmissionRepository>();
//    await repo.InitializeAsync();
//}

app.UseCors();

app.MapControllers();

app.Run();
