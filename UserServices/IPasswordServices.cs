using Entities;

namespace Service
{
    public interface IPasswordServices
    {
        Password GetStrength(string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}