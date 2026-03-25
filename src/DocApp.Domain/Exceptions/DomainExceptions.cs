namespace DocApp.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public class SlotUnavailableException : DomainException
{
    public SlotUnavailableException(string doctorName, string slot)
        : base($"The slot '{slot}' is not available for Dr. {doctorName}.") { }
}

public class InvalidTimezoneException : DomainException
{
    public InvalidTimezoneException(string timezoneId)
        : base($"The timezone '{timezoneId}' is not a valid IANA timezone identifier.") { }
}

public class AmbiguousTimeException : DomainException
{
    public AmbiguousTimeException(string localTime, string timezoneId)
        : base($"The local time '{localTime}' in timezone '{timezoneId}' is ambiguous due to DST. Please provide an explicit UTC offset.") { }
}

public class SkippedTimeException : DomainException
{
    public SkippedTimeException(string localTime, string timezoneId)
        : base($"The local time '{localTime}' in timezone '{timezoneId}' does not exist due to DST spring-forward.") { }
}

public class TenantNotFoundException : DomainException
{
    public TenantNotFoundException(string tenantId)
        : base($"Tenant '{tenantId}' was not found or is inactive.") { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object key)
        : base($"Entity '{entity}' with key '{key}' was not found.") { }
}
