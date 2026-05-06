using AutoMapper;
using DTO_s;
using Entities;
using Repositories;

namespace Service
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository _userRepositories;
        private readonly IPasswordServices _passwordServices;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public UserServices(IUserRepository userRepositories, IMapper mapper, IPasswordServices passwordServices, IJwtService jwtService)
        {
            _userRepositories = userRepositories;
            _passwordServices = passwordServices;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        public async Task<UserDTO> GetById(int id)
        {
            User user = await _userRepositories.GetById(id);
            UserDTO userDTO = _mapper.Map<User, UserDTO>(user);
            return userDTO;
        }

        public async Task<AuthResponseDTO?> AddUser(PostUserDTO user)
        {
            if (_passwordServices.GetStrength(user.Password).Strength <= 2)
                return null;
            User userEntity = _mapper.Map<PostUserDTO, User>(user);
            userEntity.Password = user.Password;
            User result = await _userRepositories.AddUser(userEntity);
            UserDTO userDTO = _mapper.Map<User, UserDTO>(result);
            string token = _jwtService.GenerateToken(userDTO);
            return new AuthResponseDTO(userDTO, token);
        }

        public async Task<AuthResponseDTO?> FindUser(LoginUser user)
        {
            User? res = await _userRepositories.FindUser(user);
            if (res == null)
                return null;
            UserDTO userDTO = _mapper.Map<User, UserDTO>(res);
            string token = _jwtService.GenerateToken(userDTO);
            return new AuthResponseDTO(userDTO, token);
        }

        public async Task<bool> UpdateUser(PostUserDTO user)
        {
            Password password1 = _passwordServices.GetStrength(user.Password);
            if (password1.Strength <= 2)
                return false;
            User userToUpdate = _mapper.Map<PostUserDTO, User>(user);
            userToUpdate.Id = user.Id;
            userToUpdate.Password = user.Password;
            await _userRepositories.UpdateUser(userToUpdate);
            return true;
        }
    }
}
