using DocApp.Application.Common.Interfaces;
using DocApp.Domain.Exceptions;
using NodaTime;
using NodaTime.TimeZones;

namespace DocApp.Infrastructure.Time;

/// <summary>
/// Production-grade DST-safe timezone service using NodaTime TZDB.
/// </summary>
public sealed class DateTimeZoneService : IDateTimeZoneService
{
    private readonly IDateTimeZoneProvider _provider;

    public DateTimeZoneService()
    {
        _provider = DateTimeZoneProviders.Tzdb;
    }

    /// <summary>
    /// Converts a LocalDateTime (no tz info) to UTC Instant using the given IANA timezone.
    /// 
    /// DST Edge Cases handled:
    /// - Spring-forward gap (e.g., 2:30 AM doesn't exist): throws SkippedTimeException
    /// - Fall-back ambiguity (e.g., 1:30 AM occurs twice): throws AmbiguousTimeException
    ///   (strict mode — client must send unambiguous time)
    /// </summary>
    public Instant ConvertToUtc(LocalDateTime localDateTime, string ianaTimezoneId)
    {
        var zone = GetZone(ianaTimezoneId);

        try
        {
            // InZoneStrictly throws SkippedTimeException and AmbiguousTimeException
            return localDateTime.InZoneStrictly(zone).ToInstant();
        }
        catch (NodaTime.SkippedTimeException)
        {
            throw new Domain.Exceptions.SkippedTimeException(
                localDateTime.ToString(), ianaTimezoneId);
        }
        catch (NodaTime.AmbiguousTimeException)
        {
            throw new Domain.Exceptions.AmbiguousTimeException(
                localDateTime.ToString(), ianaTimezoneId);
        }
    }

    /// <summary>Converts a UTC Instant to a DateTimeOffset in the specified IANA timezone.</summary>
    public DateTimeOffset ConvertFromUtc(Instant utcInstant, string ianaTimezoneId)
    {
        var zone = GetZone(ianaTimezoneId);
        return utcInstant.InZone(zone).ToDateTimeOffset();
    }

    public bool IsValidTimezone(string ianaTimezoneId)
    {
        try { return _provider[ianaTimezoneId] is not null; }
        catch { return false; }
    }

    public Instant GetUtcNow() => SystemClock.Instance.GetCurrentInstant();

    private DateTimeZone GetZone(string ianaTimezoneId)
    {
        var zone = _provider.GetZoneOrNull(ianaTimezoneId);
        if (zone is null) throw new InvalidTimezoneException(ianaTimezoneId);
        return zone;
    }
}
