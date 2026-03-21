using UserService.Models;

namespace UserService.Data;

public static class PrepDb
{
    public static void PrepPopulation(IApplicationBuilder app)
    {
        // Get application service scope to access AppDbContext
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>());
        }
    }

    private static void SeedData(AppDbContext context)
    {
        // If database not seeded, seed database with examples
        if (!context.Users.Any())
        {
            Console.WriteLine("Seeding data...");

            context.Users.AddRange(
                new User { Username = "Admin", Email = "admin@example.com" },
                new User { Username = "User", Email = "user@example.com" }
            );
            context.SaveChanges();
        }
    }
}
