using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NServiceBus.Saga.Demo.Contracts.Flights;
using NServiceBus.Saga.Demo.Contracts.Hotels;
using NServiceBus.Saga.Demo.Contracts.Trips;
using NServiceBus.Saga.Demo.TripService.Domain;
using NServiceBus.Saga.Demo.TripService.Persistence;
using NServiceBus.Sagas;

namespace NServiceBus.Saga.Demo.TripService.Application;

public class TripPolicy: Saga<TripPolicy.TripSagaData>,
        IAmStartedByMessages<TripRegistrationRequest>,
        IHandleMessages<FlightBooked>,
        IHandleMessages<HotelBooked>,
        IHandleMessages<TripStateRequest>,
        IHandleMessages<TripCancellationRequest>,
        IHandleSagaNotFound
{
    private readonly ILogger<TripPolicy> _logger;
    private readonly TripDbContext _dbContext;

    public TripPolicy(ILogger<TripPolicy> logger, TripDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    public class TripSagaData : ContainSagaData
    {
        public Guid TripId { get; set; }
    }

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TripSagaData> mapper)
    {
        mapper.MapSaga(data => data.TripId)
            .ToMessage<TripRegistrationRequest>(request => request.TripId)
            .ToMessage<FlightBooked>(booked => booked.TripId)
            .ToMessage<HotelBooked>(booked => booked.TripId)
            .ToMessage<TripStateRequest>(request => request.TripId)
            .ToMessage<TripCancellationRequest>(request => request.TripId);
    }

    public async Task Handle(TripRegistrationRequest message, IMessageHandlerContext context)
    {
        var trip = new Trip
        {
            CorrelationId = Data.TripId
        };
        trip.Initialize(message);
        _logger.LogInformation("TripID: {TripId}", message.TripId);
        _logger.LogInformation("Trip {CorrelationId}: Incoming TripRequest to {Destination}", trip.CorrelationId, trip.Destination);
        await context.Send(new BookFlightRequest
        {
            TripId = trip.CorrelationId,
            DayOfFlight = trip.Start,
            IsOutbound = true,
            From = "home",
            To = trip.Destination
        });
        
        await context.Send(new BookFlightRequest
        {
            TripId = trip.CorrelationId,
            DayOfFlight = trip.End,
            IsOutbound = false,
            From = trip.Destination,
            To = "home"
        });
        trip.CurrentState = "PendingFlightBookingConfirmations";
        _logger.LogInformation("Trip {CorrelationId}: Flights are requested", trip.CorrelationId);
        await _dbContext.TripStates.AddAsync(trip);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Handle(FlightBooked message, IMessageHandlerContext context)
    {
        var trip = _dbContext.TripStates
            .Include(t => t.BookedFlights)
            .Include(t=> t.HotelBooking)
            .Single(trip => trip.CorrelationId == Data.TripId);
        if (trip.CurrentState == "Cancelled")
        {
            _logger.LogWarning("TripId {CorrelationId}: trip was already cancelled. Ignoring flight booking", trip.CorrelationId);
        }

        trip.Handle(message);
        _logger.LogInformation("Trip {CorrelationId}: {InOutBound } flight got booked @ {Company}", trip.CorrelationId, message.IsOutbound ? "Outbound" : "Inbound", message.Company);
        if (trip.AllFlightsBooked)
        {
            await context.Send(new BookHotelRequest
            {
                TripId = trip.CorrelationId,
                RequiredStars = trip.RequiredStars,
                Location = trip.Destination

            });
            trip.CurrentState = "PendingHotelBookingConfirmation";
            _logger.LogInformation("Trip {CorrelationId}: Hotel booking in {Destination} requested", trip.CorrelationId, trip.Destination);
        }
        _logger.LogInformation("Trip {CorrelationId}: CurrentState = {CurrentState}", trip.CorrelationId, trip.CurrentState);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Handle(HotelBooked message, IMessageHandlerContext context)
    {
        var trip = _dbContext.TripStates
            .Include(t => t.BookedFlights)
            .Include(t => t.HotelBooking)
            .Single(trip => trip.CorrelationId == Data.TripId);
        if (trip.CurrentState == "Cancelled")
        {
            _logger.LogWarning("TripId {CorrelationId}: trip was already cancelled. Ignoring flight booking", trip.CorrelationId);
        }
        trip.Handle(message);
        trip.CurrentState = "Completed";
        _logger.LogInformation("Trip {CorrelationId}: Hotel booked in {Destination}", trip.CorrelationId, trip.Destination);
        _logger.LogInformation("Trip {CorrelationId}: CurrentState = {CurrentState}", trip.CorrelationId, trip.CurrentState);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Handle(TripStateRequest message, IMessageHandlerContext context)
    {
        var trip = _dbContext.TripStates
            .Include(t => t.BookedFlights)
            .Include(t => t.HotelBooking)
            .Single(trip => trip.CorrelationId == Data.TripId);
        _logger.LogInformation("TripId: {CorrelationId} Replying to TripStateRequest", trip.CorrelationId);
        await context.Reply(new TripState
        {
            TripId = trip.CorrelationId,
            HotelBooked = trip.HotelBooked,
            OutboundFlightBooked = trip.OutboundFlightBooked,
            ReturnFlightBooked = trip.ReturnFlightBooked,
            State = trip.CurrentState,
            Timestamp = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task Handle(object message, IMessageProcessingContext context)
    {
        if (message is TripStateRequest)
        {
            await context.Reply(new TripState());
        }
        else
        {
            _logger.LogError("Saga not found: {message}", JsonConvert.SerializeObject(message));
        }
    }

    public async Task Handle(TripCancellationRequest message, IMessageHandlerContext context)
    {
        var trip = _dbContext.TripStates
            .Include(t => t.BookedFlights)
            .Include(t => t.HotelBooking)
            .Single(trip => trip.CorrelationId == Data.TripId);
        _logger.LogWarning("TODO: handle a TripCancellationRequest");
        trip.CurrentState = "Cancelled";
        await _dbContext.SaveChangesAsync();
    }
}