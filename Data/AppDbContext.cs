using Microsoft.EntityFrameworkCore;
using PRIV.Models;

namespace PRIV.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ConnectionRequest> ConnectionRequests => Set<ConnectionRequest>();
    public DbSet<BlockedTime> BlockedTimes => Set<BlockedTime>();
    public DbSet<BookingRequest> BookingRequests => Set<BookingRequest>();
    public DbSet<BookingLocationOption> BookingLocationOptions => Set<BookingLocationOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User 
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.UsernameNormalized).IsUnique();
        });

        // ConnectionRequest
        modelBuilder.Entity<ConnectionRequest>(entity =>
        {
            entity.HasOne(c => c.Requester)
                  .WithMany()
                  .HasForeignKey(c => c.RequesterId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Target)
                  .WithMany()
                  .HasForeignKey(c => c.TargetId)
                  .OnDelete(DeleteBehavior.Restrict);

            // A given requester can only have one active connection record per target.
            entity.HasIndex(c => new { c.RequesterId, c.TargetId }).IsUnique();
        });

        //BlockedTime 
        modelBuilder.Entity<BlockedTime>(entity =>
        {
            entity.HasOne(b => b.User)
                  .WithMany(u => u.BlockedTimes)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // BookingRequest 
        modelBuilder.Entity<BookingRequest>(entity =>
        {
            entity.HasOne(b => b.Requester)
                  .WithMany()
                  .HasForeignKey(b => b.RequesterId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Target)
                  .WithMany()
                  .HasForeignKey(b => b.TargetId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.ConfirmedLocationOption)
                  .WithMany()
                  .HasForeignKey(b => b.ConfirmedLocationOptionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        //BookingLocationOption 
        modelBuilder.Entity<BookingLocationOption>(entity =>
        {
            entity.HasOne(o => o.BookingRequest)
                  .WithMany(b => b.LocationOptions)
                  .HasForeignKey(o => o.BookingRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
