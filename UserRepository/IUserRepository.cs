using Entities;

namespace Repositories
{
    public interface IUserRepository
    {
        Task<User> AddUser(User user);
        Task<User?> FindUser(string email);
        Task<User> GetById(int id);
        Task UpdateUser(User user);
    }
}