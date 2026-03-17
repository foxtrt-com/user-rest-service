using UserService.Data;
using Microsoft.EntityFrameworkCore;
using UserService.AsyncDataServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("DevelopmentDb"));

builder.Services.AddScoped<IUserRepo, UserRepo>();

builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();

builder.Services.AddControllers();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Configure controller Endpoints
app.MapControllers();

// Seed database
PrepDb.PrepPopulation(app);

app.Run();