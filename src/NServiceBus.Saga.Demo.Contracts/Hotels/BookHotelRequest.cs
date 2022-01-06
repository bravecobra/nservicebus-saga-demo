namespace NServiceBus.Saga.Demo.Contracts.Hotels;

public class BookHotelRequest: ICommand
{
    public Guid TripId { get; init; }

    public int RequiredStars { get; init; }
    public string Location { get; init; } = null!;
}