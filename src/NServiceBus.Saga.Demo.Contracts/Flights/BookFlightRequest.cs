namespace NServiceBus.Saga.Demo.Contracts.Flights;

public class BookFlightRequest : ICommand
{
    public Guid TripId { get; init; }

    public DateTime DayOfFlight { get; init; }
    public bool IsOutbound { get; init; }
    public string From { get; init; } = null!;

    public string To { get; init; } = null!;
}