using Microsoft.AspNetCore.Mvc;

using News.Models.ViewModels;

using System.Text.Json.Nodes; // Required for using JsonNode
// Make sure to include the namespace where your ViewModel is located
public class WeatherViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        string apiUrl = "https://api.open-meteo.com/v1/forecast?latitude=40.71&longitude=-74.01&hourly=temperature_2m";

        using (var httpClient = new HttpClient())
        {
            try
            {
                var jsonResponse = await httpClient.GetStringAsync(apiUrl);
                var weatherData = JsonNode.Parse(jsonResponse);

                // JSON parsing and temperature finding logic remains the same
                var hourlyNode = weatherData["hourly"];
                var timeArray = hourlyNode["time"].AsArray();
                var tempArray = hourlyNode["temperature_2m"].AsArray();

                var now = DateTime.UtcNow;
                var currentHourString = now.ToString("yyyy-MM-dd'T'HH:00");

                int currentIndex = -1;
                for (int i = 0; i < timeArray.Count; i++)
                {
                    if (timeArray[i].GetValue<string>() == currentHourString)
                    {
                        currentIndex = i;
                        break;
                    }
                }

                int currentTemp = (currentIndex != -1)
                    ? (int)Math.Round(tempArray[currentIndex].GetValue<double>())
                    : 0;


                var viewModel = new WeatherViewModel
                {
                    Temperature = currentTemp,
                    City = "NEW YORK",
                    CurrentDate = DateTime.Now
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {

                var errorViewModel = new WeatherViewModel { Temperature = 0, City = "Error", CurrentDate = DateTime.Now };
                return View(errorViewModel);
            }
        }
    }
}