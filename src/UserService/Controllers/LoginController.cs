using PrivateUtilities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserService.Models;
using PrivateUtilities.JWT;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtTokenService _jwtTokenService;
    
    public LoginController(AppDbContext context, JwtTokenService jwtTokenService)
    {
         _jwtTokenService = jwtTokenService;

        _context = context;
    }


    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _context.User
            .AsNoTracking()
            .FirstOrDefault(u => u.Email == request.Email);

        if(user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (user.Password != request.Password)
        {
            return Unauthorized(new { message = "Invalid password" });
        }

        var token = _jwtTokenService.GenerateToken(user);
        return Ok(new { token });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {


        foreach (var objectfield in request.GetType().GetProperties())
        {
            if (string.IsNullOrWhiteSpace(objectfield.GetValue(request)?.ToString()))
            {
                return BadRequest(new { message = $"{objectfield.Name} is required" });
            }
        }

        var existingUser = _context.User
            .AsNoTracking()
            .FirstOrDefault(u => u.Email == request.Email);

        if (existingUser != null)
        {
            return Conflict(new { message = "User already exists" });
        }


        var newUser = new User
        {
            Email = request.Email,
            Name = request.Name,
            Surname = request.Surname,
            Password = request.Password,
            Balance = 1000 // Default balance
        };

        _context.User.Add(newUser);
        _context.SaveChanges();

        return CreatedAtAction(nameof(Login), new { email = newUser.Email }, newUser);
    }

}