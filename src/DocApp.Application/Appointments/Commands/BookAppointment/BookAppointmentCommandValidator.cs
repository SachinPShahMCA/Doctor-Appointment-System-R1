using FluentValidation;
using NodaTime;

namespace DocApp.Application.Appointments.Commands.BookAppointment;

public sealed class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("Doctor ID is required.");

        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.ClientTimezoneId)
            .NotEmpty().WithMessage("Timezone is required.")
            .MaximumLength(100).WithMessage("Invalid timezone identifier.");

        RuleFor(x => x.StartTimeLocal)
            .Must(BeFutureDate).WithMessage("Appointment must be scheduled in the future.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).When(x => x.Notes is not null);
    }

    private static bool BeFutureDate(LocalDateTime localDateTime)
        => localDateTime > LocalDateTime.FromDateTime(DateTime.UtcNow);
}
