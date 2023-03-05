using IdentityModel.Client;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();

var stateCheck = Guid.NewGuid().ToString();
var authenticationMethod = app.Configuration["myID:authentication_method"] switch
{
    "client_secret_post" => ClientCredentialStyle.PostBody,
    "client_secret_basic" => ClientCredentialStyle.AuthorizationHeader,
    "private_key_jwt" => throw new NotImplementedException(),
    _ => throw new ArgumentException("myID:authentication_method")
};

app.MapGet("/login", ([Microsoft.AspNetCore.Mvc.FromServices] IWebHostEnvironment webHostEnvironment) =>
{
    var indexPageFilepath = Path.Combine(webHostEnvironment.ContentRootPath, "login.html");
    var indexPage = File.ReadAllText(indexPageFilepath)
        .Replace("{{client_id}}", app.Configuration["myID:client_id"])
        .Replace("{{redirect_uri}}", app.Configuration["myID:redirect_uri"])
        .Replace("{{state}}", stateCheck)
        .Replace("{{scope}}", app.Configuration["myID:scope"]);
    
    return Results.Content(indexPage, new MediaTypeHeaderValue(MediaTypeNames.Text.Html));
});

app.MapGet("/callback", async (string code, string state) =>
{
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
});

app.Run();