using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TrackRoad.Api.Client
{
    // Run this command to create NuGet package:
    // dotnet pack -c Release

    // Run this command to publish generated package to NuGet.org
    // dotnet nuget push bin/Release/TrackRoad.Api.Client.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
    public interface ITrackRoadClient
    {
        Task<CreditResult> GetCreditAsync(string apiKey);
        Task<CreditResult> GetCreditAsync(string apiKey, CancellationToken cancellationToken);

        Task<DispatchResult> DispatchAsync(DispatchSpecification specification, string apiKey);
        Task<DispatchResult> DispatchAsync(DispatchSpecification specification, string apiKey, CancellationToken cancellationToken);

        Task<DistanceResult> GetDistanceAsync(DistanceSpecification specification, string apiKey);
        Task<DistanceResult> GetDistanceAsync(DistanceSpecification specification, string apiKey, CancellationToken cancellationToken);

        Task<GeocodeResult> GeocodeAsync(GeocodeSpecification specification, string apiKey);
        Task<GeocodeResult> GeocodeAsync(GeocodeSpecification specification, string apiKey, CancellationToken cancellationToken);

        //Task<LoginResult> LoginAsync(LoginSpecification specification, string apiKey);
        //Task<LoginResult> LoginAsync(LoginSpecification specification, string apiKey, CancellationToken cancellationToken);

        //Task LogoutAsync(string apiKey);
        //Task LogoutAsync(string apiKey, CancellationToken cancellationToken);

        Task<RouteResult> GetRouteAsync(RouteSpecification specification, string apiKey);
        Task<RouteResult> GetRouteAsync(RouteSpecification specification, string apiKey, CancellationToken cancellationToken);

        Task<RoutesResult> GetRoutesAsync(RoutesSpecification specification, string apiKey);
        Task<RoutesResult> GetRoutesAsync(RoutesSpecification specification, string apiKey, CancellationToken cancellationToken);
    }

    public partial class TrackRoadClient : ITrackRoadClient
    {
        private string _baseUrl;
        private readonly HttpClient _httpClient;
        private static readonly Lazy<JsonSerializerSettings> _settings =
            new Lazy<JsonSerializerSettings>(CreateSerializerSettings, true);
        private JsonSerializerSettings _instanceSettings;

        public TrackRoadClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            BaseUrl = "https://ts6.trackroad.com";
            Initialize();
        }

        private static JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new JsonSerializerSettings();
            UpdateJsonSerializerSettings(settings);
            return settings;
        }

        public string BaseUrl
        {
            get => _baseUrl;
            set
            {
                _baseUrl = value;
                if (!string.IsNullOrEmpty(_baseUrl) && !_baseUrl.EndsWith("/"))
                    _baseUrl += "/";
            }
        }

        protected JsonSerializerSettings JsonSerializerSettings => _instanceSettings ?? _settings.Value;

        static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings);

        partial void Initialize();

        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url);
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder);
        partial void ProcessResponse(HttpClient client, HttpResponseMessage response);

        public Task<CreditResult> GetCreditAsync(string apiKey)
        {
            return GetCreditAsync(apiKey, CancellationToken.None);
        }

        public async Task<CreditResult> GetCreditAsync(string apiKey, CancellationToken cancellationToken)
        {
            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            using (var request = new HttpRequestMessage())
            {
                request.Headers.TryAddWithoutValidation("X-API-Key", ConvertToString(apiKey, CultureInfo.InvariantCulture));
                request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                request.Method = HttpMethod.Post;
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl))
                    urlBuilder.Append(_baseUrl);
                urlBuilder.Append("rest/Credit");

                PrepareRequest(_httpClient, request, urlBuilder);

                var url = urlBuilder.ToString();
                request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

                PrepareRequest(_httpClient, request, url);

                using (var response = await _httpClient
                           .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                           .ConfigureAwait(false))
                {
                    var headers = CreateHeaders(response);

                    ProcessResponse(_httpClient, response);

                    var status = (int)response.StatusCode;
                    if (status == 200)
                    {
                        var objectResponse = await ReadObjectResponseAsync<CreditResult>(response, headers, cancellationToken)
                            .ConfigureAwait(false);
                        if (objectResponse.Object == null)
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);

                        return objectResponse.Object;
                    }

                    var responseData = response.Content == null
                        ? null
                        : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);

                    throw new ApiException(
                        "The HTTP status code of the response was not expected (" + status + ").",
                        status,
                        responseData,
                        headers,
                        null);
                }
            }
        }

        public Task<DispatchResult> DispatchAsync(DispatchSpecification specification, string apiKey)
        {
            return DispatchAsync(specification, apiKey, CancellationToken.None);
        }

        public async Task<DispatchResult> DispatchAsync(
            DispatchSpecification specification,
            string apiKey,
            CancellationToken cancellationToken)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));
            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            using (var request = new HttpRequestMessage())
            {
                request.Headers.TryAddWithoutValidation("X-API-Key", ConvertToString(apiKey, CultureInfo.InvariantCulture));

                var json = JsonConvert.SerializeObject(specification, JsonSerializerSettings);
                var content = new StringContent(json);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request.Content = content;

                request.Method = HttpMethod.Post;
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl))
                    urlBuilder.Append(_baseUrl);
                urlBuilder.Append("rest/Dispatch");

                PrepareRequest(_httpClient, request, urlBuilder);

                var url = urlBuilder.ToString();
                request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

                PrepareRequest(_httpClient, request, url);

                using (var response = await _httpClient
                           .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                           .ConfigureAwait(false))
                {
                    var headers = CreateHeaders(response);

                    ProcessResponse(_httpClient, response);

                    var status = (int)response.StatusCode;
                    if (status == 200)
                    {
                        var objectResponse = await ReadObjectResponseAsync<DispatchResult>(response, headers, cancellationToken)
                            .ConfigureAwait(false);
                        if (objectResponse.Object == null)
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);

                        return objectResponse.Object;
                    }

                    var responseData = response.Content == null
                        ? null
                        : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);

                    throw new ApiException(
                        "The HTTP status code of the response was not expected (" + status + ").",
                        status,
                        responseData,
                        headers,
                        null);
                }
            }
        }

        public Task<DistanceResult> GetDistanceAsync(DistanceSpecification specification, string apiKey)
        {
            return GetDistanceAsync(specification, apiKey, CancellationToken.None);
        }

        public async Task<DistanceResult> GetDistanceAsync(
            DistanceSpecification specification,
            string apiKey,
            CancellationToken cancellationToken)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));
            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            using (var request = new HttpRequestMessage())
            {
                request.Headers.TryAddWithoutValidation("X-API-Key", ConvertToString(apiKey, CultureInfo.InvariantCulture));

                var json = JsonConvert.SerializeObject(specification, JsonSerializerSettings);
                var content = new StringContent(json);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request.Content = content;

                request.Method = HttpMethod.Post;
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl))
                    urlBuilder.Append(_baseUrl);
                urlBuilder.Append("rest/Distance");

                PrepareRequest(_httpClient, request, urlBuilder);

                var url = urlBuilder.ToString();
                request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

                PrepareRequest(_httpClient, request, url);

                using (var response = await _httpClient
                           .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                           .ConfigureAwait(false))
                {
                    var headers = CreateHeaders(response);

                    ProcessResponse(_httpClient, response);

                    var status = (int)response.StatusCode;
                    if (status == 200)
                    {
                        var objectResponse = await ReadObjectResponseAsync<DistanceResult>(response, headers, cancellationToken)
                            .ConfigureAwait(false);
                        if (objectResponse.Object == null)
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);

                        return objectResponse.Object;
                    }

                    var responseData = response.Content == null
                        ? null
                        : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);

                    throw new ApiException(
                        "The HTTP status code of the response was not expected (" + status + ").",
                        status,
                        responseData,
                        headers,
                        null);
                }
            }
        }

        public Task<GeocodeResult> GeocodeAsync(GeocodeSpecification specification, string apiKey)
        {
            return GeocodeAsync(specification, apiKey, CancellationToken.None);
        }

        public async Task<GeocodeResult> GeocodeAsync(
            GeocodeSpecification specification,
            string apiKey,
            CancellationToken cancellationToken)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));
            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            using (var request = new HttpRequestMessage())
            {
                request.Headers.TryAddWithoutValidation("X-API-Key", ConvertToString(apiKey, CultureInfo.InvariantCulture));

                var json = JsonConvert.SerializeObject(specification, JsonSerializerSettings);
                var content = new StringContent(json);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request.Content = content;

                request.Method = HttpMethod.Post;
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl))
                    urlBuilder.Append(_baseUrl);
                urlBuilder.Append("rest/Geocode");

                PrepareRequest(_httpClient, request, urlBuilder);

                var url = urlBuilder.ToString();
                request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

                PrepareRequest(_httpClient, request, url);

                using (var response = await _httpClient
                           .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                           .ConfigureAwait(false))
                {
                    var headers = CreateHeaders(response);

                    ProcessResponse(_httpClient, response);

                    var status = (int)response.StatusCode;
                    if (status == 200)
                    {
                        var objectResponse = await ReadObjectResponseAsync<GeocodeResult>(response, headers, cancellationToken)
                            .ConfigureAwait(false);
                        if (objectResponse.Object == null)
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);

                        return objectResponse.Object;
                    }

                    var responseData = response.Content == null
                        ? null
                        : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);

                    throw new ApiException(
                        "The HTTP status code of the response was not expected (" + status + ").",
                        status,
                        responseData,
                        headers,
                        null);
                }
            }
        }

        //public Task<LoginResult> LoginAsync(LoginSpecification specification, string apiKey)
        //{
        //    return LoginAsync(specification, apiKey, CancellationToken.None);
        //}

        //public async Task<LoginResult> LoginAsync(
        //    LoginSpecification specification,
        //    string apiKey,
        //    CancellationToken cancellationToken)
        //{
        //    if (specification == null)
        //        throw new ArgumentNullException(nameof(specification));
        //    if (apiKey == null)
        //        throw new ArgumentNullException(nameof(apiKey));

        //    using (var request = new HttpRequestMessage())
        //    {
        //        request.Headers.TryAddWithoutValidation("X-API-Key", ConvertToString(apiKey, CultureInfo.InvariantCulture));

        //        var json = JsonConvert.SerializeObject(specification, JsonSerializerSettings);
        //        var content = new StringContent(json);
        //        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        //        request.Content = content;

        //        request.Method = HttpMethod.Post;
        //        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

        //        var urlBuilder = new StringBuilder();
        //        if (!string.IsNullOrEmpty(_baseUrl))
        //            urlBuilder.Append(_baseUrl);
        //        urlBuilder.Append("rest/Login");

        //        PrepareRequest(_httpClient, request, urlBuilder);

        //        var url = urlBuilder.ToString();
        //        request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

        //        PrepareRequest(_httpClient, request, url);

        //        using (var response = await _httpClient
        //                   .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
        //                   .ConfigureAwait(false))
        //        {
        //            var headers = CreateHeaders(response);

        //            ProcessResponse(_httpClient, response);

        //            var status = (int)response.StatusCode;
        //            if (status == 200)
        //            {
        //                var objectResponse = await ReadObjectResponseAsync<LoginResult>(response, headers, cancellationToken)
        //                    .ConfigureAwait(false);
        //                if (objectResponse.Object == null)
        //                    throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);

        //                return objectResponse.Object;
        //            }

        //            var responseData = response.Content == null
        //                ? null
        //                : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);

        //            throw new ApiException(
        //                "The HTTP status code of the response was not expected (" + status + ").",
        //                status,
        //                responseData,
        //                headers,
        //                null);
        //        }
        //    }
        //}

        //public Task LogoutAsync(string apiKey)
        //{
        //    return LogoutAsync(apiKey, CancellationToken.None);
        //}

        //public async Task LogoutAsync(string apiKey, CancellationToken cancellationToken)
        //{
        //    if (apiKey == null)
        //        throw new ArgumentNullException(nameof(apiKey));

        //    using (var request = new HttpRequestMessage())
        //    {
        //        request.Headers.TryAddWithoutValidation("X-API-Key", ConvertToString(apiKey, CultureInfo.InvariantCulture));
        //        request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        //        request.Method = HttpMethod.Post;

        //        var urlBuilder = new StringBuilder();
        //        if (!string.IsNullOrEmpty(_baseUrl))
        //            urlBuilder.Append(_baseUrl);
        //        urlBuilder.Append("rest/Logout");

        //        PrepareRequest(_httpClient, request, urlBuilder);

        //        var url = urlBuilder.ToString();
        //        request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

        //        PrepareRequest(_httpClient, request, url);

        //        using (var response = await _httpClient
        //                   .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
        //                   .ConfigureAwait(false))
        //        {
        //            var headers = CreateHeaders(response);

        //            ProcessResponse(_httpClient, response);

        //            var status = (int)response.StatusCode;
        //            if (status == 204)
        //                return;

        //            var responseData = response.Content == null
        //                ? null
        //                : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);

        //            throw new ApiException(
        //                "The HTTP status code of the response was not expected (" + status + ").",
        //                status,
        //                responseData,
        //                headers,
        //                null);
        //        }
        //    }
        //}

        public Task<RouteResult> GetRouteAsync(RouteSpecification specification, string apiKey)
        {
            return GetRouteAsync(specification, apiKey, CancellationToken.None);
        }

        public async Task<RouteResult> GetRouteAsync(
            RouteSpecification specification,
            string apiKey,
            CancellationToken cancellationToken)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));
            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            using (var request = new HttpRequestMessage())
            {
                request.Headers.TryAddWithoutValidation("X-API-Key", ConvertToString(apiKey, CultureInfo.InvariantCulture));

                var json = JsonConvert.SerializeObject(specification, JsonSerializerSettings);
                var content = new StringContent(json);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request.Content = content;

                request.Method = HttpMethod.Post;
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl))
                    urlBuilder.Append(_baseUrl);
                urlBuilder.Append("rest/Route");

                PrepareRequest(_httpClient, request, urlBuilder);

                var url = urlBuilder.ToString();
                request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

                PrepareRequest(_httpClient, request, url);

                using (var response = await _httpClient
                           .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                           .ConfigureAwait(false))
                {
                    var headers = CreateHeaders(response);

                    ProcessResponse(_httpClient, response);

                    var status = (int)response.StatusCode;
                    if (status == 200)
                    {
                        var objectResponse = await ReadObjectResponseAsync<RouteResult>(response, headers, cancellationToken)
                            .ConfigureAwait(false);
                        if (objectResponse.Object == null)
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);

                        return objectResponse.Object;
                    }

                    var responseData = response.Content == null
                        ? null
                        : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);

                    throw new ApiException(
                        "The HTTP status code of the response was not expected (" + status + ").",
                        status,
                        responseData,
                        headers,
                        null);
                }
            }
        }

        public Task<RoutesResult> GetRoutesAsync(RoutesSpecification specification, string apiKey)
        {
            return GetRoutesAsync(specification, apiKey, CancellationToken.None);
        }

        public async Task<RoutesResult> GetRoutesAsync(
            RoutesSpecification specification,
            string apiKey,
            CancellationToken cancellationToken)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));
            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            using (var request = new HttpRequestMessage())
            {
                request.Headers.TryAddWithoutValidation("X-API-Key", ConvertToString(apiKey, CultureInfo.InvariantCulture));

                var json = JsonConvert.SerializeObject(specification, JsonSerializerSettings);
                var content = new StringContent(json);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request.Content = content;

                request.Method = HttpMethod.Post;
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl))
                    urlBuilder.Append(_baseUrl);
                urlBuilder.Append("rest/Routes");

                PrepareRequest(_httpClient, request, urlBuilder);

                var url = urlBuilder.ToString();
                request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

                PrepareRequest(_httpClient, request, url);

                using (var response = await _httpClient
                           .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                           .ConfigureAwait(false))
                {
                    var headers = CreateHeaders(response);

                    ProcessResponse(_httpClient, response);

                    var status = (int)response.StatusCode;
                    if (status == 200)
                    {
                        var objectResponse = await ReadObjectResponseAsync<RoutesResult>(response, headers, cancellationToken)
                            .ConfigureAwait(false);
                        if (objectResponse.Object == null)
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);

                        return objectResponse.Object;
                    }

                    var responseData = response.Content == null
                        ? null
                        : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);

                    throw new ApiException(
                        "The HTTP status code of the response was not expected (" + status + ").",
                        status,
                        responseData,
                        headers,
                        null);
                }
            }
        }

        private static Dictionary<string, IEnumerable<string>> CreateHeaders(HttpResponseMessage response)
        {
            var headers = new Dictionary<string, IEnumerable<string>>();

            foreach (var item in response.Headers)
                headers[item.Key] = item.Value;

            if (response.Content?.Headers != null)
            {
                foreach (var item in response.Content.Headers)
                    headers[item.Key] = item.Value;
            }

            return headers;
        }

        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                Object = responseObject;
                Text = responseText;
            }

            public T Object { get; }

            public string Text { get; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Task<string> ReadAsStringAsync(HttpContent content, CancellationToken cancellationToken)
        {
#if NET5_0_OR_GREATER
            return content.ReadAsStringAsync(cancellationToken);
#else
            return content.ReadAsStringAsync();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Task<Stream> ReadAsStreamAsync(HttpContent content, CancellationToken cancellationToken)
        {
#if NET5_0_OR_GREATER
            return content.ReadAsStreamAsync(cancellationToken);
#else
            return content.ReadAsStreamAsync();
#endif
        }

        public bool ReadResponseAsString { get; set; }

        protected virtual async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(
            HttpResponseMessage response,
            IReadOnlyDictionary<string, IEnumerable<string>> headers,
            CancellationToken cancellationToken)
        {
            if (response == null || response.Content == null)
                return new ObjectResponseResult<T>(default(T), string.Empty);

            if (ReadResponseAsString)
            {
                var responseText = await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
                try
                {
                    var typedBody = JsonConvert.DeserializeObject<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }

            try
            {
                using (var responseStream = await ReadAsStreamAsync(response.Content, cancellationToken).ConfigureAwait(false))
                using (var streamReader = new StreamReader(responseStream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var serializer = JsonSerializer.Create(JsonSerializerSettings);
                    var typedBody = serializer.Deserialize<T>(jsonTextReader);
                    return new ObjectResponseResult<T>(typedBody, string.Empty);
                }
            }
            catch (JsonException exception)
            {
                var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
            }
        }

        private string ConvertToString(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return "";

            if (value is Enum)
            {
                var name = Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = value.GetType().GetField(name);
                    if (field != null)
                    {
                        var attribute = Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) as EnumMemberAttribute;
                        if (attribute != null)
                            return attribute.Value ?? name;
                    }

                    var converted = Convert.ToString(
                        Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), cultureInfo),
                        cultureInfo);

                    return converted ?? string.Empty;
                }
            }
            else if (value is bool boolValue)
            {
                return Convert.ToString(boolValue, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[] bytes)
            {
                return Convert.ToBase64String(bytes);
            }
            else if (value is string[] stringArray)
            {
                return string.Join(",", stringArray);
            }
            else if (value.GetType().IsArray)
            {
                var valueArray = (Array)value;
                var valueTextArray = new string[valueArray.Length];
                for (var i = 0; i < valueArray.Length; i++)
                    valueTextArray[i] = ConvertToString(valueArray.GetValue(i), cultureInfo);

                return string.Join(",", valueTextArray);
            }

            var result = Convert.ToString(value, cultureInfo);
            return result ?? "";
        }
    }

    public enum OperationStatus
    {
        None = 0,
        Success = 1,
        Failed = 2,
        SuccessWithErrors = 3
    }

    public enum RoutingService
    {
        NetRoad = 0,
        TrackRoad = 1,
        Bing = 2
    }

    public enum DispatchMode
    {
        Auto = 0,
        EqualStop = 1,
        SingleRegion = 2,
        MultipleRegion = 3,
        EqualHour = 4,
        EqualDistance = 5,
        Central = 5,
        TimeWindow = 6,
        TimeWindowDepot = 7,
        Optima = 8,
        BalanceLocation = 9,
        BalanceTime = 10,
        MinimumVehicles = 11
    }

    public enum DistanceUnit
    {
        Mile = 0,
        Kilometer = 1
    }

    public enum RouteOptimize
    {
        MinimizeTime = 0,
        MinimizeDistance = 1
    }

    public enum TransportType
    {
        None = 0,
        Car = 1,
        Truck = 2,
        Bus = 3,
        Motorcycle = 4,
        Pedestrian = 5,
        MotorScooter = 6,
        Bicycle = 7
    }

    public enum MatchCode : byte
    {
        None = 0,
        Poor = 1,
        Approx = 2,
        Good = 3,
        Exact = 4
    }

    public enum LocationType
    {
        Midway = 0,
        Start = 1,
        Finish = 2,
        Delivery = 3,
        MidwayDrop = 4,
        Break = 5
    }

    public enum Restriction
    {
        DoNotEnter = 0,
        DoNotExit = 1
    }

    public enum RouteWarningSeverity
    {
        None = 0,
        LowImpact = 1,
        Minor = 2,
        Moderate = 3,
        Serious = 4
    }

    public enum RouteHintType
    {
        PreviousIntersection = 0,
        NextIntersection = 1,
        Landmark = 2
    }

    public partial class CreditResult
    {
        [JsonProperty("Credit", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Credit { get; set; }

        [JsonProperty("Errors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Error> Errors { get; set; }

        [JsonProperty("Status", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public OperationStatus? Status { get; set; }
    }

    public partial class Error
    {
        [JsonProperty("Message", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }

    public partial class DispatchSpecification
    {
        [JsonProperty("RoutingService", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RoutingService? RoutingService { get; set; }

        [JsonProperty("IsNeedMatchCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNeedMatchCode { get; set; }

        [JsonProperty("CurrentTime", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CurrentTime { get; set; }

        [JsonProperty("DispatchMode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DispatchMode? DispatchMode { get; set; }

        [JsonProperty("MinimumOptimization", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? MinimumOptimization { get; set; }

        [JsonProperty("DistanceUnit", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DistanceUnit? DistanceUnit { get; set; }

        [JsonProperty("RouteOptimize", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RouteOptimize? RouteOptimize { get; set; }

        [JsonProperty("Vehicles", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Vehicle> Vehicles { get; set; }

        [JsonProperty("Locations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Location> Locations { get; set; }
    }

    public partial class Vehicle
    {
        [JsonProperty("Name", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("Email", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty("Group", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Group { get; set; }

        [JsonProperty("Speed", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Speed { get; set; }

        [JsonProperty("MaxStops", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxStops { get; set; }

        [JsonProperty("MaxWeight", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? MaxWeight { get; set; }

        [JsonProperty("MaxSkids", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxSkids { get; set; }

        [JsonProperty("MaxVolume", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? MaxVolume { get; set; }

        [JsonProperty("MaxMinutes", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxMinutes { get; set; }

        [JsonProperty("FuelCost", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? FuelCost { get; set; }

        [JsonProperty("OnTheRoad", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? OnTheRoad { get; set; }

        [JsonProperty("Tin", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Tin { get; set; }

        [JsonProperty("Tout", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Tout { get; set; }

        [JsonProperty("StartLocation", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Location StartLocation { get; set; }

        [JsonProperty("FinishLocation", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Location FinishLocation { get; set; }

        [JsonProperty("Roles", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Roles { get; set; }

        [JsonProperty("Shapes", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Shape> Shapes { get; set; }

        [JsonProperty("TransportType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public TransportType? TransportType { get; set; }

        [JsonProperty("ExcludeTolls", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? ExcludeTolls { get; set; }

        [JsonProperty("ExcludeTunnels", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? ExcludeTunnels { get; set; }

        [JsonProperty("ExcludeCashOnlyTolls", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? ExcludeCashOnlyTolls { get; set; }

        [JsonProperty("ExcludeHighways", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? ExcludeHighways { get; set; }

        [JsonProperty("ExcludeUnpaved", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? ExcludeUnpaved { get; set; }

        [JsonProperty("UseTruckRoute", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseTruckRoute { get; set; }

        [JsonProperty("Width", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public float? Width { get; set; }

        [JsonProperty("Height", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public float? Height { get; set; }

        [JsonProperty("Length", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public float? Length { get; set; }

        [JsonProperty("Weight", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public float? Weight { get; set; }

        [JsonProperty("AxleLoad", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public float? AxleLoad { get; set; }

        [JsonProperty("AxleCount", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? AxleCount { get; set; }

        [JsonProperty("UseTrails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseTrails { get; set; }

        [JsonProperty("UseFerry", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseFerry { get; set; }

        [JsonProperty("UseLivingStreets", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseLivingStreets { get; set; }

        [JsonProperty("UseTracks", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseTracks { get; set; }

        [JsonProperty("UseHills", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseHills { get; set; }

        [JsonProperty("UseLit", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseLit { get; set; }

        [JsonProperty("WalkingSpeed", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public float? WalkingSpeed { get; set; }

        [JsonProperty("UsePrimary", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? UsePrimary { get; set; }

        [JsonProperty("UseRoads", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseRoads { get; set; }

        [JsonProperty("AvoidBadSurfaces", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? AvoidBadSurfaces { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("MatchCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public MatchCode? MatchCode { get; set; }

        [JsonProperty("Name", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("Delivery", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Delivery { get; set; }

        [JsonProperty("DeliveryNonStop", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? DeliveryNonStop { get; set; }

        [JsonProperty("KeepSameOrder", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? KeepSameOrder { get; set; }

        [JsonProperty("Vehicle", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Vehicle { get; set; }

        [JsonProperty("Description", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("Phone", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Phone { get; set; }

        [JsonProperty("LatLong", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public LatLong LatLong { get; set; }

        [JsonProperty("Address", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Address Address { get; set; }

        [JsonProperty("Priority", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Priority { get; set; }

        [JsonProperty("Wait", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Wait { get; set; }

        [JsonProperty("Volume", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? Volume { get; set; }

        [JsonProperty("Weight", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? Weight { get; set; }

        [JsonProperty("Skids", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Skids { get; set; }

        [JsonProperty("TimeConstraintArrival", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? TimeConstraintArrival { get; set; }

        [JsonProperty("TimeConstraintDeparture", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? TimeConstraintDeparture { get; set; }

        [JsonProperty("TimeConstraintArrival2", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? TimeConstraintArrival2 { get; set; }

        [JsonProperty("TimeConstraintDeparture2", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? TimeConstraintDeparture2 { get; set; }

        [JsonProperty("TimeEstimatedArrival", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? TimeEstimatedArrival { get; set; }

        [JsonProperty("LocationType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public LocationType? LocationType { get; set; }

        [JsonProperty("CanArriveEarly", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanArriveEarly { get; set; }

        [JsonProperty("Distance", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? Distance { get; set; }

        [JsonProperty("Time", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Time { get; set; }

        [JsonProperty("Conditions", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Conditions { get; set; }
    }

    public partial class Shape
    {
        [JsonProperty("Points", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<LatLong> Points { get; set; }

        [JsonProperty("Restriction", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Restriction? Restriction { get; set; }
    }

    public partial class LatLong
    {
        [JsonProperty("Latitude", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? Latitude { get; set; }

        [JsonProperty("Longitude", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? Longitude { get; set; }
    }

    public partial class Address
    {
        [JsonProperty("Street", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Street { get; set; }

        [JsonProperty("City", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        [JsonProperty("State", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty("PostalCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string PostalCode { get; set; }

        [JsonProperty("Country", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }
    }

    public partial class DispatchResult
    {
        [JsonProperty("Items", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<VehicleItem> Items { get; set; }

        [JsonProperty("Errors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Error> Errors { get; set; }

        [JsonProperty("Status", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public OperationStatus? Status { get; set; }
    }

    public partial class VehicleItem
    {
        [JsonProperty("Vehicle", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Vehicle Vehicle { get; set; }

        [JsonProperty("Locations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Location> Locations { get; set; }
    }

    public partial class DistanceSpecification
    {
        [JsonProperty("StartLocation", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Location StartLocation { get; set; }

        [JsonProperty("FinishLocation", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Location FinishLocation { get; set; }

        [JsonProperty("DistanceUnit", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DistanceUnit? DistanceUnit { get; set; }

        [JsonProperty("RouteOptimize", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RouteOptimize? RouteOptimize { get; set; }
    }

    public partial class DistanceResult
    {
        [JsonProperty("StartLocationMatchCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public MatchCode? StartLocationMatchCode { get; set; }

        [JsonProperty("FinishLocationMatchCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public MatchCode? FinishLocationMatchCode { get; set; }

        [JsonProperty("Distance", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? Distance { get; set; }

        [JsonProperty("Time", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Time { get; set; }

        [JsonProperty("Errors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Error> Errors { get; set; }

        [JsonProperty("Status", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public OperationStatus? Status { get; set; }
    }

    public partial class GeocodeSpecification
    {
        [JsonProperty("IsNeedMatchCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNeedMatchCode { get; set; }

        [JsonProperty("Addresses", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Address> Addresses { get; set; }
    }

    public partial class GeocodeResult
    {
        [JsonProperty("Items", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<GeocodeItem> Items { get; set; }

        [JsonProperty("Errors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Error> Errors { get; set; }

        [JsonProperty("Status", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public OperationStatus? Status { get; set; }
    }

    public partial class GeocodeItem
    {
        [JsonProperty("Address", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Address Address { get; set; }

        [JsonProperty("Locations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Location> Locations { get; set; }
    }

    //public partial class LoginSpecification
    //{
    //    [JsonProperty("UserName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    //    public string UserName { get; set; }

    //    [JsonProperty("Password", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    //    public string Password { get; set; }
    //}

    //public partial class LoginResult
    //{
    //    [JsonProperty("Message", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    //    public string Message { get; set; }

    //    [JsonProperty("Status", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    //    public OperationStatus? Status { get; set; }

    //    [JsonProperty("Key", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    //    public string Key { get; set; }
    //}

    public partial class RouteSpecification
    {
        [JsonProperty("Locations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Location> Locations { get; set; }

        [JsonProperty("RouteOptions", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RouteOptions RouteOptions { get; set; }
    }

    public partial class RouteOptions
    {
        [JsonProperty("RoutingService", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RoutingService? RoutingService { get; set; }

        [JsonProperty("DistanceUnit", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DistanceUnit? DistanceUnit { get; set; }

        [JsonProperty("RouteOptimize", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RouteOptimize? RouteOptimize { get; set; }

        [JsonProperty("Culture", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Culture { get; set; }

        [JsonProperty("MapSize", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public MapSize MapSize { get; set; }

        [JsonProperty("RouteColor", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RouteColor RouteColor { get; set; }

        [JsonProperty("MapCenter", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public LatLong MapCenter { get; set; }

        [JsonProperty("HideStops", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? HideStops { get; set; }

        [JsonProperty("ZoomLevel", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? ZoomLevel { get; set; }
    }

    public partial class MapSize
    {
        [JsonProperty("Width", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Width { get; set; }

        [JsonProperty("Height", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Height { get; set; }
    }

    public partial class RouteColor
    {
        [JsonProperty("A", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? A { get; set; }

        [JsonProperty("R", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? R { get; set; }

        [JsonProperty("G", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? G { get; set; }

        [JsonProperty("B", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? B { get; set; }
    }

    public partial class RouteResult
    {
        [JsonProperty("Route", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Route Route { get; set; }

        [JsonProperty("Errors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Error> Errors { get; set; }

        [JsonProperty("Status", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public OperationStatus? Status { get; set; }
    }

    public partial class Route
    {
        [JsonProperty("Distance", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? Distance { get; set; }

        [JsonProperty("Time", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Time { get; set; }

        [JsonProperty("RouteLegs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<RouteLeg> RouteLegs { get; set; }

        [JsonProperty("Points", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<LatLong> Points { get; set; }

        [JsonProperty("Map", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Map { get; set; }
    }

    public partial class RouteLeg
    {
        [JsonProperty("Distance", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? Distance { get; set; }

        [JsonProperty("Time", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Time { get; set; }

        [JsonProperty("Itinerary", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RouteItinerary Itinerary { get; set; }

        [JsonProperty("Map", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Map { get; set; }
    }

    public partial class RouteItinerary
    {
        [JsonProperty("Items", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<RouteItineraryItem> Items { get; set; }
    }

    public partial class RouteItineraryItem
    {
        [JsonProperty("Distance", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? Distance { get; set; }

        [JsonProperty("LatLong", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public LatLong LatLong { get; set; }

        [JsonProperty("Text", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty("Time", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Time { get; set; }

        [JsonProperty("Warnings", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<RouteWarning> Warnings { get; set; }

        [JsonProperty("Hints", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<RouteHint> Hints { get; set; }
    }

    public partial class RouteWarning
    {
        [JsonProperty("Severity", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RouteWarningSeverity? Severity { get; set; }

        [JsonProperty("Text", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }
    }

    public partial class RouteHint
    {
        [JsonProperty("Type", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RouteHintType? Type { get; set; }

        [JsonProperty("Text", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }
    }

    public partial class RoutesSpecification
    {
        [JsonProperty("Specifications", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<RouteSpecification> Specifications { get; set; }

        [JsonProperty("RoutesOptions", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RouteOptions RoutesOptions { get; set; }
    }

    public partial class RoutesResult
    {
        [JsonProperty("Results", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<RouteResult> Results { get; set; }

        [JsonProperty("Map", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Map { get; set; }

        [JsonProperty("Errors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Error> Errors { get; set; }

        [JsonProperty("Status", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public OperationStatus? Status { get; set; }
    }

    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public string Response { get; }

        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

        public ApiException(
            string message,
            int statusCode,
            string response,
            IReadOnlyDictionary<string, IEnumerable<string>> headers,
            Exception innerException)
            : base(
                message + "\n\nStatus: " + statusCode + "\nResponse: \n" +
                (response == null ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)),
                innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        public override string ToString()
        {
            return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
        }
    }

    public class ApiException<TResult> : ApiException
    {
        public TResult Result { get; }

        public ApiException(
            string message,
            int statusCode,
            string response,
            IReadOnlyDictionary<string, IEnumerable<string>> headers,
            TResult result,
            Exception innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }
}