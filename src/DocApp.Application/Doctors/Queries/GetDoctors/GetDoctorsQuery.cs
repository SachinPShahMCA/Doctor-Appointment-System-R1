using DocApp.Application.Doctors.DTOs;
using MediatR;

namespace DocApp.Application.Doctors.Queries.GetDoctors;

public sealed record GetDoctorsQuery(string? Specialty = null) : IRequest<List<DoctorDto>>;
