using System;
using System.Collections.Generic;

namespace FlightScannerApp.CheapFlights;

/// <summary>
/// Stores all available pairs of flights. Date difference between departure and arrival is maximum 14 days.
/// </summary>
public partial class FaresPair
{
    /// <summary>
    /// Represents date time when record was created
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// IATA airport code from which flight going to happen
    /// </summary>
    public string Origin { get; set; } = null!;

    /// <summary>
    /// IATA airport code where arrival is
    /// </summary>
    public string Destination { get; set; } = null!;

    /// <summary>
    /// Price of the flight from the origin point of view
    /// </summary>
    public decimal OriginPrice { get; set; }

    /// <summary>
    /// Currency in which price is from origin
    /// </summary>
    public string OriginCurrency { get; set; } = null!;

    /// <summary>
    /// Date of the departure
    /// </summary>
    public DateTime OriginDate { get; set; }

    /// <summary>
    /// Price of the flight from the destination point of view
    /// </summary>
    public decimal DestinationPrice { get; set; }

    /// <summary>
    /// Currency in which price is from destination
    /// </summary>
    public string DestinationCurrency { get; set; } = null!;

    /// <summary>
    /// Date of the flight back
    /// </summary>
    public DateTime DestinationDate { get; set; }

    /// <summary>
    /// Indicate batch number in which record was created
    /// </summary>
    public int UpdateNo { get; set; }

    public int Id { get; set; }
}
