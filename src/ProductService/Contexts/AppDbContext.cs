using Microsoft.EntityFrameworkCore;
using PrivateUtilities.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ProductDto> Products { get; set; }
}
