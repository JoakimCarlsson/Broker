var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient(typeof(IRequestPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddHandlers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", (
        [FromServices] ISender sender
    ) => sender.SendAsync(new GetWeatherForecastCommand()))
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapGet("/nothing", (
        [FromServices] ISender sender
        ) => sender.SendAsync(new GetNothingCommand()))
    .WithName("GetNothing")
    .WithOpenApi();

app.Run();