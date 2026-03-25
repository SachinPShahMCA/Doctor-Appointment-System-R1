using NodaTime;

namespace DocApp.Application.Common.Interfaces;

public interface IDateTimeZoneService
{
    /// <summary>
    /// Converts a local datetime string + IANA timezone to UTC Instant.
    /// Throws InvalidTimezoneException if timezone invalid.
    /// Throws SkippedTimeException if time falls in DST spring-forward gap.
    /// Throws AmbiguousTimeException if time is ambiguous in DST fall-back.
    /// </summary>
    Instant ConvertToUtc(LocalDateTime localDateTime, string ianaTimezoneId);

    /// <summary>Converts a UTC Instant to a DateTimeOffset in the given IANA timezone.</summary>
    DateTimeOffset ConvertFromUtc(Instant utcInstant, string ianaTimezoneId);

    /// <summary>Returns true if the given IANA timezone ID is valid.</summary>
    bool IsValidTimezone(string ianaTimezoneId);

    /// <summary>Gets the current UTC instant.</summary>
    Instant GetUtcNow();
}
