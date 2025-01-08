using Boutique.Core.Domain.Common;
using Boutique.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Boutique.Infrastructure.Persistence.DataContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductDiscount> ProductDiscounts { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Size> Sizes { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
                .HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);

        // Configure Cart -> User (one-to-one)
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Cart)
            .WithOne(c => c.User)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Cart -> CartItem (one-to-many)
        builder.Entity<Cart>()
            .HasMany(c => c.CartItems)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId);

        // Configure CartItem -> ProductVariant (many-to-one)
        builder.Entity<CartItem>()
            .HasOne(ci => ci.ProductVariant)
            .WithMany(pv => pv.CartItems)
            .HasForeignKey(ci => ci.ProductVariantId);

        // Configure Product -> ProductVariant (one-to-many)
        builder.Entity<Product>()
            .HasMany(p => p.ProductVariants)
            .WithOne(pv => pv.Product)
            .HasForeignKey(pv => pv.ProductId);

        // Configure ProductVariant -> Color, Size relationships
        builder.Entity<ProductVariant>()
            .HasOne(pv => pv.Color)
            .WithMany(c => c.ProductVariants)
            .HasForeignKey(pv => pv.ColorId);

        builder.Entity<ProductVariant>()
            .HasOne(pv => pv.Size)
            .WithMany(s => s.ProductVariants)
            .HasForeignKey(pv => pv.SizeId);

        // Configure Order -> User (one-to-many)
        builder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Order -> OrderItem (one-to-many)
        builder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Product -> ProductImage (one-to-many)
        builder.Entity<Product>()
            .HasMany(p => p.Images)
            .WithOne(pi => pi.Product)
            .HasForeignKey(pi => pi.ProductId);

        // Configure Discount -> ProductDiscount (many-to-many)
        builder.Entity<ProductDiscount>()
            .HasKey(pd => new { pd.ProductId, pd.DiscountId });

        builder.Entity<ProductDiscount>()
            .HasOne(pd => pd.Product)
            .WithMany(p => p.ProductDiscounts)
            .HasForeignKey(pd => pd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductDiscount>()
            .HasOne(pd => pd.Discount)
            .WithMany(d => d.ProductDiscounts)
            .HasForeignKey(pd => pd.DiscountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure OrderItem -> ProductVariant (many-to-one)
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.ProductVariant)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Order -> Transaction (one-to-one)
        builder.Entity<Order>()
            .HasOne(o => o.Address)
            .WithMany()
            .HasForeignKey(o => o.AddressId);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entity in base.ChangeTracker.Entries<BaseEntity>().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            entity.Entity.DateModified = DateTime.UtcNow;
            if (entity.State == EntityState.Added)
               {
                 entity.Entity.DateCreated = DateTime.UtcNow;
             }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
