using HeimdallMini.Domain.Entities;
using HeimdallMini.Infrastructure.Services.Tokens;
using HeimdallMini.Models;
using HeimdallMini.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HeimdallMini.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LoginContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly IActionResult _defaultError;

        public AuthController(LoginContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _defaultError = BadRequest("Invalid data");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            if(command is { Password: string, Username: string })    //Everything is filled
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == command.Username);

                if (user != null && user.Validate(command.Password))
                {
                    var loginTime = user.Login(HttpContext?.Connection?.RemoteIpAddress?.ToString()!);
                    await _dbContext.SaveChangesAsync();

                    return Ok(_tokenService.CreateLoginToken(user.UserName, loginTime));
                }
            }

            return _defaultError;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            if(command is { Password: string, RepeatPassword: string, Username: string }    //Everything is filled
                && command.Password == command.RepeatPassword                               //Passwords check
                && !await _dbContext.Users.AnyAsync(x => x.UserName == command.Username))    //Already exists
            {
                var user = new User(command.Username, command.Password);
                _dbContext.Add(user);
                await _dbContext.SaveChangesAsync();

                return Ok();
            }

            return _defaultError;
        }

        [HttpPost("passwordChange")]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
        {
            var token = HttpContext.Request.Headers.Authorization.FirstOrDefault();
            var data = await _tokenService.Validate(token!);

            if (command is { OldPassword: string, NewPassword: string }     //Everything is filled
                && command.OldPassword != command.NewPassword               //Passwords check
                && data.HasValue)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == data.Value.Username);
                if (user == null || !user.ChangePassword(command.OldPassword, command.NewPassword)) return _defaultError;
                await _dbContext.SaveChangesAsync();

                return Ok();
            }

            return _defaultError;
        }
    }
}
