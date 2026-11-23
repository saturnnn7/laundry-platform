using Laundry.BuildingBlocks.Api;
using Laundry.Host.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiProblemDetails();
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();

app.UseApiProblemDetails();
app.MapGroup("/api/v1").MapEndpoints();

app.Run();

public partial class Program;
