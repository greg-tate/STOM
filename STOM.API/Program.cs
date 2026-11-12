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
        ?? "Server=(localdb)\\mssqllocaldb;Database=STOM_DB;Integrated Security=true;"));

builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<BracketGenerator>();
builder.Services.AddScoped<PlayerImportService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Apply pending migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StomDbContext>();
    dbContext.Database.Migrate();
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
