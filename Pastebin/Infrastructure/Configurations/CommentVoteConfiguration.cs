using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pastebin.Models;

namespace Pastebin.Infrastructure.Configurations;

public class CommentVoteConfiguration : IEntityTypeConfiguration<CommentVote>
{
    public void Configure(EntityTypeBuilder<CommentVote> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.UserId, e.CommentId }).IsUnique();

        builder.HasOne(e => e.User)
              .WithMany(u => u.CommentVotes)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Comment)
              .WithMany(c => c.Votes)
              .HasForeignKey(e => e.CommentId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
