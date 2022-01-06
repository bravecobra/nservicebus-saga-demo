
using NServiceBus.Saga.Demo.Contracts.Flights;
using NServiceBus.Saga.Demo.Contracts.Hotels;
using NServiceBus.Saga.Demo.Contracts.Trips;
using NServiceBus.Saga.Demo.TripService.Domain;

namespace NServiceBus.Saga.Demo.TripService.Application
{
    public class TripPolicy: Saga<Trip>,
        IAmStartedByMessages<TripRegistrationRequest>,
        IHandleMessages<FlightBooked>,
        IHandleMessages<HotelBooked>
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
            mapper.MapSaga(data => data.Id)
                .ToMessage<TripRegistrationRequest>(request => request.TripId)
                .ToMessage<FlightBooked>(booked => booked.TripId)
                .ToMessage<HotelBooked>(booked => booked.TripId);
        }

        public async Task Handle(TripRegistrationRequest message, IMessageHandlerContext context)
        {
            Data.Initialize(message);
            _logger.LogInformation($"Trip {Data.Id}: Incoming TripRequest to {Data.Destination}");
            await context.Send(new BookFlightRequest
            {
                TripId = Data.Id,
                DayOfFlight = Data.Start,
                IsOutbound = true,
                From = "home",
                To = Data.Destination
            });

            await context.Send(new BookFlightRequest
            {
                TripId = Data.Id,
                DayOfFlight = Data.End,
                IsOutbound = false,
                From = Data.Destination,
                To = "home"
            });
            Data.CurrentState = "PendingFlightBookingConfirmations";
            _logger.LogInformation($"Trip {Data.Id}: Flights are requested");
        }

        public async Task Handle(FlightBooked message, IMessageHandlerContext context)
        {
            Data.Handle(message);
            _logger.LogInformation($"Trip {Data.Id}: {(message.IsOutbound ? "Outbound" : "Inbound") } flight got booked @ {message.Company}");
            if (Data.AllFlightsBooked)
            {
                await context.Send(new BookHotelRequest
                {
                    TripId = Data.Id,
                    RequiredStars = Data.RequiredStars,
                    Location = Data.Destination

                });
                Data.CurrentState = "PendingHotelBookingConfirmation";
                _logger.LogInformation($"Trip {Data.Id}: Hotel booking in {Data.Destination} requested");
            }
            _logger.LogInformation($"Trip {Data.Id}: CurrentState = {Data.CurrentState}");
            
        }

        public Task Handle(HotelBooked message, IMessageHandlerContext context)
        {
            Data.Handle(message);
            Data.CurrentState = "Completed";
            _logger.LogInformation($"Trip {Data.Id}: Hotel booked in {Data.Destination}");
            _logger.LogInformation($"Trip {Data.Id}: CurrentState = {Data.CurrentState}");
            return Task.CompletedTask;
        }
    }
}
