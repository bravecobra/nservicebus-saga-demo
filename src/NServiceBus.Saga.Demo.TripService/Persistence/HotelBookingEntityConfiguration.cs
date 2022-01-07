using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NServiceBus.Saga.Demo.TripService.Domain;

namespace NServiceBus.Saga.Demo.TripService.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    public class HotelBookingEntityConfiguration : IEntityTypeConfiguration<HotelBooking>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<HotelBooking> builder)
        {
            builder.HasKey(_ => _.HotelBookingId);
            builder.Property(c => c.HotelBookingId).ValueGeneratedNever();
        }
    }
}