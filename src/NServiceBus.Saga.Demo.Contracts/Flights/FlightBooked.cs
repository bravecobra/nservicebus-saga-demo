

// ReSharper disable InconsistentNaming

namespace NServiceBus.Saga.Demo.Contracts.Flights
{
    public class FlightBooked: IEvent
    {
        public Guid TripId { get; init; }
        public bool IsOutbound { get; init; }
        public Guid FlightId { get; init; }
        public decimal Cost { get; init; }
        public string Company { get; init; } = null!;
    }
}