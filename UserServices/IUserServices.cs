using DTO_s;

namespace Service
{
    public interface IUserServices
    {
        Task<AuthResponseDTO?> AddUser(PostUserDTO user);
        Task<AuthResponseDTO?> FindUser(LoginUser user);
        Task<UserDTO> GetById(int id);
        Task<bool> UpdateUser(PostUserDTO user);
    }
}