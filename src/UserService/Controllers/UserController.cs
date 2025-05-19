using PrivateUtilities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }


    // GET: api/user
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.User.ToListAsync();
        return Ok(users);
    }

    // GET: api/user/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.User.FindAsync(id);
        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı");
        }
        return Ok(user);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyInfo()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("Token içerisinde kullanıcı bilgisi bulunamadı.");
        }

        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest("Geçersiz kullanıcı ID formatı.");
        }

        var user = await _context.User.FindAsync(userId);
        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı");
        }

        return Ok(user);
    }


    [HttpGet("HasEnoughBalance/{id}/{amount}")]
    public async Task<IActionResult> HasEnoughBalance(int id, decimal amount)
    {
        var user = await _context.User.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.Balance >= amount)
        {
            return Ok(true);
        }
        else
        {
            return Ok(false);
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateBalance(int id, [FromBody] decimal amount)
    {
        var user = await _context.User.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.Balance -= amount;
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    

}