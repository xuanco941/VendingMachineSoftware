using Microsoft.EntityFrameworkCore;
using StyleX.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace StyleX.Models
{
    public class DatabaseContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<OrderProduct> OrderProducts { get; set; } = null!; 




        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration)
    : base(options)
        {
            _configuration = configuration;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //optionsBuilder.UseSqlServer(_configuration["ConnectionStrings:DefaultConnection"]);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Account
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }


    }
}