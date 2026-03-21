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

            // Create users with identical test passwords: Password123!
            context.Users.AddRange(
                new User { Username = "Admin", Email = "admin@example.com", Password = "$2a$11$YUvVQGsxxr66axP04pzVOu6L4EvtR8M1dUdEiUosZeNMzV2axlcIa", Roles = ["Admin"] },
                new User { Username = "User", Email = "user@example.com", Password = "$2a$11$YUvVQGsxxr66axP04pzVOu6L4EvtR8M1dUdEiUosZeNMzV2axlcIa" }
            );
            context.SaveChanges();
        }
    }
}
