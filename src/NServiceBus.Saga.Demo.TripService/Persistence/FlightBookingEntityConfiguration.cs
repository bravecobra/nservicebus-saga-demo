﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NServiceBus.Saga.Demo.TripService.Domain;

namespace NServiceBus.Saga.Demo.TripService.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    public class FlightBookingEntityConfiguration : IEntityTypeConfiguration<FlightBooking>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<FlightBooking> builder)
        {
            builder.HasKey(_ => _.FlightId);
            builder.Property(c => c.FlightId).ValueGeneratedNever();
            builder.Property(c => c.FlightCost).HasPrecision(14,2);
        }
    }
}
