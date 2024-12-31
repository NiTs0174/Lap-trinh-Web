using Microsoft.EntityFrameworkCore;
using NguyenMinhThang.Models;
namespace B3WebsiteBanHang.DataAccess;
using B3WebsiteBanHang.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NguyenMinhThang.Models;
using System.Collections.Generic;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    //3----------------------------------------
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    //3----------------------------------------
    public DbSet<NguyenMinhThang.Models.CartItem> CartItem { get; set; } = default!;
}
