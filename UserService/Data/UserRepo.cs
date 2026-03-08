using UserService.Models;

namespace UserService.Data;

public class UserRepo : IUserRepo
{
    private readonly AppDbContext _context;

    public UserRepo(AppDbContext context)
    {
        _context = context;
    }

    public bool SaveChanges()
    {
        // Save changes to Db, return true if changes > 0 were saved
        return _context.SaveChanges() > 0;
    }

    public IEnumerable<User> GetAllUsers()
    {
        // Return all users from Db
        return _context.Users.ToList();
    }

    public User GetUserById(int id)
    {
        // Return user with matching id from Db
        return _context.Users.FirstOrDefault(u => u.Id == id);
    }

    public void CreateUser(User user)
    {
        // Check if user is null, throw exception if it is
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        // Add user to Db
        _context.Users.Add(user);
    }

    public void UpdateUser(User user)
    {
        // Check if user is null, throw exception if it is
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        // Update user in Db
        _context.Users.Update(user);
    }

    public void DeleteUser(User user)
    {
        // Check if user is null, throw exception if it is
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        // Delete user from Db
        _context.Users.Remove(user);
    }
}
