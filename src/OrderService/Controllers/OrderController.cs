
using PrivateUtilities.Models;
using PrivateUtilities.EventBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateUtilities.JWT;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ProductClient _productClient;
    private readonly UserClient _userClient;

    public OrderController(AppDbContext context, JwtTokenService jwtTokenService, ProductClient productClient, UserClient userClient)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _productClient = productClient;
        _userClient = userClient;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetOrdersByUser(int userId)
    {
        var orders = await _context.Order.Where(o => o.UserId == userId).ToListAsync();


        var result = new List<OrderItem>();

        var products = await _productClient.GetProductsAsync();

        foreach (var order in orders)
        {
            var product = products.FirstOrDefault(p => p.Id == order.ProductId);
            if (product != null)
            {
                result.Add(new OrderItem(order, product));
            }
        }
        return Ok(result);
    }
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        // JWT içinden userId'yi çekiyoruz
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found in token.");
        }

        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest("Invalid User ID format.");
        }

        // Siparişleri userId ile filtreleyip çekiyoruz
        var orders = await _context.Order.Where(o => o.UserId == userId).ToListAsync();

        var result = new List<OrderItem>();
        var products = await _productClient.GetProductsAsync();

        foreach (var order in orders)
        {
            var product = products.FirstOrDefault(p => p.Id == order.ProductId);
            if (product != null)
            {
                result.Add(new OrderItem(order, product));
            }
        }

        return Ok(result);
    }
  
    [Authorize]
    [HttpPost]
    public IActionResult CreateOrder([FromBody] OrderRequest request)
    {
        // Token'dan kullanıcı ID'sini al
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("User ID token içerisinde bulunamadı.");
        }

        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest("Geçersiz kullanıcı ID formatı.");
        }

        // Ürünleri al
        var products = _productClient.GetProductsByIdsAsync(
            request.ProductValues.Select(p => p.ProductId).ToList()
        ).Result;

        foreach (var productValue in request.ProductValues)
        {
            var product = products.FirstOrDefault(p => p.Id == productValue.ProductId);
            if (product == null)
            {
                return NotFound($"Ürün bulunamadı: {productValue.ProductId}");
            }

            if (!_productClient.HasEnoughStock(productValue.ProductId, productValue.ProductAmount).Result)
            {
                return BadRequest($"Yetersiz stok: {productValue.ProductId}");
            }
        }

        var totalPrice = products.Sum(p =>
            p.Price * (request.ProductValues.FirstOrDefault(x => x.ProductId == p.Id)?.ProductAmount ?? 0)
        );

        var user = _userClient.GetUserByIdAsync(userId).Result;
        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }

        if (!_userClient.HasEnoughBalanceAsync(userId, totalPrice).Result)
        {
            return BadRequest("Yetersiz bakiye.");
        }

        var orders = new List<Order>();

        foreach (var productValue in request.ProductValues)
        {
            for (int i = 0; i < productValue.ProductAmount; i++)
            {
                orders.Add(new Order
                {
                    UserId = userId,
                    ProductId = productValue.ProductId,
                    OrderDate = DateTime.Now
                });
            }
        }

        try
        {
            _userClient.UpdateBalanceAsync(userId, totalPrice).Wait();

            foreach (var productValue in request.ProductValues)
            {
                _productClient.UpdateStock(productValue.ProductId, productValue.ProductAmount).Wait();
            }

            _context.Order.AddRange(orders);
            _context.SaveChanges();
            return Ok("Sipariş oluşturuldu ve yayınlandı.");
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, $"Veritabanı hatası: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
        }
    }

}

public class OrderRequest
{
    public int UserId { get; set; }
    public List<ProductValues> ProductValues { get; set; } = new List<ProductValues>();
}

public class ProductValues
{

    public int ProductId { get; set; }
    public int ProductAmount { get; set; }
}

public class OrderItem
{
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public int OrderId { get; set; }
    public ProductDto? productDto { get; set; }

    public OrderItem(Order order, ProductDto productDto)
    {
        UserId = order.UserId;
        OrderDate = order.OrderDate;
        OrderId = order.Id;
        this.productDto = productDto;
    }
}