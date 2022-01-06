namespace NServiceBus.Saga.Demo.Contracts.Trips;

public class TripNotFound: IMessage
{
    public Guid TripId { get; init; }
}