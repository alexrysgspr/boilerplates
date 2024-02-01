// ReSharper disable CheckNamespace
namespace System;
// ReSharper restore CheckNamespace

public static class DateTimeExtensions
{
    public static string ToRowKey(this DateTime dateTime)
    {
        return $"{DateTime.MaxValue.Ticks - dateTime.ToDateTimeKindUtc().Ticks:D19}";
    }

    public static DateTime ToDateTimeKindUtc(this DateTime dateTime)
    {
        var dateTimeKindUtc = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour,
            dateTime.Minute, dateTime.Second, dateTime.Millisecond, DateTimeKind.Utc);

        return dateTimeKindUtc;
    }
}
