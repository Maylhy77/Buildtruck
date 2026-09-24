namespace BuildTruckIncidentService.Incidents.Domain.Model.Commands;
        
        public record UpdateIncidentCommand(
            int Id,
            int? ProjectId, // <-- Agrega esta línea
            string Title,
            string Description,
            string IncidentType,
            string Severity,
            string Status,
            string Location,
            string? ReportedBy,
            string? AssignedTo,
            DateTime OccurredAt,
            DateTime? ResolvedAt, // <-- Agrega esta línea
            string? Image,        // <-- Agrega esta línea
            string Notes,
            string? ImagePath
        );