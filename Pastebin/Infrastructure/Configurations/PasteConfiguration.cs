using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pastebin.Models;

namespace Pastebin.Infrastructure.Configurations;

public class PasteConfiguration : IEntityTypeConfiguration<Paste>
{
    public void Configure(EntityTypeBuilder<Paste> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.HasOne(e => e.User)
              .WithMany(u => u.Pastes)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.SetNull);
    }
}
