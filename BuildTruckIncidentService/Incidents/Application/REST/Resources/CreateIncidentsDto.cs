using System;
using Microsoft.AspNetCore.Http;

namespace BuildTruckIncidentService.Incidents.Application.REST.Resources;

public class CreateIncidentDto
{
    public int? ProjectId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string IncidentType { get; set; }
    public string Severity { get; set; }
    public string Status { get; set; }
    public string Location { get; set; }
    public string? ReportedBy { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime OccurredAt { get; set; }
    public IFormFile? Image { get; set; }
    public string Notes { get; set; }
}