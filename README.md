# TrackRoad.Api.Client

A .NET client library for the TrackRoad REST JSON API.

TrackRoad API is a routing and logistics API for developers who need route optimization, multi-vehicle matrix dispatching, geocoding, routing, and ETA or distance calculations in their own applications.

TrackRoad provides both REST and SOAP APIs. This NuGet package wraps the REST API and gives you strongly typed .NET models for the most common route optimization workflows.

Try TrackRoad route optimization software online: https://trackroad.com/desktop/routing/ 

For more information: https://trackroad.com/

This package provides a typed `HttpClient`-based client for these TrackRoad API operations:

- Dispatch
- Distance
- Geocode
- Credit
- Route
- Routes

## What is TrackRoad API?

TrackRoad API is useful for:

- delivery routing
- courier and parcel operations
- field service scheduling
- fleet dispatching
- service businesses with multiple stops per driver
- systems that need address geocoding before route planning

In most real-world integrations, the most important API method is **Dispatch**.

Dispatch is the core optimization operation in TrackRoad. It can:

- optimize a list of stops with no vehicles at all
- optimize a route for one vehicle
- distribute stops across multiple vehicles
- distribute load across multiple vehicles based on capacity and time windows
- return optimized stop order, ETA, distance, and duration

If you already know your stop coordinates, send `LatLong` values whenever possible. If coordinates are not available, TrackRoad can geocode addresses and use those coordinates for routing.

## Supported framework

- `netstandard2.0`

This means it can be used from:

- .NET Framework 4.6.1+
- .NET Core 2.0+
- .NET 5+
- .NET 6+
- .NET 7+
- .NET 8+

## NuGet package

Install the package:

```bash
dotnet add package TrackRoad.Api.Client
```

Or from Visual Studio:

- Right click project
- Manage NuGet Packages
- Search for `TrackRoad.Api.Client`
- Install

## Package dependencies

This package depends on:

- `Newtonsoft.Json`

## Base URL

Default base URL used by the client:

```text
https://ts6.trackroad.com/
```

You can override it after creating the client:

```csharp
client.BaseUrl = "https://ts6.trackroad.com/";
```

## Authentication

Most methods require an API key passed in the `X-API-Key` header.

You can obtain your `X-API-Key` from your TrackRoad account.

In this client, you pass the API key as a method argument:

```csharp
var apiKey = "YOUR_API_KEY";
```

## Quick start

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class Example
{
    public async Task RunAsync()
    {
        using var httpClient = new HttpClient();

        var client = new TrackRoadClient(httpClient);
        var apiKey = "YOUR_API_KEY";

        var result = await client.GetCreditAsync(apiKey);

        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"Credit: {result.Credit}");
    }
}
```

## Why Dispatch is the main API method

For most TrackRoad integrations, `DispatchAsync` is the main API operation.

Use `DispatchAsync` when you need to:

- optimize stop sequence
- distribute stops across multiple vehicles
- respect time windows
- consider weight, volume, skids, maximum stops, or maximum route time
- use start and finish locations
- calculate practical routes for delivery, service, or field operations

A simple way to think about the TrackRoad API is:

- **Dispatch** = optimize and assign stops across zero, one, or many vehicles
- **Route** = calculate one route through a given stop list
- **Routes** = calculate multiple separate routes
- **Geocode** = convert addresses to coordinates
- **Distance** = calculate travel distance and time between two locations
- **Credit** = check available API credit

## API methods

## 1. DispatchAsync

Creates dispatch results for vehicles and locations.

Use Dispatch to optimize routes across many stops. Dispatch returns an ordered route with timing, distance, and assignment output for delivery routing, field service, courier operations, and fleet planning.

### What Dispatch does

Dispatch solves route optimization and dispatching problems.

It supports these common scenarios:

- **Stops only, no vehicles**  
  You can send just a list of stops and let TrackRoad find the best optimized route.

- **Single vehicle route optimization**  
  You can send one vehicle and many stops to get an optimized route for that vehicle.

- **Multi-vehicle dispatch optimization**  
  You can send multiple vehicles and many stops to distribute stops across vehicles and optimize each resulting route.

### Vehicles are optional

Vehicles are optional.

If you do not provide vehicles, Dispatch can still optimize the stop order from the stop list alone.

You can also use `LocationType` values such as:

- `Start`
- `Finish`
- `Midway`

This allows you to define:

- a fixed start location
- a fixed finish location
- both start and finish
- neither start nor finish, so the optimizer finds the best open route

### Location input

`LatLong` is preferred.

Each stop should include `LatLong` whenever possible. If `LatLong` is not provided, you should provide `Address` so the service can geocode the stop.

Each location can contain:

- name
- address fields
- latitude and longitude
- time windows
- service time (`Wait`)
- capacity-related values such as weight, volume, and skids

### When to use Dispatch vs Route

Use **Dispatch** when:

- you want optimization
- you want vehicle assignment
- you want best stop sequence
- you want to handle multiple stops with route logic

Use **Route** when:

- you already know the stop order
- you only want route geometry, itinerary, distance, and time for that exact order

### Common Dispatch use cases

Use Dispatch when you need to solve problems such as:

- assign daily deliveries across multiple trucks
- optimize technician routes for field service visits
- distribute stops between drivers fairly or efficiently
- enforce customer time windows
- limit what each vehicle can carry by weight, volume, stops, or time
- generate optimized stop order and ETA for each assigned route
- optimize a single list of stops even when no vehicle is supplied

If you are unsure which dispatch mode to use, start with `Auto`.

### Method signatures

```csharp
Task<DispatchResult> DispatchAsync(DispatchSpecification specification, string apiKey)
Task<DispatchResult> DispatchAsync(DispatchSpecification specification, string apiKey, CancellationToken cancellationToken)
```

### Example 1: Dispatch by using latitude and longitude with one vehicle

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class DispatchExample
{
    public static async Task Main()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);
        var apiKey = "YOUR_API_KEY";

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
                        LatLong = new LatLong
                        {
                            Latitude = 38.917435,
                            Longitude = -77.226263
                        }
                    },
                    FinishLocation = new Location
                    {
                        Name = "Warehouse",
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
                    Name = "B",
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
                    Name = "C",
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

        Console.WriteLine($"Status: {result.Status}");

        if (result.Errors != null)
        {
            foreach (var error in result.Errors)
                Console.WriteLine($"Error: {error.Message}");
        }

        if (result.Items != null)
        {
            foreach (var item in result.Items)
            {
                Console.WriteLine($"Vehicle: {item.Vehicle?.Name}");

                if (item.Locations != null)
                {
                    foreach (var location in item.Locations)
                    {
                        var time = location.Time.GetValueOrDefault();
                        var wait = location.Wait.GetValueOrDefault();

                        Console.WriteLine(
                            $"    Stop: {location.Name,10}   ETA: {location.TimeEstimatedArrival.GetValueOrDefault():HH:mm}   Distance: {location.Distance.GetValueOrDefault():F2} mi   Travel: {time} sec   Wait: {wait} sec");
                    }
                }
            }
        }
    }
}
```

### Example 2: Dispatch by using addresses with one vehicle

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class DispatchExample
{
    public static async Task Main()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);
        var apiKey = "YOUR_API_KEY";

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
                        Address = new Address
                        {
                            Street = "123 Main St",
                            City = "Chicago",
                            State = "IL",
                            PostalCode = "60601",
                            Country = "US"
                        }
                    }
                }
            },
            Locations = new[]
            {
                new Location
                {
                    Name = "A",
                    Address = new Address
                    {
                        Street = "456 Oak Ave",
                        City = "Chicago",
                        State = "IL",
                        PostalCode = "60611",
                        Country = "US"
                    },
                    Weight = 120,
                    Volume = 10,
                    Wait = 300
                },
                new Location
                {
                    Name = "B",
                    Address = new Address
                    {
                        Street = "789 Pine Rd",
                        City = "Chicago",
                        State = "IL",
                        PostalCode = "60622",
                        Country = "US"
                    },
                    Weight = 90,
                    Volume = 8,
                    Wait = 180
                },
                new Location
                {
                    Name = "C",
                    Address = new Address
                    {
                        Street = "5700 S Cicero Ave",
                        City = "Chicago",
                        State = "IL",
                        PostalCode = "60638",
                        Country = "US"
                    },
                    Weight = 20,
                    Volume = 1,
                    Wait = 10
                }
            }
        };

        var result = await client.DispatchAsync(specification, apiKey);

        Console.WriteLine($"Status: {result.Status}");

        if (result.Errors != null)
        {
            foreach (var error in result.Errors)
                Console.WriteLine($"Error: {error.Message}");
        }

        if (result.Items != null)
        {
            foreach (var item in result.Items)
            {
                Console.WriteLine($"Vehicle: {item.Vehicle?.Name}");

                if (item.Locations != null)
                {
                    foreach (var location in item.Locations)
                    {
                        var time = location.Time.GetValueOrDefault();
                        var wait = location.Wait.GetValueOrDefault();

                        Console.WriteLine(
                            $"    Stop: {location.Name,10}   ETA: {location.TimeEstimatedArrival.GetValueOrDefault():HH:mm}   Distance: {location.Distance.GetValueOrDefault():F2} mi   Travel: {time} sec   Wait: {wait} sec");
                    }
                }
            }
        }
    }
}
```

### Example 3: Dispatch with no vehicle, stops only

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class DispatchStopsOnlyExample
{
    public static async Task Main()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);
        var apiKey = "YOUR_API_KEY";

        var specification = new DispatchSpecification
        {
            RoutingService = RoutingService.NetRoad,
            CurrentTime = DateTime.UtcNow,
            DispatchMode = DispatchMode.Auto,
            DistanceUnit = DistanceUnit.Mile,
            RouteOptimize = RouteOptimize.MinimizeTime,
            Locations = new[]
            {
                new Location
                {
                    Name = "Start",
                    LocationType = LocationType.Start,
                    LatLong = new LatLong
                    {
                        Latitude = 38.917435,
                        Longitude = -77.226263
                    }
                },
                new Location
                {
                    Name = "Stop A",
                    LatLong = new LatLong
                    {
                        Latitude = 38.933802,
                        Longitude = -77.1794
                    },
                    Wait = 300
                },
                new Location
                {
                    Name = "Stop B",
                    LatLong = new LatLong
                    {
                        Latitude = 38.93415,
                        Longitude = -77.182173
                    },
                    Wait = 180
                },
                new Location
                {
                    Name = "Finish",
                    LocationType = LocationType.Finish,
                    LatLong = new LatLong
                    {
                        Latitude = 38.917435,
                        Longitude = -77.226263
                    }
                }
            }
        };

        var result = await client.DispatchAsync(specification, apiKey);

        Console.WriteLine($"Status: {result.Status}");

        if (result.Errors != null)
        {
            foreach (var error in result.Errors)
                Console.WriteLine($"Error: {error.Message}");
        }

        if (result.Items != null)
        {
            foreach (var item in result.Items)
            {
                if (item.Locations != null)
                {
                    foreach (var location in item.Locations)
                    {
                        Console.WriteLine(
                            $"Stop: {location.Name}   ETA: {location.TimeEstimatedArrival.GetValueOrDefault():HH:mm}   Distance: {location.Distance.GetValueOrDefault():F2}");
                    }
                }
            }
        }
    }
}
```

### Example 4: Dispatch with latitude and longitude and time windows

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class DispatchTimeWindowsExample
{
    public static async Task Main()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);
        var apiKey = "YOUR_API_KEY";
        var now = DateTime.UtcNow;

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
                        LatLong = new LatLong
                        {
                            Latitude = 38.917435,
                            Longitude = -77.226263
                        }
                    },
                    FinishLocation = new Location
                    {
                        Name = "Warehouse",
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
                    LatLong = new LatLong
                    {
                        Latitude = 38.933802,
                        Longitude = -77.1794
                    },
                    Weight = 120,
                    Volume = 10,
                    Wait = 30,
                    TimeConstraintArrival = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0),
                    TimeConstraintDeparture = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0),
                    CanArriveEarly = true
                },
                new Location
                {
                    Name = "B",
                    LatLong = new LatLong
                    {
                        Latitude = 38.93415,
                        Longitude = -77.182173
                    },
                    Weight = 90,
                    Volume = 8,
                    Wait = 150,
                    TimeConstraintArrival = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0),
                    TimeConstraintDeparture = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0),
                    CanArriveEarly = true
                },
                new Location
                {
                    Name = "C",
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
                }
            }
        };

        var result = await client.DispatchAsync(specification, apiKey);

        Console.WriteLine($"Status: {result.Status}");

        if (result.Errors != null)
        {
            foreach (var error in result.Errors)
                Console.WriteLine($"Error: {error.Message}");
        }

        if (result.Items != null)
        {
            foreach (var item in result.Items)
            {
                Console.WriteLine($"Vehicle: {item.Vehicle?.Name}");

                if (item.Locations != null)
                {
                    foreach (var location in item.Locations)
                    {
                        var time = location.Time.GetValueOrDefault();
                        var wait = location.Wait.GetValueOrDefault();

                        Console.WriteLine(
                            $"    Stop: {location.Name,10}   ETA: {location.TimeEstimatedArrival.GetValueOrDefault():HH:mm}   Distance: {location.Distance.GetValueOrDefault():F2} mi   Travel: {time} sec   Wait: {wait} sec");
                    }
                }
            }
        }
    }
}
```

## 2. GetDistanceAsync

Calculates travel distance and time between two locations.

### Method signatures

```csharp
Task<DistanceResult> GetDistanceAsync(DistanceSpecification specification, string apiKey)
Task<DistanceResult> GetDistanceAsync(DistanceSpecification specification, string apiKey, CancellationToken cancellationToken)
```

### Example

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class DistanceExample
{
    public async Task RunAsync()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);
        var apiKey = "YOUR_API_KEY";

        var specification = new DistanceSpecification
        {
            StartLocation = new Location
            {
                Name = "Start",
                Address = new Address
                {
                    Street = "1600 Pennsylvania Ave NW",
                    City = "Washington",
                    State = "DC",
                    PostalCode = "20500",
                    Country = "USA"
                }
            },
            FinishLocation = new Location
            {
                Name = "Finish",
                Address = new Address
                {
                    Street = "1 Infinite Loop",
                    City = "Cupertino",
                    State = "CA",
                    PostalCode = "95014",
                    Country = "USA"
                }
            },
            DistanceUnit = DistanceUnit.Mile,
            RouteOptimize = RouteOptimize.MinimizeTime
        };

        var result = await client.GetDistanceAsync(specification, apiKey);

        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"Start MatchCode: {result.StartLocationMatchCode}");
        Console.WriteLine($"Finish MatchCode: {result.FinishLocationMatchCode}");
        Console.WriteLine($"Distance: {result.Distance}");
        Console.WriteLine($"Time (seconds): {result.Time}");
    }
}
```

## 3. GeocodeAsync

Geocodes one or more addresses into one or more locations.

Use Geocode to convert postal addresses into latitude/longitude coordinates. Geocoding is usually the first step before route planning or dispatch optimization when you do not already have reliable stop coordinates.

### Method signatures

```csharp
Task<GeocodeResult> GeocodeAsync(GeocodeSpecification specification, string apiKey)
Task<GeocodeResult> GeocodeAsync(GeocodeSpecification specification, string apiKey, CancellationToken cancellationToken)
```

### Example

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class GeocodeExample
{
    public async Task RunAsync()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);
        var apiKey = "YOUR_API_KEY";

        var specification = new GeocodeSpecification
        {
            IsNeedMatchCode = true,
            Addresses = new[]
            {
                new Address
                {
                    Street = "350 5th Ave",
                    City = "New York",
                    State = "NY",
                    PostalCode = "10118",
                    Country = "USA"
                },
                new Address
                {
                    Street = "1 Microsoft Way",
                    City = "Redmond",
                    State = "WA",
                    PostalCode = "98052",
                    Country = "USA"
                }
            }
        };

        var result = await client.GeocodeAsync(specification, apiKey);

        Console.WriteLine($"Status: {result.Status}");

        if (result.Items != null)
        {
            foreach (var item in result.Items)
            {
                Console.WriteLine($"Input address: {item.Address?.Street}, {item.Address?.City}");

                if (item.Locations != null)
                {
                    foreach (var location in item.Locations)
                    {
                        Console.WriteLine($"  MatchCode: {location.MatchCode}");
                        Console.WriteLine($"  Name: {location.Name}");
                        Console.WriteLine($"  Lat: {location.LatLong?.Latitude}");
                        Console.WriteLine($"  Lng: {location.LatLong?.Longitude}");
                    }
                }
            }
        }
    }
}
```

## 4. GetCreditAsync

Returns current credit information.

### Method signatures

```csharp
Task<CreditResult> GetCreditAsync(string apiKey)
Task<CreditResult> GetCreditAsync(string apiKey, CancellationToken cancellationToken)
```

### Example

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class CreditExample
{
    public async Task RunAsync()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);
        var apiKey = "YOUR_API_KEY";

        var result = await client.GetCreditAsync(apiKey);

        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"Credit: {result.Credit}");

        if (result.Errors != null)
        {
            foreach (var error in result.Errors)
                Console.WriteLine($"Error: {error.Message}");
        }
    }
}
```

## 5. GetRouteAsync

Calculates a route for a list of locations.

Use Route when you already know the stop order and want route distance, time, itinerary, points, or map output for that exact sequence.

### Method signatures

```csharp
Task<RouteResult> GetRouteAsync(RouteSpecification specification, string apiKey)
Task<RouteResult> GetRouteAsync(RouteSpecification specification, string apiKey, CancellationToken cancellationToken)
```

### Example

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class RouteExample
{
    public async Task RunAsync()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);
        var apiKey = "YOUR_API_KEY";

        var specification = new RouteSpecification
        {
            Locations = new[]
            {
                new Location
                {
                    Name = "Start",
                    Address = new Address
                    {
                        Street = "500 S Buena Vista St",
                        City = "Burbank",
                        State = "CA",
                        PostalCode = "91521",
                        Country = "USA"
                    }
                },
                new Location
                {
                    Name = "Stop 1",
                    Address = new Address
                    {
                        Street = "1111 S Figueroa St",
                        City = "Los Angeles",
                        State = "CA",
                        PostalCode = "90015",
                        Country = "USA"
                    }
                },
                new Location
                {
                    Name = "Finish",
                    Address = new Address
                    {
                        Street = "100 Universal City Plaza",
                        City = "Universal City",
                        State = "CA",
                        PostalCode = "91608",
                        Country = "USA"
                    }
                }
            },
            RouteOptions = new RouteOptions
            {
                RoutingService = RoutingService.Bing,
                DistanceUnit = DistanceUnit.Mile,
                RouteOptimize = RouteOptimize.MinimizeTime,
                Culture = "en-US",
                HideStops = false,
                ZoomLevel = 0
            }
        };

        var result = await client.GetRouteAsync(specification, apiKey);

        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"Distance: {result.Route?.Distance}");
        Console.WriteLine($"Time: {result.Route?.Time}");
        Console.WriteLine($"Map: {result.Route?.Map}");

        if (result.Route?.RouteLegs != null)
        {
            foreach (var leg in result.Route.RouteLegs)
            {
                Console.WriteLine($"Leg distance: {leg.Distance}");
                Console.WriteLine($"Leg time: {leg.Time}");
            }
        }
    }
}
```

## 6. GetRoutesAsync

Calculates multiple routes and optionally returns a combined map.

Use Routes when you want to calculate several separate routes in one request.

### Method signatures

```csharp
Task<RoutesResult> GetRoutesAsync(RoutesSpecification specification, string apiKey)
Task<RoutesResult> GetRoutesAsync(RoutesSpecification specification, string apiKey, CancellationToken cancellationToken)
```

### Example

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class RoutesExample
{
    public async Task RunAsync()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);
        var apiKey = "YOUR_API_KEY";

        var specification = new RoutesSpecification
        {
            Specifications = new[]
            {
                new RouteSpecification
                {
                    Locations = new[]
                    {
                        new Location
                        {
                            Name = "Route 1 Start",
                            Address = new Address
                            {
                                Street = "1600 Amphitheatre Pkwy",
                                City = "Mountain View",
                                State = "CA",
                                PostalCode = "94043",
                                Country = "USA"
                            }
                        },
                        new Location
                        {
                            Name = "Route 1 Finish",
                            Address = new Address
                            {
                                Street = "1 Infinite Loop",
                                City = "Cupertino",
                                State = "CA",
                                PostalCode = "95014",
                                Country = "USA"
                            }
                        }
                    }
                },
                new RouteSpecification
                {
                    Locations = new[]
                    {
                        new Location
                        {
                            Name = "Route 2 Start",
                            Address = new Address
                            {
                                Street = "1 Microsoft Way",
                                City = "Redmond",
                                State = "WA",
                                PostalCode = "98052",
                                Country = "USA"
                            }
                        },
                        new Location
                        {
                            Name = "Route 2 Finish",
                            Address = new Address
                            {
                                Street = "410 Terry Ave N",
                                City = "Seattle",
                                State = "WA",
                                PostalCode = "98109",
                                Country = "USA"
                            }
                        }
                    }
                }
            },
            RoutesOptions = new RouteOptions
            {
                RoutingService = RoutingService.Bing,
                DistanceUnit = DistanceUnit.Mile,
                RouteOptimize = RouteOptimize.MinimizeTime,
                Culture = "en-US",
                HideStops = false
            }
        };

        var result = await client.GetRoutesAsync(specification, apiKey);

        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"Map: {result.Map}");

        if (result.Results != null)
        {
            var index = 1;
            foreach (var routeResult in result.Results)
            {
                Console.WriteLine($"Route #{index}");
                Console.WriteLine($"  Distance: {routeResult.Route?.Distance}");
                Console.WriteLine($"  Time: {routeResult.Route?.Time}");
                index++;
            }
        }
    }
}
```

## Models overview

### Common result fields

Most result objects contain:

- `Status`
- `Errors`

Possible `Status` values:

- `OperationStatus.None`
- `OperationStatus.Success`
- `OperationStatus.Failed`
- `OperationStatus.SuccessWithErrors`

### Common location fields

`Location` can include:

- `Name`
- `Delivery`
- `Vehicle`
- `Description`
- `Phone`
- `LatLong`
- `Address`
- `Priority`
- `Wait`
- `Volume`
- `Weight`
- `Skids`
- `TimeConstraintArrival`
- `TimeConstraintDeparture`
- `LocationType`
- `MatchCode`

## Common enum values

### RoutingService

```csharp
RoutingService.NetRoad
RoutingService.TrackRoad
RoutingService.Bing
```

### DistanceUnit

```csharp
DistanceUnit.Mile
DistanceUnit.Kilometer
```

### RouteOptimize

```csharp
RouteOptimize.MinimizeTime
RouteOptimize.MinimizeDistance
```

### DispatchMode

```csharp
DispatchMode.Auto
DispatchMode.EqualStop
DispatchMode.SingleRegion
DispatchMode.MultipleRegion
DispatchMode.EqualHour
DispatchMode.EqualDistance
DispatchMode.Central
DispatchMode.TimeWindow
DispatchMode.TimeWindowDepot
DispatchMode.Optima
DispatchMode.BalanceLocation
DispatchMode.BalanceTime
DispatchMode.MinimumVehicles
```

### LocationType

```csharp
LocationType.Midway
LocationType.Start
LocationType.Finish
LocationType.Delivery
LocationType.MidwayDrop
LocationType.Break
```

### MatchCode

```csharp
MatchCode.None
MatchCode.Poor
MatchCode.Approx
MatchCode.Good
MatchCode.Exact
```

## Error handling

The client throws `ApiException` for unexpected HTTP status codes or response parsing errors.

### Example

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class ErrorHandlingExample
{
    public async Task RunAsync()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);

        try
        {
            var result = await client.GetCreditAsync("INVALID_API_KEY");
            Console.WriteLine(result.Credit);
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"HTTP Status: {ex.StatusCode}");
            Console.WriteLine($"Response: {ex.Response}");
            Console.WriteLine(ex.Message);
        }
    }
}
```

## Cancellation support

All methods have overloads that accept `CancellationToken`.

### Example

```csharp
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TrackRoad.Api.Client;

public class CancellationExample
{
    public async Task RunAsync()
    {
        using var httpClient = new HttpClient();
        var client = new TrackRoadClient(httpClient);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var result = await client.GetCreditAsync("YOUR_API_KEY", cts.Token);

        Console.WriteLine(result.Credit);
    }
}
```

## Recommended HttpClient usage

Reuse `HttpClient` whenever possible.

Good:

```csharp
var httpClient = new HttpClient();
var client = new TrackRoadClient(httpClient);
```

Avoid creating a new `HttpClient` for every single call in long-running applications.

In ASP.NET Core, prefer DI:

```csharp
services.AddHttpClient<TrackRoadClient>();
```

Or:

```csharp
services.AddHttpClient<ITrackRoadClient, TrackRoadClient>();
```

### ASP.NET Core dependency injection example

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using TrackRoad.Api.Client;

var services = new ServiceCollection();

services.AddHttpClient<ITrackRoadClient, TrackRoadClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(100);
});

var provider = services.BuildServiceProvider();

var trackRoadClient = provider.GetRequiredService<ITrackRoadClient>();
```

## Notes

- The client sends the API key using the `X-API-Key` request header.
- The client uses `Newtonsoft.Json` for serialization and deserialization.
- The DTOs use nullable properties because the API may omit some fields.
- Response objects may contain `Errors` even when a call succeeds partially.
- For best routing quality, prefer `LatLong` over address input whenever you already have coordinates.
- `DispatchAsync` can work with zero, one, or many vehicles.

## Troubleshooting

### 1. ApiException with non-200 or non-204 status

Check:

- API key
- request payload
- base URL
- whether the server expects additional validation

### 2. Null result properties

This usually means:

- the API did not return those fields
- the request was incomplete
- the operation failed and `Errors` contains details

Always inspect:

```csharp
result.Status
result.Errors
```

### 3. TLS / connection issues

Make sure your application environment can reach:

```text
https://ts6.trackroad.com/
```

## Minimal examples summary

### Credit

```csharp
var result = await client.GetCreditAsync(apiKey);
```

### Dispatch

```csharp
var result = await client.DispatchAsync(specification, apiKey);
```

### Distance

```csharp
var result = await client.GetDistanceAsync(specification, apiKey);
```

### Geocode

```csharp
var result = await client.GeocodeAsync(specification, apiKey);
```

### Route

```csharp
var result = await client.GetRouteAsync(specification, apiKey);
```

### Routes

```csharp
var result = await client.GetRoutesAsync(specification, apiKey);
```

## License

MIT
