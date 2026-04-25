namespace MeetupApi.Tests;

public class WeatherForecastTests
{
    [Fact]
    public void WeatherForecast_TemperatureF_ConvertsCorrectly()
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 0, "Freezing");
        Assert.Equal(32, forecast.TemperatureF);
    }

    [Fact]
    public void WeatherForecast_TemperatureF_ConvertsPositiveTemperature()
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 100, "Scorching");
        Assert.Equal(212, forecast.TemperatureF);
    }

    [Fact]
    public void WeatherForecast_Summary_CanBeNull()
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 20, null);
        Assert.Null(forecast.Summary);
    }

    [Theory]
    [InlineData(-20)]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(55)]
    public void WeatherForecast_TemperatureC_IsPreserved(int temperatureC)
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), temperatureC, "Mild");
        Assert.Equal(temperatureC, forecast.TemperatureC);
    }

    [Fact]
    public void WeatherForecast_Date_IsPreserved()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var forecast = new WeatherForecast(date, 15, "Cool");
        Assert.Equal(date, forecast.Date);
    }
}
