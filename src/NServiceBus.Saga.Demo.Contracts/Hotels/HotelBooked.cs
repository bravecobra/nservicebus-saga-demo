namespace NServiceBus.Saga.Demo.Contracts.Hotels;

public class HotelBooked: IEvent
{
    public Guid TripId { get; init; }

    public int Stars { get; init; }
    public string HotelName { get; init; } = null!;
    public Guid HotelBookingId { get; init; }
}