// ReSharper disable InconsistentNaming

namespace NServiceBus.Saga.Demo.Contracts.Trips;

public class TripRegistrationRequest: ICommand
{
    public Guid TripId { get; init; }

    public int RequiredStars { get; init; }
    public string Destination { get; init; } = null!;
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
}