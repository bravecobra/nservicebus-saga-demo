using NServiceBus.Saga.Demo.Contracts.Flights;

namespace NServiceBus.Saga.Demo.TransportationService.Application;

public class FlightBookingConsumer : IHandleMessages<BookFlightRequest>
{
    private readonly ILogger<FlightBookingConsumer> _logger;

    public FlightBookingConsumer(ILogger<FlightBookingConsumer> logger)
    {
        _logger = logger;
    }
    public async Task Handle(BookFlightRequest message, IMessageHandlerContext context)
    {
        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(15, 30)));
        await context.Publish(new FlightBooked
        {
            TripId =  message.TripId,
            IsOutbound = message.IsOutbound,
            FlightId = Guid.NewGuid(),
            Cost = Convert.ToDecimal(Random.Shared.NextDouble() * 90 + 10),
            Company = Random.Shared.NextDouble() > .5d ? "Ryanair" : "EasyJet"
        });
        _logger.LogInformation("Booked flight for {TripId}", message.TripId);
    }
}