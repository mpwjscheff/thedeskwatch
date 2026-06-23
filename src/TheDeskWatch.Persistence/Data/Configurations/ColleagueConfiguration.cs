using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDeskWatch.Domain;

namespace TheDeskWatch.Persistence.Data.Configurations;

internal sealed class ColleagueConfiguration : IEntityTypeConfiguration<Colleague>
{
    public void Configure(EntityTypeBuilder<Colleague> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.HexColor)
            .IsRequired()
            .HasMaxLength(7);

        builder.HasMany(e => e.Departures)
            .WithOne(d => d.Colleague)
            .HasForeignKey(d => d.ColleagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
