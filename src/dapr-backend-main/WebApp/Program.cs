var builder = WebApplication.CreateBuilder(args);

// ��Swagger ���g�������ꍇ
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ���J������ Swagger UI ��L���ɂ���ꍇ
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// �V�C�\��p�̃T�}���[
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Minimal API
// GET /weatherforecast
app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
{
    // 1�`5�����̃_�~�[�f�[�^�𐶐�
    var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = summaries[Random.Shared.Next(summaries.Length)]
    })
    .ToArray();

    // ���O���o�� (�ȗ���)
    logger.LogInformation("Weather forecast data fetched.");

    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi() // �� .NET 8 ���� Microsoft.AspNetCore.OpenApi �Q�Ǝ���Swagger�֓o�^����ꍇ
;

// https://localhost:7061/swagger/index.html
// https://localhost:7061/weatherforecast
app.Run();
