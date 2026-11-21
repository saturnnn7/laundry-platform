using Laundry.BuildingBlocks.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiProblemDetails();
builder.Services.AddEndpoints(typeof(Program).Assembly);

var app = builder.Build();

app.UseApiProblemDetails();
app.MapGroup("/api/v1").MapEndpoints();

app.Run();

public partial class Program;
