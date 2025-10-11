using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pastebin.Models;

namespace Pastebin.Application.Configurations;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.UserId, e.PasteId }).IsUnique();
        builder.HasOne(e => e.User)
              .WithMany(u => u.Likes)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Paste)
              .WithMany(p => p.Likes)
              .HasForeignKey(e => e.PasteId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
