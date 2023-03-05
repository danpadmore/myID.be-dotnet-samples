using IdentityModel.OidcClient;
using Microsoft.Extensions.Configuration;
using native;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.Development.json", optional: true)
    .Build();

var browserPort = new Uri(configuration["myID:redirect_uri"]).Port;
var browser = new SystemBrowser(browserPort);
var options = new OidcClientOptions
{
    Authority = configuration["myID:baseAddress"],
    ClientId = configuration["myID:client_id"],
    RedirectUri = configuration["myID:redirect_uri"],
    Scope = configuration["myID:scope"],
    FilterClaims = false,
    Browser = browser,
};

var oidcClient = new OidcClient(options);
var result = await oidcClient.LoginAsync(new LoginRequest());

Console.WriteLine($"{nameof(result.AccessToken)}: {result.AccessToken}");
Console.WriteLine($"{nameof(result.IdentityToken)}: {result.IdentityToken}");
Console.ReadLine();