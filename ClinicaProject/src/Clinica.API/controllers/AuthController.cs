using Clinica.API.Models.DTOs;
using Clinica.API.Services;
using Microsoft.AspNetCore.Mvc;
 
namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
 
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
 
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO model)
        {
            try
            {
                var sucesso = await _authService.CadastrarUsuarioAsync(model);
                if (!sucesso) return BadRequest("Não foi possível criar o usuário.");
                return Ok("Usuário criado com sucesso!");
            }
            catch (ArgumentException ex)         { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO model)
        {
            var token = await _authService.LoginAsync(model);
            if (token == null) return Unauthorized("E-mail ou senha inválidos.");
            return Ok(new { token });
        }
    }
}