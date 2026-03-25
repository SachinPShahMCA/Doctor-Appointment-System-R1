using DocApp.Application.Patients.DTOs;
using MediatR;

namespace DocApp.Application.Patients.Queries.GetPatients;

public sealed record GetPatientsQuery(string? SearchTerm = null) : IRequest<List<PatientDto>>;
