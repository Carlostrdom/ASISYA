using Asisya.Application.DTOs.Auth;
using Asisya.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Asisya.API.Controllers;

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
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var success = await _authService.RegisterAsync(dto);
        if (!success)
            return Conflict(new { message = "El usuario ya existe." });

        return Ok(new { message = "Usuario registrado exitosamente." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        if (token is null)
            return Unauthorized(new { message = "Credenciales inválidas." });

        return Ok(token);
    }
}
