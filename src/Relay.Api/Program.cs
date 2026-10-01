using Relay.Api.Data;
using Relay.Api.Features.LocationNormality;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RelayDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=relay.db"));
builder.Services.AddScoped<LocationNormalityService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RelayDbContext>();
    dbContext.Database.Migrate();
    DatabaseSeeder.EnsureSeeded(dbContext, app.Environment.ContentRootPath);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/location-normality", LocationNormalityEndpoint.GetAsync);

app.Run();
