using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDeskWatch.Domain;

namespace TheDeskWatch.Persistence.Data.Configurations;

internal sealed class DeskDepartureConfiguration : IEntityTypeConfiguration<DeskDeparture>
{
    public void Configure(EntityTypeBuilder<DeskDeparture> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DepartedAt)
            .IsRequired();

        builder.HasOne(e => e.Colleague)
            .WithMany(c => c.Departures)
            .HasForeignKey(e => e.ColleagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
