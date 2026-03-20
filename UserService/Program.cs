using UserService.Data;
using Microsoft.EntityFrameworkCore;
using UserService.AsyncDataServices;
using UserService.TokenServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserService.AuthService;

var builder = WebApplication.CreateBuilder(args);

// Add services

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Database Context
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("DevelopmentDb"));

// Repositories for database access
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IRefreshTokenRepo, RefreshTokenRepo>();

// Message bus client
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();

// AutoMapper (Mapping between DTOs and Models)
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Jwt and user authentication
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

// Add authorization service
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Configure controller Endpoints
app.MapControllers();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Seed database
PrepDb.PrepPopulation(app);

app.Run();