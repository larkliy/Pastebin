using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pastebin.Models;

namespace Pastebin.Infrastructure.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(e => e.User)
              .WithMany(u => u.Comments)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Paste)
              .WithMany(p => p.Comments)
              .HasForeignKey(e => e.PasteId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
