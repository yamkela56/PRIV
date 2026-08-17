using Microsoft.EntityFrameworkCore;
using PRIV.Models;

namespace PRIV.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<BlockedTime> BlockedTimes { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<BlockedTime>()
                .HasKey(b => b.BlockId);

            
            modelBuilder.Entity<Connection>()
                .HasOne(c => c.Requester)
                .WithMany()
                .HasForeignKey(c => c.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Connection>()
                .HasOne(c => c.Target)
                .WithMany()
                .HasForeignKey(c => c.TargetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Requester)
                .WithMany()
                .HasForeignKey(b => b.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Host)
                .WithMany()
                .HasForeignKey(b => b.HostId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}