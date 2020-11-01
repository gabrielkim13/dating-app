using api.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
  public class DataContext : DbContext
  {
    public DataContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<AppUser> Users { get; set; }

    public DbSet<UserLike> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<UserLike>()
        .HasKey(userLike => new { userLike.SourceUserId, userLike.LikedUserId });

      builder.Entity<UserLike>()
        .HasOne(userLike => userLike.SourceUser)
        .WithMany(userLike => userLike.LikedUsers)
        .HasForeignKey(userLike => userLike.SourceUserId)
        .OnDelete(DeleteBehavior.Cascade);

      builder.Entity<UserLike>()
        .HasOne(userLike => userLike.LikedUser)
        .WithMany(userLike => userLike.LikedByUsers)
        .HasForeignKey(userLike => userLike.LikedUserId)
        .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
