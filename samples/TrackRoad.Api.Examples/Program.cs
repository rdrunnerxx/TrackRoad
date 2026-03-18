using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

Console.WriteLine("TrackRoad API Examples");
Console.WriteLine("======================");
Console.WriteLine();
Console.WriteLine("This sample shows the most common TrackRoad Dispatch scenarios:");
Console.WriteLine("1. Single-vehicle route optimization using latitude/longitude");
Console.WriteLine("2. Route optimization with time windows");
Console.WriteLine("3. Route optimization without vehicles (stops only)");
Console.WriteLine();
Console.WriteLine("Set your API key before running:");
Console.WriteLine("    var apiKey = \"YOUR_API_KEY\";");
Console.WriteLine();

await RunDispatchExampleAsync();
Console.WriteLine();
await RunDispatchExampleTimeWindowAsync();
Console.WriteLine();
await RunDispatchStopsOnlyExampleAsync();
Console.ReadKey();

static void PrintHeader(string title)
{
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

static void PrintResult(DispatchResult result)
{
    if (result == null)
    {
        Console.WriteLine("No result returned.");
        return;
    }

    Console.WriteLine($"Status: {result.Status}");

    if (result.Errors != null && result.Errors.Count > 0)
    {
        Console.WriteLine("Errors:");
        foreach (var error in result.Errors)
            Console.WriteLine($"  - {error.Message}");
    }

    if (result.Items == null || result.Items.Count == 0)
    {
        Console.WriteLine("No vehicle routes returned.");
        return;
    }

    var vehicleIndex = 1;
    foreach (var item in result.Items)
    {
        var vehicleName = string.IsNullOrWhiteSpace(item.Vehicle?.Name)
            ? $"Route {vehicleIndex}"
            : item.Vehicle.Name;

        Console.WriteLine();
        Console.WriteLine($"Vehicle/Route: {vehicleName}");

        if (item.Locations == null || item.Locations.Count == 0)
        {
            Console.WriteLine("  No assigned stops.");
            vehicleIndex++;
            continue;
        }

        var stopIndex = 1;
        foreach (var location in item.Locations)
        {
            var travelMinutes = location.Time.GetValueOrDefault()/60.0;
            var waitMinutes = location.Wait.GetValueOrDefault();
            var eta = location.TimeEstimatedArrival;
            var distance = location.Distance.GetValueOrDefault();
            var matchCode = location.MatchCode?.ToString() ?? "n/a";
            var type = location.LocationType?.ToString() ?? "n/a";

            Console.WriteLine(
                $"  {stopIndex,2}. {location.Name,-12} " +
                $"Type: {type,-10} " +
                $"ETA: {(eta.HasValue ? eta.Value.ToString("HH:mm") : "n/a"),-5} " +
                $"Distance: {distance,7:F2} mi " +
                $"Travel: {travelMinutes,6:f2} minutes " +
                $"Wait: {waitMinutes,6} minutes " +
                $"Match: {matchCode}");

            stopIndex++;
        }
    }
}

static async Task RunDispatchExampleAsync()
{
    PrintHeader("Example 1: Dispatch with one vehicle and lat/long stops");

    using var httpClient = new HttpClient();
    var client = new TrackRoadClient(httpClient);
    var apiKey = "YOUR_API_KEY";

    // This is the most common starting point.
    // One vehicle is supplied, so Dispatch acts like a route optimizer for that vehicle.
    var specification = new DispatchSpecification
    {
        RoutingService = RoutingService.NetRoad,
        IsNeedMatchCode = true,
        CurrentTime = DateTime.UtcNow,
        DispatchMode = DispatchMode.Auto,
        MinimumOptimization = 4,
        DistanceUnit = DistanceUnit.Mile,
        RouteOptimize = RouteOptimize.MinimizeTime,
        Vehicles = new[]
        {
            new Vehicle
            {
                Name = "Truck 1",
                Speed = 45,
                MaxStops = 10,
                MaxWeight = 5000,
                StartLocation = new Location
                {
                    Name = "Warehouse",
                    LocationType = LocationType.Start, // optional
                    LatLong = new LatLong
                    {
                        Latitude = 38.917435,
                        Longitude = -77.226263
                    }
                },
                FinishLocation = new Location
                {
                    Name = "Warehouse",
                    LocationType = LocationType.Finish, // optional
                    LatLong = new LatLong
                    {
                        Latitude = 38.917435,
                        Longitude = -77.226263
                    }
                }
            }
        },
        Locations = new[]
        {
             new Location
            {
                Name = "A",
                LocationType = LocationType.Midway, // optional
                LatLong = new LatLong
                {
                    Latitude = 38.93415,
                    Longitude = -77.182173
                },
                Weight = 90,
                Volume = 8,
                Wait = 180
            },
            new Location
            {
                Name = "B",
                LocationType = LocationType.Midway,
                LatLong = new LatLong
                {
                    Latitude = 38.933802,
                    Longitude = -77.1794
                },
                Weight = 120,
                Volume = 10,
                Wait = 300
            },           
            new Location
            {
                Name = "C",
                LocationType = LocationType.Midway,
                LatLong = new LatLong
                {
                    Latitude = 38.929695,
                    Longitude = -77.22713
                },
                Weight = 20,
                Volume = 1,
                Wait = 10
            }
        }
    };

    var result = await client.DispatchAsync(specification, apiKey);
    PrintResult(result);
}

static async Task RunDispatchExampleTimeWindowAsync()
{
    PrintHeader("Example 2: Dispatch with time windows");

    using var httpClient = new HttpClient();
    var client = new TrackRoadClient(httpClient);
    var apiKey = "YOUR_API_KEY";
    var now = DateTime.UtcNow;

    // Time windows are useful for delivery appointments and service calls.
    // Tin/Tout define driver availability.
    // TimeConstraintArrival/Departure define stop service windows.
    var specification = new DispatchSpecification
    {
        RoutingService = RoutingService.NetRoad,
        IsNeedMatchCode = true,
        CurrentTime = now,
        DispatchMode = DispatchMode.Auto,
        MinimumOptimization = 4,
        DistanceUnit = DistanceUnit.Mile,
        RouteOptimize = RouteOptimize.MinimizeTime,
        Vehicles = new[]
        {
            new Vehicle
            {
                Name = "Truck 1",
                Speed = 45,
                MaxStops = 10,
                MaxWeight = 5000,
                Tin = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0),
                Tout = new DateTime(now.Year, now.Month, now.Day, 18, 0, 0),
                StartLocation = new Location
                {
                    Name = "Warehouse",
                    LocationType = LocationType.Start,
                    LatLong = new LatLong
                    {
                        Latitude = 38.917435,
                        Longitude = -77.226263
                    }
                },
                FinishLocation = new Location
                {
                    Name = "Warehouse",
                    LocationType = LocationType.Finish,
                    LatLong = new LatLong
                    {
                        Latitude = 38.917435,
                        Longitude = -77.226263
                    }
                }
            }
        },
        Locations = new[]
        {
            new Location
            {
                Name = "A",
                LocationType = LocationType.Midway,
                LatLong = new LatLong
                {
                    Latitude = 38.933802,
                    Longitude = -77.1794
                },
                Weight = 120,
                Volume = 10,
                Wait = 30,
                TimeConstraintArrival = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0),
                TimeConstraintDeparture = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0),
                CanArriveEarly = true
            },
            new Location
            {
                Name = "B",
                LocationType = LocationType.Midway,
                LatLong = new LatLong
                {
                    Latitude = 38.929695,
                    Longitude = -77.22713
                },
                Weight = 20,
                Volume = 1,
                Wait = 10,
                TimeConstraintArrival = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0),
                TimeConstraintDeparture = new DateTime(now.Year, now.Month, now.Day, 19, 0, 0),
                CanArriveEarly = true
            },
            new Location
            {
                Name = "C",
                LocationType = LocationType.Midway,
                LatLong = new LatLong
                {
                    Latitude = 38.93415,
                    Longitude = -77.182173
                },
                Weight = 90,
                Volume = 8,
                Wait = 50,
                TimeConstraintArrival = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0),
                TimeConstraintDeparture = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0),
                CanArriveEarly = true
            }            
        }
    };

    var result = await client.DispatchAsync(specification, apiKey);
    PrintResult(result);
}

static async Task RunDispatchStopsOnlyExampleAsync()
{
    PrintHeader("Example 3: Dispatch without vehicles (stops only optimization)");

    using var httpClient = new HttpClient();
    var client = new TrackRoadClient(httpClient);
    var apiKey = "YOUR_API_KEY";

    // Vehicles are optional.
    // You can send only stops and let TrackRoad optimize the stop sequence.
    // Start/finish can be expressed through LocationType on the stops themselves.
    var specification = new DispatchSpecification
    {
        RoutingService = RoutingService.NetRoad,
        IsNeedMatchCode = true,
        CurrentTime = DateTime.UtcNow,
        DispatchMode = DispatchMode.Optima,
        DistanceUnit = DistanceUnit.Mile,
        RouteOptimize = RouteOptimize.MinimizeTime,
        Locations = new[]
        {
            new Location
            {
                Name = "Start",
                LocationType = LocationType.Start, // optional
                LatLong = new LatLong
                {
                    Latitude = 38.917435,
                    Longitude = -77.226263
                }
            },
            new Location
            {
                Name = "Stop A",
                LocationType = LocationType.Midway, // optional
                LatLong = new LatLong
                {
                    Latitude = 38.933802,
                    Longitude = -77.1794
                },
                Wait = 120
            },
            new Location
            {
                Name = "Stop B",
                LocationType = LocationType.Midway, // optional
                LatLong = new LatLong
                {
                    Latitude = 38.93415,
                    Longitude = -77.182173
                },
                Wait = 120
            },
            new Location
            {
                Name = "Stop C",
                LocationType = LocationType.Midway, // optional
                LatLong = new LatLong
                {
                    Latitude = 38.929695,
                    Longitude = -77.22713
                },
                Wait = 120
            },
            new Location
            {
                Name = "Finish",
                LocationType = LocationType.Finish, // optional
                LatLong = new LatLong
                {
                    Latitude = 38.917435,
                    Longitude = -77.226263
                }
            }
        }
    };

    var result = await client.DispatchAsync(specification, apiKey);
    PrintResult(result);

    Console.WriteLine();
    Console.WriteLine("Notes:");
    Console.WriteLine("- This pattern is useful when you only need best stop order.");
    Console.WriteLine("- If you omit both Start and Finish, TrackRoad can optimize the route freely.");
    Console.WriteLine("- If you provide Start and/or Finish as stops, TrackRoad uses them as route anchors.");
}
