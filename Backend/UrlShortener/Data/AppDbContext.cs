using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Models;

namespace UrlShortener.API.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; } = null!; // Add this line

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the relationship between ShortenedUrl and IdentityUser
            builder.Entity<ShortenedUrl>()
                .HasOne(su => su.CreatedByUser)
                .WithMany()
                .HasForeignKey(su => su.CreatedByUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete of user when URL is deleted
        }
    }
}
