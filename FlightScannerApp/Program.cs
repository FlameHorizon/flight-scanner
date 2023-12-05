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
            await UpdateFaresMap();
        }
    }
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
    return await new CheapFlightsContext().OneWayFares.MaxAsync(f => f.UpdateNo);
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
}

async Task UpdateFaresMap()
{
    var ctx = new CheapFlightsContext();
    int maxUpdateNo = await ctx.ViewFlightPrices.MaxAsync(f => f.UpdateNo);

    List<ViewFlightPrice> flightsFromWarsaw = ctx.ViewFlightPrices
        .Where(f => f.Origin == "WAW" &&
                    f.UpdateNo == maxUpdateNo).ToList();

    foreach (ViewFlightPrice flight in flightsFromWarsaw)
    {
        // Get flights to Warsaw after flight.Day
        List<ViewFlightPrice> futureFlights = ctx.ViewFlightPrices
            .Where(f => f.Destination == "WAW" &&
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
            ctx.SaveChanges();
        }
    }
}