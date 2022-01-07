﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NServiceBus.Saga.Demo.TripService.Domain;

namespace NServiceBus.Saga.Demo.TripService.Persistence;

public class TripEntityConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("Trips");
        builder.HasKey(_ => _.CorrelationId);
        builder.HasMany(_ => _.BookedFlights).WithOne();
        builder.HasOne(_ => _.HotelBooking).WithOne().HasForeignKey<HotelBooking>("TripCorrelationId");
        builder.Metadata.FindNavigation($"{nameof(Trip.BookedFlights)}")?.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}