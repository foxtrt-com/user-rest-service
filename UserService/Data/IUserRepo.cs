using UserService.Models;

namespace UserService.Data;

public interface IUserRepo
{
    bool SaveChanges();
    IEnumerable<User> GetAllUsers();
    User GetUserById(int id);
    User GetUserByUsername(string username);
    void CreateUser(User user);
    void UpdateUser(User user);
    void DeleteUser(User user);
}
