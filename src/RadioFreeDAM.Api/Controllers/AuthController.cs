using Microsoft.AspNetCore.Mvc;
using RadioFreeDAM.Api.Data.Entities;
using RadioFreeDAM.Api.Helpers;
using RadioFreeDAM.Api.Contracts;
using RadioFreeDAM.Api.Data.Repositories;

namespace RadioFreeDAM.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserRepository _userRepository;

    public AuthController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserEntity user)
    {
        try 
        {
            if (await _userRepository.ExistsByEmailAsync(user.Email))
                return BadRequest(new { message = "El email ya está registrado" });

            if (await _userRepository.ExistsByUsernameAsync(user.Username))
                return BadRequest(new { message = "El nombre de usuario ya está en uso" });

            user.CreatedAt = DateTime.UtcNow;
            user.PasswordHash = SecurityHelper.HashPassword(user.PasswordHash.Trim());

            await _userRepository.AddAsync(user);

            user.PasswordHash = "";
            return Ok(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH-REG] Error: {ex.Message}");
            return StatusCode(500, new { message = "Error interno durante el registro" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest login)
    {
        Console.WriteLine($"[AUTH-DEBUG] Login request received for User: {login.Username}");
        try 
        {
            if (login == null) {
                Console.WriteLine("[AUTH-DEBUG] Login request body is NULL");
                return BadRequest(new { message = "Body nulo" });
            }

            var username = login.Username?.Trim();
            var password = login.Password?.Trim();

            Console.WriteLine($"[AUTH-DEBUG] Searching for user: {username}");
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null)
            {
                Console.WriteLine($"[AUTH-DEBUG] User not found: {username}");
                return Unauthorized(new { message = "Usuario no encontrado" });
            }

            Console.WriteLine($"[AUTH-DEBUG] User found. Verifying password...");
            if (!SecurityHelper.VerifyPassword(password, user.PasswordHash))
            {
                Console.WriteLine($"[AUTH-DEBUG] Invalid password for: {username}");
                return Unauthorized(new { message = "Contraseña incorrecta" });
            }

            Console.WriteLine($"[AUTH-DEBUG] Login SUCCESS for: {username}. Sending response...");
            user.PasswordHash = "";
            return Ok(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH-ERROR] CRITICAL: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new { message = "Error interno durante el login" });
        }
    }
}
