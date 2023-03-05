using IdentityModel.Client;
using System.Net.Http.Headers;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();

var stateCheck = Guid.NewGuid().ToString();
var authenticationMethod = app.Configuration["myID:auth:authentication_method"] switch
{
    "client_secret_post" => ClientCredentialStyle.PostBody,
    "client_secret_basic" => ClientCredentialStyle.AuthorizationHeader,
    "private_key_jwt" => throw new NotImplementedException(),
    _ => throw new ArgumentException("myID:authentication_method")
};

app.MapGet("/start", ([Microsoft.AspNetCore.Mvc.FromServices] IWebHostEnvironment webHostEnvironment) =>
{
    var startPageFilepath = Path.Combine(webHostEnvironment.ContentRootPath, "start.html");
    var startPage = File.ReadAllText(startPageFilepath);

    return Results.Content(startPage, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(MediaTypeNames.Text.Html));
});

app.MapGet("/sign", async () =>
{
    var authClient = new HttpClient { BaseAddress = new Uri(app.Configuration["myID:auth:baseAddress"]) };
    var response = await authClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
    {
        Address = "token",

        ClientId = app.Configuration["myID:auth:client_id"],
        ClientSecret = app.Configuration["myID:auth:client_secret"],
        ClientCredentialStyle = authenticationMethod,

        Scope = "request_signature"
    });

    var signClient = new HttpClient { BaseAddress = new Uri(app.Configuration["myID:sign:baseAddress"]) };
    signClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response.AccessToken);

    var signResponse = await authClient.PostAsync("sign", null);
    var signUrl = await signResponse.Content.ReadAsStringAsync();

    return Results.Content(signUrl);
});

app.MapGet("/callback", async (string code, string state) =>
{
    throw new NotImplementedException("//TODO");

    if (state != stateCheck)
        return Results.Unauthorized();

    var client = new HttpClient { BaseAddress = new Uri(app.Configuration["myID:baseAddress"]) };
    var response = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
    {
        Address = "token",

        ClientId = app.Configuration["myID:client_id"],
        ClientSecret = app.Configuration["myID:client_secret"],
        ClientCredentialStyle = authenticationMethod,

        Code = code,
        RedirectUri = app.Configuration["myID:redirect_uri"]
    });

    var userInfo = await client.GetUserInfoAsync(new UserInfoRequest
    {
        Address = "userinfo",
        Token = response.AccessToken
    });

    return Results.Ok(userInfo);

    //var signResult = await client.GetStringAsync("https://sign.myid.be/api/retrievePending");
});

app.Run();