namespace NServiceBus.Saga.Demo.Contracts.Trips;

public class TripSubmissionResponse: IMessage
{
    public bool Succeeded { get; init; }
    public string? Reason { get; init; }
}