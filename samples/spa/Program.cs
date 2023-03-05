using Microsoft.Net.Http.Headers;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();

var stateCheck = Guid.NewGuid().ToString();

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

app.MapGet("/callback", ([Microsoft.AspNetCore.Mvc.FromServices] IWebHostEnvironment webHostEnvironment) =>
{
    var callbackPageFilepath = Path.Combine(webHostEnvironment.ContentRootPath, "callback.html");
    var callbackPage = File.ReadAllText(callbackPageFilepath)
        .Replace("{{client_id}}", app.Configuration["myID:client_id"])
        .Replace("{{redirect_uri}}", app.Configuration["myID:redirect_uri"]);

    return Results.Content(callbackPage, new MediaTypeHeaderValue(MediaTypeNames.Text.Html));
});

app.Run();