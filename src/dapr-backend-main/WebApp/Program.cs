var builder = WebApplication.CreateBuilder(args);

// ※Swagger を使いたい場合
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ※開発環境で Swagger UI を有効にする場合
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 天気予報用のサマリー
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Minimal API
// GET /weatherforecast
app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
{
    // 1～5日分のダミーデータを生成
    var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = summaries[Random.Shared.Next(summaries.Length)]
    })
    .ToArray();

    // ログを出力 (省略可)
    logger.LogInformation("Weather forecast data fetched.");

    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi() // ← .NET 8 かつ Microsoft.AspNetCore.OpenApi 参照時にSwaggerへ登録する場合
;

// https://localhost:7061/swagger/index.html
// https://localhost:7061/weatherforecast
app.Run();
