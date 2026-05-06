using DTO_s;

namespace Service
{
    public interface IJwtService
    {
        string GenerateToken(UserDTO user);
    }
}
