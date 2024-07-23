var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHandlers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", void (
        [FromServices] ISender sender
    ) => sender.SendAsync(new GetWeatherForecastCommand()))
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();