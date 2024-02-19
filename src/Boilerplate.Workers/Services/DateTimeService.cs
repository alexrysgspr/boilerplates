namespace Boilerplate.Workers.Services;

public interface IDateTimeService
{
    DateTime UtcDateTimeNow { get; }
    DateTime AetDateTimeNow { get; }
    TimeZoneInfo AetTimeZoneInfo { get; }
}

public class DateTimeService : IDateTimeService
{
    public const string AetTimeZoneId = "AUS Eastern Standard Time";
    public DateTime UtcDateTimeNow => DateTime.UtcNow;
    public TimeZoneInfo AetTimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById(AetTimeZoneId);

    public DateTime AetDateTimeNow
    {
        get
        {
            var utcDateTime = DateTime.UtcNow;
            var aetZone = TimeZoneInfo.FindSystemTimeZoneById(AetTimeZoneId);
            var aetTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, aetZone);

            return aetTime;
        }
    }
}