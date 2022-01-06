using CSharpFunctionalExtensions;
using NServiceBus.Saga.Demo.Contracts.Flights;
using NServiceBus.Saga.Demo.Contracts.Hotels;
using NServiceBus.Saga.Demo.Contracts.Trips;

namespace NServiceBus.Saga.Demo.TripService.Domain
{
    public class Trip: IContainSagaData
    {
        /// <summary>
        ///
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Originator { get; set; } = null!;
        /// <summary>
        ///
        /// </summary>
        public string OriginalMessageId { get; set; } = null!;

        /// <summary>
        ///
        /// </summary>
        //public Guid CorrelationId { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string CurrentState { get; set; } = null!;

        /// <summary>
        ///
        /// </summary>
        public int RequiredStars { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public string Destination { get; private set; } = null!;

        /// <summary>
        ///
        /// </summary>
        public DateTime Start { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public DateTime End { get; private set; }

        private readonly List<FlightBooking> _bookedFlights = new();
        /// <summary>
        ///
        /// </summary>
        public IReadOnlyList<FlightBooking> BookedFlights => _bookedFlights.AsReadOnly();
        /// <summary>
        ///
        /// </summary>
        public bool HotelBooked => HotelBooking != null;
        /// <summary>
        ///
        /// </summary>
        public HotelBooking? HotelBooking { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public bool OutboundFlightBooked => _bookedFlights.Exists(_ => _.IsOutboundFlight);
        /// <summary>
        ///
        /// </summary>
        public bool ReturnFlightBooked => _bookedFlights.Exists(_ => _.IsReturnFlight);
        /// <summary>
        ///
        /// </summary>
        public bool AllFlightsBooked => OutboundFlightBooked && ReturnFlightBooked;
        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result Validate(SubmitTrip message) =>
            Result.Combine(
                //Result.FailureIf(message.Start >= message.End, "End date should be after start date"),
                Result.FailureIf(message.RequiredStars <= 0 || message.RequiredStars > 5, "Required stars should be between 1 and 5"),
                Result.FailureIf(string.IsNullOrWhiteSpace(message.Destination), "Destination is required"));
        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        public void Initialize(TripRegistrationRequest message)
        {
            RequiredStars = message.RequiredStars;
            Destination = message.Destination;
            Start = message.Start;
            End = message.End;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        public void Handle(FlightBooked message)
        {
            var booking = new FlightBooking(message.FlightId, message.IsOutbound, message.Cost, message.Company);
            _bookedFlights.Add(booking);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        public void Handle(HotelBooked message)
        {
            HotelBooking = new HotelBooking(message.HotelBookingId, message.Stars, message.HotelName);
        }
    }
}
