using NodaTime;
using NodaTime.Extensions;

namespace Common.Lang.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Get datetime from Unix timestamp
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static Instant AsUnixTimestampMs(this long self)
    {
        return Instant.FromUnixTimeMilliseconds(self);
    }

    /// <summary>
    /// Get Unix timestamp from datetime.
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static long ToUnixTimestampMs(this Instant self)
    {
        return self.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Convert dotnet DateTime to ZonedDateTime.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="tz"></param>
    /// <param name="fromTz"></param>
    /// <returns>ZonedDateTime</returns>
    public static ZonedDateTime ToTimeZone(this DateTime self, string tz, string fromTz = "Etc/GMT")
    {
        return self.ToLocalDateTime()
                   .ToZonedDateTime(fromTz)
                   .WithZone(DateTimeZoneProviders.Tzdb[tz]);
    }

    /// <summary>
    /// Change timezone.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="tz"></param>
    /// <returns>ZonedDateTime</returns>
    public static ZonedDateTime ToTimeZone(this ZonedDateTime self, string tz)
    {
        return self.WithZone(DateTimeZoneProviders.Tzdb[tz]);
    }

    /// <summary>
    /// Convert LocalDateTime to ZonedDateTime
    /// </summary>
    /// <param name="self"></param>
    /// <param name="tz"></param>
    /// <returns>ZonedDateTime</returns>
    public static ZonedDateTime ToZonedDateTime(this LocalDateTime self, string tz)
    {
        return DateTimeZoneProviders.Tzdb[tz].AtStrictly(self);
    }
}