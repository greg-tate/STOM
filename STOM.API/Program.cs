using Microsoft.EntityFrameworkCore;
using STOM.API.Data;
using STOM.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<StomDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=(localdb)\\mssqllocaldb;Database=STOM_DB;Integrated Security=true;",
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<BracketGenerator>();
builder.Services.AddScoped<PlayerImportService>();
builder.Services.AddScoped<DatabaseInitializationService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Apply pending migrations automatically on startup
try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Application starting...");
    
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<StomDbContext>();
        var dbInitService = scope.ServiceProvider.GetRequiredService<DatabaseInitializationService>();
        
        await dbInitService.InitializeDatabaseAsync(dbContext);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Fatal error during database initialization. Application will not start.");
    throw;
}

app.UseCors("AllowBlazor");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
