using NodaTime;

namespace DocApp.Domain.ValueObjects;

/// <summary>Immutable time slot value object with overlap detection.</summary>
public record TimeSlot(Instant Start, Instant End)
{
    public Duration Duration => End - Start;

    public bool Overlaps(TimeSlot other)
        => Start < other.End && End > other.Start;

    public bool Contains(Instant point)
        => point >= Start && point < End;

    public bool IsValid()
        => End > Start;
}
