using System;
using System.Collections.Generic;

namespace FlightScannerApp.CheapFlights;

public partial class OneWayFare
{
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// IATA airport code
    /// </summary>
    public string Origin { get; set; } = null!;

    /// <summary>
    /// IATA airport code
    /// </summary>
    public string Destination { get; set; } = null!;

    /// <summary>
    /// Response from Ryanair API
    /// </summary>
    public string Content { get; set; } = null!;

    public int UpdateNo { get; set; }
}
