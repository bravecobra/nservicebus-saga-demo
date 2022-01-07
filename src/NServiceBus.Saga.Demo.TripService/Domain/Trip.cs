using CSharpFunctionalExtensions;
using NServiceBus.Saga.Demo.Contracts.Flights;
using NServiceBus.Saga.Demo.Contracts.Hotels;
using NServiceBus.Saga.Demo.Contracts.Trips;

namespace NServiceBus.Saga.Demo.TripService.Domain;

public class Trip
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public int RequiredStars { get; private set; }
    public string Destination { get; private set; } = null!;
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    private readonly List<FlightBooking> _bookedFlights = new();
    public IReadOnlyList<FlightBooking> BookedFlights => _bookedFlights.AsReadOnly();
    public bool HotelBooked => HotelBooking != null;
    public HotelBooking? HotelBooking { get; private set; }
    public bool OutboundFlightBooked => _bookedFlights.Exists(_ => _.IsOutboundFlight);
    public bool ReturnFlightBooked => _bookedFlights.Exists(_ => _.IsReturnFlight);
    public bool AllFlightsBooked => OutboundFlightBooked && ReturnFlightBooked;
    
    public static Result Validate(SubmitTrip message) =>
        Result.Combine(
            Result.FailureIf(message.Start >= message.End,
                "End date should be after start date"),
            Result.FailureIf(message.RequiredStars <= 0 || message.RequiredStars > 5,
                "Required stars should be between 1 and 5"),
            Result.FailureIf(string.IsNullOrWhiteSpace(message.Destination),
                "Destination is required"));

    public void Initialize(TripRegistrationRequest message)
    {
        RequiredStars = message.RequiredStars;
        Destination = message.Destination;
        Start = message.Start;
        End = message.End;
    }

    public void Handle(FlightBooked message)
    {
        var booking = new FlightBooking(message.FlightId, message.IsOutbound, message.Cost, message.Company);
        _bookedFlights.Add(booking);
    }

    public void Handle(HotelBooked message)
    {
        HotelBooking = new HotelBooking(message.HotelBookingId, message.Stars, message.HotelName);
    }
}