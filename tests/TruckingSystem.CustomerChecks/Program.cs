using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TruckingSystem.API.Controllers;
using TruckingSystem.Application.Customers;
using TruckingSystem.Infrastructure.ExternalServices;

var image = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=");
var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Nvidia:ApiKey"] = "test-only-key",
    ["Nvidia:Model"] = "meta/llama-3.2-11b-vision-instruct",
    ["Nvidia:BaseUrl"] = "https://integrate.api.nvidia.com/v1"
}).Build();
var checks = 0;

static HttpResponseMessage Response(string content, HttpStatusCode status = HttpStatusCode.OK) => new(status)
{
    Content = new StringContent(content, Encoding.UTF8, "application/json")
};
static string Envelope(string content, string finishReason = "stop") => JsonSerializer.Serialize(new
{
    choices = new[] { new { message = new { content }, finish_reason = finishReason } }
});
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    checks++;
}
async Task<T> Throws<T>(Func<Task> action) where T : Exception
{
    try { await action(); }
    catch (T error) { checks++; return error; }
    throw new Exception($"Expected {typeof(T).Name}.");
}

var cardJson = "```json\n{\"name\":\" Công ty Ánh Dương \",\"phone\":\"0900123456\",\"taxCode\":null,\"customerType\":\"company\",\"address\":\" 12 Nguyễn Huệ \"}\n```";
using var successHttp = new HttpClient(new FakeHandler(async (request, token) =>
{
    Check(request.RequestUri?.AbsoluteUri == "https://integrate.api.nvidia.com/v1/chat/completions", "Wrong NVIDIA endpoint.");
    Check(request.Headers.Authorization?.ToString() == "Bearer test-only-key", "Missing bearer key.");
    using var payload = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
    Check(payload.RootElement.GetProperty("model").GetString() == "meta/llama-3.2-11b-vision-instruct", "Wrong model.");
    var parts = payload.RootElement.GetProperty("messages")[0].GetProperty("content");
    Check(parts[1].GetProperty("image_url").GetProperty("url").GetString() == "data:image/png;base64," + Convert.ToBase64String(image), "Wrong image payload.");
    Check(!payload.RootElement.TryGetProperty("chat_template_kwargs", out _), "Nemotron parameter leaked into Llama request.");
    return Response(Envelope(cardJson));
}));
var reader = new CustomerBusinessCardReader(successHttp, config);
var info = await reader.ReadAsync(image, "image/png");
Check(info.Name == "Công ty Ánh Dương" && info.Address == "12 Nguyễn Huệ", "Accents or whitespace were lost.");
Check(info.Phone == "0900123456" && info.TaxCode is null && info.CustomerType == "Company", "Incorrect field mapping.");
await Throws<ArgumentException>(() => reader.ReadAsync(Encoding.UTF8.GetBytes("not an image"), "image/png"));
await Throws<ArgumentException>(() => reader.ReadAsync(image, "image/jpeg"));
await Throws<ArgumentException>(() => reader.ReadAsync(new byte[10 * 1024 * 1024 + 1], "image/png"));

foreach (var body in new[] { Envelope("{}"), "{\"choices\":[]}", Envelope("{\"Name\":\"A\"}", "length") })
{
    using var http = new HttpClient(new FakeHandler((_, _) => Task.FromResult(Response(body))));
    await Throws<InvalidOperationException>(() => new CustomerBusinessCardReader(http, config).ReadAsync(image, "image/png"));
}
using (var http = new HttpClient(new FakeHandler((_, _) => Task.FromResult(Response("{\"choices\":")))))
    await Throws<JsonException>(() => new CustomerBusinessCardReader(http, config).ReadAsync(image, "image/png"));
using (var http = new HttpClient(new FakeHandler((_, _) => Task.FromResult(Response("secret upstream body", HttpStatusCode.Unauthorized)))))
{
    var error = await Throws<HttpRequestException>(() => new CustomerBusinessCardReader(http, config).ReadAsync(image, "image/png"));
    Check(error.StatusCode == HttpStatusCode.Unauthorized && !error.Message.Contains("secret upstream body"), "Raw provider body was exposed.");
}
var missingConfig = new ConfigurationBuilder().Build();
using var unconfiguredHttp = new HttpClient(new FakeHandler((_, _) => throw new Exception("Missing configuration must prevent sending an image.")));
var unconfiguredReader = new CustomerBusinessCardReader(unconfiguredHttp, missingConfig);
await Throws<InvalidOperationException>(() => unconfiguredReader.ReadAsync(image, "image/png"));
using (var http = new HttpClient(new FakeHandler((_, _) => Task.FromResult(Response("Page not found", HttpStatusCode.NotFound)))))
{
    var error = await Throws<HttpRequestException>(() => new CustomerBusinessCardReader(http, config).ReadAsync(image, "image/png"));
    Check(error.StatusCode == HttpStatusCode.NotFound && error.Message.Contains("meta/llama-3.2-11b-vision-instruct"), "Provider 404 must identify the configured model.");
}

using var imageStream = new MemoryStream(image);
var file = new FormFile(imageStream, 0, image.Length, "image", "card.png")
{
    Headers = new HeaderDictionary(), ContentType = "image/png"
};
var controller = new CustomersController(null!, new TimeoutReader());
var timeout = await controller.ReadBusinessCard(file, CancellationToken.None);
Check(timeout.Result is ObjectResult { StatusCode: 504 }, "Timeout should map to HTTP 504.");
using var cancellation = new CancellationTokenSource();
cancellation.Cancel();
await Throws<OperationCanceledException>(() => controller.ReadBusinessCard(file, cancellation.Token));
Console.WriteLine($"Passed {checks} customer business-card checks (no external API calls).");

sealed class FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => send(request, token);
}
sealed class TimeoutReader : ICustomerBusinessCardReader
{
    public Task<CustomerBusinessCardInfo> ReadAsync(byte[] image, string contentType, CancellationToken cancellationToken = default)
        => throw new TaskCanceledException("Simulated NVIDIA timeout.");
}
