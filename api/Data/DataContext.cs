using api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
  public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
  {
    public DataContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<UserLike> Likes { get; set; }

    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<AppUser>()
        .HasMany(user => user.UserRoles)
        .WithOne(userRole => userRole.User)
        .HasForeignKey(userRole => userRole.UserId)
        .IsRequired();

      builder.Entity<AppRole>()
        .HasMany(role => role.UserRoles)
        .WithOne(userRole => userRole.Role)
        .HasForeignKey(userRole => userRole.RoleId)
        .IsRequired();

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

      builder.Entity<Message>()
        .HasOne(message => message.Recipient)
        .WithMany(user => user.MessagesReceived)
        .OnDelete(DeleteBehavior.Restrict);

      builder.Entity<Message>()
        .HasOne(message => message.Sender)
        .WithMany(user => user.MessagesSent)
        .OnDelete(DeleteBehavior.Restrict);
    }
  }
}
