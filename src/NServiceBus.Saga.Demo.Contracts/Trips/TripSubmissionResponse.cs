// ReSharper disable InconsistentNaming
namespace NServiceBus.Saga.Demo.Contracts.Trips;

public class TripSubmissionResponse: IMessage
{
    public bool Succeeded { get; set; }
    public string? Reason { get; init; }
}