using Microsoft.EntityFrameworkCore;
using Pastebin.Application;
using Pastebin.ConfigurationSettings;
using Pastebin.Endpoints;
using Pastebin.Handlers;
using Pastebin.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddValidation();

builder.Services.AddProblemDetails(options => 
    options.CustomizeProblemDetails = ctx => ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapUserEndpoints();

app.Run();
