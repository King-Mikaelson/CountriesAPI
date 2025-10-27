using CountriesAPI.Data;
using CountriesAPI.Services;
using CountryCurrencyAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
    connectionString,
    ServerVersion.AutoDetect(connectionString),
      mysqlOptions => mysqlOptions.CommandTimeout(300)
      )
    );

//builder.Services.Configure<ApiBehaviorOptions>(options =>
//{
//    // Disable automatic 400 responses from model binding
//    options.SuppressModelStateInvalidFilter = true;
//});
// Add services to the container.

builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddControllers();

// Register HttpClient for External API Service
builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2); // 30 seconds timeout
    client.DefaultRequestHeaders.Add("User-Agent", "CountriesAPI/1.0");
});

// Configure Kestrel to allow longer requests
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

// Also increase the default request timeout
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter("default", options =>
{
    options.PermitLimit = 10;
    options.Window = TimeSpan.FromSeconds(10);
    options.QueueLimit = 0;
}));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();


//app.UseMiddleware<StringAnalyzer.Middleware.Middleware>();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
