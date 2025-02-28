using MauiHybridAuth.Shared.Services;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MauiHybridAuth.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly ICustomAuthenticationStateProvider _authenticationStateProvider;

        public WeatherService(ICustomAuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<WeatherForecast[]> GetWeatherForecastsAsync()
        {
            var forecasts = Array.Empty<WeatherForecast>();
            try
            {
                var httpClient = HttpClientHelper.GetHttpClient();
                var weatherUrl = HttpClientHelper.WeatherUrl;

                var loginToken = _authenticationStateProvider.AccessTokenInfo?.LoginToken;
                var token = loginToken?.AccessToken;
                var scheme = loginToken?.TokenType;

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(scheme))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
                    forecasts = await httpClient.GetFromJsonAsync<WeatherForecast[]>(weatherUrl);
                }
                else
                {
                    Debug.WriteLine("Token or scheme is null or empty.");                  
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"HTTP Request error: {httpEx.Message}");                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");                
            }
            return forecasts;
        }
    }
}
