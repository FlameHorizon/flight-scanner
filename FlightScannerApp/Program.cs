using FlightScannerApp.CheapFlights;
using Microsoft.EntityFrameworkCore;

int updateNo = await GetLastUpdateNo();

while (true)
{
    try
    {
        updateNo++;
        await StartJob();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }

    Console.WriteLine("Waiting 6 hours...");
    await Task.Delay(TimeSpan.FromHours(6));
}

async Task StartJob()
{
    Console.WriteLine($"Starting update {updateNo}");
    foreach ((string origin, string destination) in GetFlightsToScan())
    {
        Console.WriteLine($"Scanning {origin} to {destination}");
        // Look ahead for next 6 months
        for (var i = 0; i < 6; i++)
        {
            var start = DateTime.UtcNow.AddMonths(i).ToString("yyyy-MM-dd");
            await FetchFlightPrices(origin, destination, start);
            await FetchFlightPrices(destination, origin, start);
        }
    }

    await UpdateFaresMap();
}

List<(string, string)> GetFlightsToScan()
{
    return File.ReadLines("flights.txt")
        .Select(line => line.Split(','))
        .Select(line => (line[0].Trim(), line[1].Trim()))
        .ToList();
}

static async Task<int> GetLastUpdateNo()
{
    // The only way we can get InvalidOperationException is when table
    // OneWayFares is empty. When this does happen, we will return 0 
    // to properly initialize first value.
    var ctx = new CheapFlightsContext();
    if (await ctx.OneWayFares.AnyAsync())
    {
        return await ctx.OneWayFares.MaxAsync(f => f.UpdateNo);
    }

    return 0;
}

async Task FetchFlightPrices(string origin, string destination, string outboundMonthOfDate)
{
    string apiUrl = $"https://services-api.ryanair.com/farfnd/3/oneWayFares/" +
                    $"{origin}/{destination}/cheapestPerDay?" +
                    $"outboundMonthOfDate={outboundMonthOfDate}";
    string jsonResponse = await GetApiResponse(apiUrl);

    await StoreInDatabase(origin, destination, jsonResponse);
    Console.WriteLine("Database updated at " + DateTime.Now);
}

static async Task<string> GetApiResponse(string apiUrl)
{
    using var client = new HttpClient();
    HttpResponseMessage response = await client.GetAsync(apiUrl);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
}

async Task StoreInDatabase(string origin, string destination, string jsonResponse)
{
    var ctx = new CheapFlightsContext();
    await ctx.OneWayFares.AddAsync(new OneWayFare
    {
        Origin = origin,
        Destination = destination,
        Content = jsonResponse,
        UpdateNo = updateNo
    });
    await ctx.SaveChangesAsync();
}

async Task UpdateFaresMap()
{
    var ctx = new CheapFlightsContext();
    int maxUpdateNo = await ctx.ViewFlightPrices.MaxAsync(f => f.UpdateNo);

    List<ViewFlightPrice> flightsFromWarsaw = ctx.ViewFlightPrices
        .Where(f =>f.UpdateNo == maxUpdateNo).ToList();

    foreach (ViewFlightPrice flight in flightsFromWarsaw)
    {
        List<ViewFlightPrice> futureFlights = ctx.ViewFlightPrices
            .Where(f => f.Destination == f.Origin &&
                        f.Origin == flight.Destination &&
                        f.Day.Value.Date > flight.Day.Value.Date &&
                        f.Day.Value.Date <= flight.Day.Value.AddDays(14) &&
                        f.UpdateNo == maxUpdateNo)
            .ToList();

        foreach (ViewFlightPrice futureFlight in futureFlights)
        {
            await ctx.FaresPairs.AddAsync(new FaresPair
            {
                Origin = flight.Origin,
                Destination = flight.Destination,
                OriginPrice = flight.Price.Value,
                OriginCurrency = flight.Currency,
                OriginDate = flight.Day.Value,
                DestinationPrice = futureFlight.Price.Value,
                DestinationCurrency = futureFlight.Currency,
                DestinationDate = futureFlight.Day.Value,
                UpdateNo = flight.UpdateNo
            });
        }
    }
    
    ctx.SaveChanges();
}