using NServiceBus.Saga.Demo.Contracts.Hotels;

namespace NServiceBus.Saga.Demo.HotelService.Application;

public class HotelBookingConsumer : IHandleMessages<BookHotelRequest>
{
    private readonly ILogger<HotelBookingConsumer> _logger;

    public HotelBookingConsumer(ILogger<HotelBookingConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Handle(BookHotelRequest message, IMessageHandlerContext context)
    {
        await Task.Delay(TimeSpan.FromSeconds(15));
        await context.Publish(new HotelBooked
        {
            TripId = message.TripId,
            Stars = message.RequiredStars,
            HotelName = "Hilton",
            HotelBookingId = Guid.NewGuid()
        });
        _logger.LogInformation("TripId: {TripId} Booking {RequiredStars} star hotel in {Location}", message.TripId, message.RequiredStars, message.Location);
    }
}