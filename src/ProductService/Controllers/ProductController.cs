using PrivateUtilities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }


    // GET: api/product
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    // GET: api/product/5


    [HttpGet("{ids}")]
    public async Task<IActionResult> GetByIds([FromRoute] int[] ids)
    {
        var products = await _context.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
        if (products == null || products.Count == 0)
            return NotFound();

        return Ok(products);
    }

    [HttpGet("HasEnoughStock/{id}/{amount}")]
    public async Task<IActionResult> HasEnoughStock(int id, int amount)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        if (product.Stock >= amount)
        {
            return Ok(true);
        }
        else
        {
            return Ok(false);
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int amount)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.Stock -= amount;
        await _context.SaveChangesAsync();

        return Ok(product);
    }


}
