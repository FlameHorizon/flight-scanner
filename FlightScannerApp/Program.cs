// Replace the URL with the actual API endpoint you want to call

using MySqlConnector;

int updateNo = await GetLastUpdateNo();
const string connectionString = "server=mysql;database=CheapFlights;user=root;password=pass;";

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
    const string query = "SELECT MAX(UpdateNo) FROM OneWayFares";
    await using var cn = new MySqlConnection(connectionString);
    await cn.OpenAsync();
    await using var cmd = new MySqlCommand(query, cn);

    int result = cmd.ExecuteScalar() as int? ?? 0;

    await cn.CloseAsync();
    return result;
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
    await using var cn = new MySqlConnection(connectionString);
    await cn.OpenAsync();

    const string query = "INSERT " +
                         "INTO OneWayFares (Origin, Destination, Content, UpdateNo) " +
                         "VALUES (@origin, @destination, @json, @updateNo)";

    await using var cmd = new MySqlCommand(query, cn);

    cmd.Parameters.AddWithValue("@origin", origin);
    cmd.Parameters.AddWithValue("@destination", destination);
    cmd.Parameters.AddWithValue("@json", jsonResponse);
    cmd.Parameters.AddWithValue("@updateNo", updateNo);

    cmd.ExecuteNonQuery();
    await cn.CloseAsync();
}