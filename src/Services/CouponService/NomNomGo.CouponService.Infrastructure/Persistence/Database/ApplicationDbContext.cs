using Microsoft.EntityFrameworkCore;
using NomNomGo.CouponService.Domain.Entities;

namespace NomNomGo.CouponService.Infrastructure.Persistence.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<CouponType> CouponTypes => Set<CouponType>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();

}