using Grpc.Net.Client;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleWebApp.Data;
using SampleWebApp.Models;
using StackExchange.Redis;
using System.Net;
using static SampleWebApp.Greeter;

namespace SampleWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            return View("Error", exception);
        }

        public IActionResult GrpcRequest()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GrpcRequest(string customMessage)
        {
            try
            {
                string grpcUrl = _configuration.GetValue<string>("GrpcUrl")!;

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using var channel = GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions { HttpHandler = handler });
                var client = new GreeterClient(channel);

                var reply = await client.SayHelloAsync(new HelloRequest { Name = customMessage });
                ViewBag.Message = reply.Message;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in gRPC request");
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult WebRequest()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> WebRequest(string siteUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(siteUrl))
                {
                    ViewBag.ErrorMessage = "Please provide a valid site URL.";
                    return View();
                }

                // Use UriBuilder to handle relative URLs gracefully
                UriBuilder uriBuilder = new(siteUrl);

                using HttpClient httpClient = new();

                // Set the User-Agent header to mimic a specific browser
                string customUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0";
                httpClient.DefaultRequestHeaders.Add("User-Agent", customUserAgent);

                // Set a timeout for the HTTP request (adjust the timeout duration as needed)
                httpClient.Timeout = TimeSpan.FromSeconds(10); // Timeout set to 10 seconds

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(uriBuilder.Uri);
                    string content = await response.Content.ReadAsStringAsync();

                    ViewBag.SiteContent = content;
                    ViewBag.ResponseStatusCode = response.StatusCode;
                }
                catch (TaskCanceledException)
                {
                    ViewBag.ErrorMessage = "The request has timed out.";
                    ViewBag.ResponseStatusCode = HttpStatusCode.RequestTimeout;
                }
                catch (HttpRequestException ex)
                {
                    ViewBag.ErrorMessage = $"Error calling the site: {ex.Message}";
                    ViewBag.ResponseStatusCode = HttpStatusCode.InternalServerError;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HTTP request");
                return View("Error", ex);
            }
        }

        public IActionResult RedisRequest(string key, string value)
        {
            try
            {
                string redisConnectionString = _configuration.GetConnectionString("Redis")!;

                using var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
                IDatabase redisDatabase = redisConnection.GetDatabase();

                // Check if the request is for adding a new key-value pair
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    // User input is valid, add the new key-value pair
                    redisDatabase.StringSet(key, value);

                    string retrievedValue = redisDatabase.StringGet(key)!;

                    ViewBag.RedisResult = $"Key: {key}, Value: {retrievedValue}";
                }

                // Retrieve all existing key-value pairs for display
                var existingPairs = GetExistingPairs(redisDatabase);
                ViewBag.ExistingPairs = existingPairs;

                return View();
            }
            catch (Exception ex)
            {
                // Check if the exception is related to Redis connection or operation
                if (ex is RedisConnectionException || ex is RedisException || ex is RedisTimeoutException)
                {
                    ViewBag.ErrorMessage = $"Error accessing Azure Redis: {ex.Message}";
                    ViewBag.RedisResult = null; // Reset RedisResult in case of error
                    return View();
                }

                _logger.LogError(ex, "Error in Redis request");
                return View("Error", ex);
            }
        }

        public async Task<IActionResult> AllInOne()
        {
            try
            {
                string grpcUrl = _configuration["GrpcUrl"]!;
                string sampleSiteUrl = _configuration["SampleSiteUrl"]!;
                string redisConnectionString = _configuration.GetConnectionString("Redis")!;

                // gRPC Request
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using (var grpcChannel = GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions { HttpHandler = handler }))
                {
                    var grpcClient = new GreeterClient(grpcChannel);
                    var grpcReply = await grpcClient.SayHelloAsync(new HelloRequest { Name = "Otel Test App" });
                    ViewBag.GrpcMessage = grpcReply.Message;
                }

                // Web Request
                using (HttpClient httpClient = new())
                {
                    // Set the User-Agent header to mimic a specific browser
                    string customUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0";
                    httpClient.DefaultRequestHeaders.Add("User-Agent", customUserAgent);

                    // Set a timeout for the HTTP request (adjust the timeout duration as needed)
                    httpClient.Timeout = TimeSpan.FromSeconds(10); // Timeout set to 10 seconds

                    try
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(sampleSiteUrl);
                        string content = await response.Content.ReadAsStringAsync();

                        ViewBag.SiteContent = content;
                        ViewBag.ResponseStatusCode = response.StatusCode;
                    }
                    catch (TaskCanceledException)
                    {
                        ViewBag.ErrorMessage = "The request has timed out.";
                        ViewBag.ResponseStatusCode = HttpStatusCode.RequestTimeout;
                    }
                    catch (HttpRequestException ex)
                    {
                        ViewBag.ErrorMessage = $"Error calling the site: {ex.Message}";
                        ViewBag.ResponseStatusCode = HttpStatusCode.InternalServerError;
                    }
                }

                // Redis Request
                using (var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString))
                {
                    IDatabase redisDatabase = redisConnection.GetDatabase();

                    // Retrieve all existing key-value pairs for display
                    var existingPairs = GetExistingPairs(redisDatabase);
                    ViewBag.ExistingPairs = existingPairs;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AllInOne action: {ex.Message}");
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult ExceptionsTest()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExceptionsTest(ExceptionModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                throw model.Type switch
                {
                    "DivideByZeroException" => new DivideByZeroException("Attempted to divide by zero."),
                    "ArgumentNullException" => new ArgumentNullException("Parameter cannot be null."),
                    "ArgumentException" => new ArgumentException("Invalid argument."),
                    "InvalidOperationException" => new InvalidOperationException("Invalid operation."),
                    "IndexOutOfRangeException" => new IndexOutOfRangeException("Index is out of range."),
                    "HttpRequestException" => new HttpRequestException("HTTP request failed."),
                    "TimeoutException" => new TimeoutException("Timeout Exception."),
                    "AccessViolationException" => new AccessViolationException("Access violation."),
                    "OutOfMemoryException" => new OutOfMemoryException("Out of memory."),
                    _ => new Exception("Unknown exception type."),
                };
            }
            catch (Exception ex)
            {
                if (model.IsHandled)
                {
                    _logger.LogError(ex, "Error in ThrowException request");
                    return View("Error", ex);
                }

                throw;
            }
        }

        private IEnumerable<KeyValuePair<string, string>> GetExistingPairs(IDatabase redisDatabase)
        {
            // Retrieve and return all existing key-value pairs from Redis
            var server = redisDatabase.Multiplexer.GetServer(redisDatabase.Multiplexer.GetEndPoints().First());
            var keys = server.Keys();

            var existingPairs = new List<KeyValuePair<string, string>>();

            foreach (var existingKey in keys)
            {
                string existingValue = redisDatabase.StringGet(existingKey)!;
                existingPairs.Add(new KeyValuePair<string, string>(existingKey!, existingValue));
            }

            return existingPairs;
        }
    }
}