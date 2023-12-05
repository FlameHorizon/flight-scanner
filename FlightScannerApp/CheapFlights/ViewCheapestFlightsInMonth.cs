using System;
using System.Collections.Generic;

namespace FlightScannerApp.CheapFlights;

public partial class ViewCheapestFlightsInMonth
{
    public int? Year { get; set; }

    public int? Month { get; set; }

    /// <summary>
    /// IATA airport code
    /// </summary>
    public string Origin { get; set; } = null!;

    /// <summary>
    /// IATA airport code
    /// </summary>
    public string Destination { get; set; } = null!;

    public decimal? CheapestPrice { get; set; }

    public string? Currency { get; set; }

    public DateTime? Date { get; set; }

    public int? UpdateNo { get; set; }
}
