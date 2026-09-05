using Microsoft.EntityFrameworkCore;
using Shopizy.IdentityService.Domain.Entities;
using Shopizy.IdentityService.Domain.ValueObjects;

namespace Shopizy.IdentityService.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .HasConversion(
                    email => email.Value,
                    value => Email.Create(value).Value)
                .HasMaxLength(256)
                .IsRequired();

            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(50).IsRequired();
            builder.Property(u => u.IsActive).IsRequired();
            builder.Property(u => u.CreatedAtUtc).IsRequired();
            builder.Property(u => u.UpdatedAtUtc);

            builder.HasMany(u => u.RefreshTokens)
                .WithOne()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(u => u.RefreshTokens)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.ToTable("refresh_tokens");

            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.Token).HasMaxLength(128).IsRequired();
            builder.HasIndex(rt => rt.Token);
            builder.Property(rt => rt.ExpiresAtUtc).IsRequired();
            builder.Property(rt => rt.CreatedAtUtc).IsRequired();
            builder.Property(rt => rt.RevokedAtUtc);
        });
    }
}
