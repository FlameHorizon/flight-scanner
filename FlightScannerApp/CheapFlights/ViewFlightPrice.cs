using System;
using System.Collections.Generic;

namespace FlightScannerApp.CheapFlights;

public partial class ViewFlightPrice
{
    /// <summary>
    /// IATA airport code
    /// </summary>
    public string Origin { get; set; } = null!;

    /// <summary>
    /// IATA airport code
    /// </summary>
    public string Destination { get; set; } = null!;

    public int UpdateNo { get; set; }

    public DateTime? Day { get; set; }

    public decimal? Price { get; set; }

    public string? Currency { get; set; }
}
