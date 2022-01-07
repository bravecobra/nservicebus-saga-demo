using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NServiceBus.Saga.Demo.TripService.Domain;

namespace NServiceBus.Saga.Demo.TripService.Persistence;

public class HotelBookingEntityConfiguration : IEntityTypeConfiguration<HotelBooking>
{
    public void Configure(EntityTypeBuilder<HotelBooking> builder)
    {
        builder.HasKey(_ => _.HotelBookingId);
        builder.Property(c => c.HotelBookingId).ValueGeneratedNever();
    }
}