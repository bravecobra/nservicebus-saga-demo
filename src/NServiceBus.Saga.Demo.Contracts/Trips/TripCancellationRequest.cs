namespace NServiceBus.Saga.Demo.Contracts.Trips;

public class TripCancellationRequest: ICommand
{
    public Guid TripId { get; init; }

    public string Reason { get; init; } = "No reason given";
}