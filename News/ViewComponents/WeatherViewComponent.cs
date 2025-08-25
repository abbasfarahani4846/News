using Microsoft.AspNetCore.Mvc;

using News.Models.ViewModels;

using System.Text.Json.Nodes;
public class WeatherViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Define the API URL for the weather forecast.
        string apiUrl = "https://api.open-meteo.com/v1/forecast?latitude=40.71&longitude=-74.01&hourly=temperature_2m";

        // In a real application, it's better to use IHttpClientFactory to manage HttpClient instances.
        using (var httpClient = new HttpClient())
        {
            try
            {
                // 1. Fetch the data from the weather API as a JSON string.
                var jsonResponse = await httpClient.GetStringAsync(apiUrl);
                var weatherData = JsonNode.Parse(jsonResponse);

                // 2. Extract the hourly time and temperature arrays from the JSON.
                var timeArray = weatherData["hourly"]["time"].AsArray();
                var tempArray = weatherData["hourly"]["temperature_2m"].AsArray();

                // 3. Find the temperature for the current hour.
                // Get the current time (UTC) and format it to match the API's format (e.g., "2025-08-25T07:00").
                var currentHourString = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:00");

                // Find the index of the current hour string in the time array.
                var timeList = timeArray.Select(t => t.GetValue<string>()).ToList();
                int currentIndex = timeList.FindIndex(t => t == currentHourString);

                int currentTemp = 0;
                // Check if the current hour was found in the list.
                if (currentIndex != -1)
                {
                    // If found, get the temperature at the same index and round it to the nearest integer.
                    currentTemp = (int)Math.Round(tempArray[currentIndex].GetValue<double>());
                }

                // 4. Create the ViewModel to pass the final data to the view.
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
                // If any error occurs (e.g., API is down), log the exception for debugging.
                Console.WriteLine($"Error fetching weather data: {ex.Message}");

                // Return the view with default error data.
                var errorViewModel = new WeatherViewModel { Temperature = 0, City = "Error", CurrentDate = DateTime.Now };
                return View(errorViewModel);
            }
        }
    }
}