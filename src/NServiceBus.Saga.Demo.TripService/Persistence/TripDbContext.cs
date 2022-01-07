﻿using Microsoft.EntityFrameworkCore;
using NServiceBus.Saga.Demo.TripService.Domain;

namespace NServiceBus.Saga.Demo.TripService.Persistence;

public class TripDbContext : DbContext
{
    public TripDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TripEntityConfiguration());
        modelBuilder.ApplyConfiguration(new FlightBookingEntityConfiguration());
        modelBuilder.ApplyConfiguration(new HotelBookingEntityConfiguration());
    }

    public DbSet<Trip> TripStates { get; set; }
}