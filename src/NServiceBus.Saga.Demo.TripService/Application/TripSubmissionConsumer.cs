using NServiceBus.Saga.Demo.Contracts.Trips;
using NServiceBus.Saga.Demo.TripService.Domain;

namespace NServiceBus.Saga.Demo.TripService.Application;

public class TripSubmissionConsumer: IHandleMessages<SubmitTrip>
{
    private readonly ILogger<TripSubmissionConsumer> _logger;

    public TripSubmissionConsumer(ILogger<TripSubmissionConsumer> logger)
    {
        _logger = logger;
    }
    public async Task Handle(SubmitTrip message, IMessageHandlerContext context)
    {
        var validationResult = Trip.Validate(message);
        if (validationResult.IsFailure)
        {
            await context.Reply(new TripSubmissionResponse
            {
                Succeeded = false,
                Reason = validationResult.Error
            } );
            _logger.LogError($"TripRequest for {message.Destination} invalid: {validationResult.Error}");
            return;
        }

        await context.Send(new TripRegistrationRequest
        {
            TripId = message.TripId,
            RequiredStars = message.RequiredStars,
            Destination = message.Destination,
            Start = message.Start,
            End = message.End
        });
        _logger.LogInformation($"TripRequest for {message.Destination} accepted");
        await context.Reply(new TripSubmissionResponse {Succeeded = true});
    }
}