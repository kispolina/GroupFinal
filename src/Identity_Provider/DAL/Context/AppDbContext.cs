using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> dbContext) : DbContext(dbContext)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
       protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.Property(u => u.Name).IsRequired().HasMaxLength(100);
                b.Property(u => u.Email).IsRequired().HasMaxLength(256);
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.PasswordHash).IsRequired();
                b.Property(u => u.Role).IsRequired().HasMaxLength(50);
                b.Property(u => u.CreatedAt).IsRequired();
            });

            builder.Entity<RefreshToken>(b =>
            {
                b.HasKey(rt => rt.Id);
                b.Property(rt => rt.Token).IsRequired();
                b.Property(rt => rt.Expires).IsRequired();
                b.Property(rt => rt.Created).IsRequired();
                b.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId);
            });

            base.OnModelCreating(builder);
        }
    }
}
