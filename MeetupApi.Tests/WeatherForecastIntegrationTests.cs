using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MeetupApi.Tests;

public class WeatherForecastIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsOk()
    {
        var response = await _client.GetAsync("/weatherforecast");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsFiveItems()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");
        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Length);
    }

    [Fact]
    public async Task GetWeatherForecast_EachItemHasValidDate()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");
        Assert.NotNull(forecasts);
        var today = DateOnly.FromDateTime(DateTime.Today);
        foreach (var forecast in forecasts)
        {
            Assert.True(forecast.Date > today);
        }
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync("/weatherforecast");
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}
