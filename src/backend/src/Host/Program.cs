using Laundry.BuildingBlocks.Api;
using Laundry.Host.Database;
using Laundry.Host.Health;
using Laundry.Host.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiProblemDetails();
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddHealth();
builder.Services.AddApiDocs();

var app = builder.Build();

app.UseApiProblemDetails();
app.MapHealth();
app.UseApiDocs();
app.MapGroup("/api/v1").MapEndpoints();

app.Run();

public partial class Program;
