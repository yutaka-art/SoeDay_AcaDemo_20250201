using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WebApp.Models;

namespace WebApp.Pages
{
    public class DaprModel : PageModel
    {
        private readonly DaprClient _daprClient;

        public DaprModel(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public async Task OnGet()
        {
            var url = "http://dapr-backend/WeatherForecast";
            var httpClient = DaprClient.CreateInvokeHttpClient();
            var json = await httpClient.GetStringAsync(url);
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var forecasts = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(json, options);

            ViewData["WeatherForecastData"] = forecasts;
        }
    }
}
