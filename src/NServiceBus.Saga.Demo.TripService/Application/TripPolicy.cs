using Newtonsoft.Json;
using NServiceBus.Saga.Demo.Contracts.Flights;
using NServiceBus.Saga.Demo.Contracts.Hotels;
using NServiceBus.Saga.Demo.Contracts.Trips;
using NServiceBus.Saga.Demo.TripService.Domain;
using NServiceBus.Sagas;

namespace NServiceBus.Saga.Demo.TripService.Application;

public class TripPolicy: Saga<Trip>,
        IAmStartedByMessages<TripRegistrationRequest>,
        IHandleMessages<FlightBooked>,
        IHandleMessages<HotelBooked>,
        IHandleMessages<TripStateRequest>,
        IHandleMessages<TripCancellationRequest>,
        IHandleSagaNotFound
{
    private readonly ILogger<TripPolicy> _logger;

    public TripPolicy(ILogger<TripPolicy> logger)
    {
        _logger = logger;
    }
    // public class TripData: ContainSagaData
    // {
    //     public Guid TripId { get; set; }
    // }

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<Trip> mapper)
    {
        mapper.MapSaga(data => data.CorrelationId)
            .ToMessage<TripRegistrationRequest>(request => request.TripId)
            .ToMessage<FlightBooked>(booked => booked.TripId)
            .ToMessage<HotelBooked>(booked => booked.TripId)
            .ToMessage<TripStateRequest>(request => request.TripId)
            .ToMessage<TripCancellationRequest>(request => request.TripId);
    }

    public async Task Handle(TripRegistrationRequest message, IMessageHandlerContext context)
    {
        Data.Initialize(message);
        _logger.LogInformation($"TripID: {message.TripId}");
        _logger.LogInformation($"Trip {Data.CorrelationId}: Incoming TripRequest to {Data.Destination}");
        await context.Send(new BookFlightRequest
        {
            TripId = Data.CorrelationId,
            DayOfFlight = Data.Start,
            IsOutbound = true,
            From = "home",
            To = Data.Destination
        });
        
        await context.Send(new BookFlightRequest
        {
            TripId = Data.CorrelationId,
            DayOfFlight = Data.End,
            IsOutbound = false,
            From = Data.Destination,
            To = "home"
        });
        Data.CurrentState = "PendingFlightBookingConfirmations";
        _logger.LogInformation($"Trip {Data.CorrelationId}: Flights are requested");
    }

    public async Task Handle(FlightBooked message, IMessageHandlerContext context)
    {
        if (Data.CurrentState == "Cancelled")
        {
            _logger.LogWarning($"TripId {Data.CorrelationId}: trip was already cancelled. Ignoring flight booking");
        }

        Data.Handle(message);
        _logger.LogInformation($"Trip {Data.CorrelationId}: {(message.IsOutbound ? "Outbound" : "Inbound") } flight got booked @ {message.Company}");
        if (Data.AllFlightsBooked)
        {
            await context.Send(new BookHotelRequest
            {
                TripId = Data.CorrelationId,
                RequiredStars = Data.RequiredStars,
                Location = Data.Destination

            });
            Data.CurrentState = "PendingHotelBookingConfirmation";
            _logger.LogInformation($"Trip {Data.CorrelationId}: Hotel booking in {Data.Destination} requested");
        }
        _logger.LogInformation($"Trip {Data.CorrelationId}: CurrentState = {Data.CurrentState}");
            
    }

    public Task Handle(HotelBooked message, IMessageHandlerContext context)
    {
        if (Data.CurrentState == "Cancelled")
        {
            _logger.LogWarning($"TripId {Data.CorrelationId}: trip was already cancelled. Ignoring flight booking");
        }
        Data.Handle(message);
        Data.CurrentState = "Completed";
        _logger.LogInformation($"Trip {Data.CorrelationId}: Hotel booked in {Data.Destination}");
        _logger.LogInformation($"Trip {Data.CorrelationId}: CurrentState = {Data.CurrentState}");
        return Task.CompletedTask;
    }

    public async Task Handle(TripStateRequest message, IMessageHandlerContext context)
    {
        _logger.LogInformation($"TripId: {Data.CorrelationId} Replying to TripStateRequest");
        await context.Reply(new TripState
        {
            TripId = Data.CorrelationId,
            HotelBooked = Data.HotelBooked,
            OutboundFlightBooked = Data.OutboundFlightBooked,
            ReturnFlightBooked = Data.ReturnFlightBooked,
            State = Data.CurrentState,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task Handle(object message, IMessageProcessingContext context)
    {
        if (message is TripStateRequest request)
        {
            await context.Reply(new TripState());
            //await context.Reply(new TripNotFound { TripId = request.TripId });
        }
        else
        {
            _logger.LogError($"Saga not found: {JsonConvert.SerializeObject(message)}");
        }
    }

    public Task Handle(TripCancellationRequest message, IMessageHandlerContext context)
    {
        _logger.LogWarning("TODO: handle a TripCancellationRequest");
        Data.CurrentState = "Cancelled";
        return Task.CompletedTask;
    }
}