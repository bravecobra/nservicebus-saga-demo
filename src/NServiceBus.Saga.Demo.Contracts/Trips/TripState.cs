namespace NServiceBus.Saga.Demo.Contracts.Trips;

public class TripState: IMessage
{
    public Guid TripId { get; init; }
    public DateTime Timestamp { get; init; }
    public string State { get; init; } = null!;
    public bool OutboundFlightBooked { get; init; }
    public bool ReturnFlightBooked { get; init; }
    public bool HotelBooked { get; init; }
}