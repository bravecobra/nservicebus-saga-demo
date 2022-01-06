namespace NServiceBus.Saga.Demo.Contracts.Trips;

public class TripStateRequest: IMessage
{
    public Guid TripId { get; init; }
}