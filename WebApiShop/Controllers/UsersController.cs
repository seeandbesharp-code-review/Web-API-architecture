using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using DTO_s;

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserServices userServices, ILogger<UsersController> logger)
        {
            _userServices = userServices;
            _logger = logger;
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetById(int id)
        {
            UserDTO user = await _userServices.GetById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        // POST api/<UsersController>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserDTO>> NewUser([FromBody] PostUserDTO user)
        {
            AuthResponseDTO? result = await _userServices.AddUser(user);
            if (result == null)
                return BadRequest("Password too weak. Minimum required strength: 2/4");
            AppendJwtCookie(result.Token);
            return CreatedAtAction(nameof(GetById), new { id = result.User.Id }, result.User);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDTO>> Login([FromBody] LoginUser user)
        {
            AuthResponseDTO? result = await _userServices.FindUser(user);
            if (result == null)
                return Unauthorized();
            _logger.LogInformation($"Login attempted with Email {user.Email}");
            AppendJwtCookie(result.Token);
            return Ok(result.User);
        }

        private void AppendJwtCookie(string token)
        {
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateUser([FromBody]PostUserDTO user)
        {
            bool res = await _userServices.UpdateUser(user);
            if(!res)
                return BadRequest("User details didn't update");
            return Ok();
        }
    }
}