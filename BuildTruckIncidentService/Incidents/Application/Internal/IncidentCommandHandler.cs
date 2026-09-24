using System;
                    using System.IO;
                    using System.Threading.Tasks;
                    using BuildTruckIncidentService.Incidents.Application.ACL.Services;
                    using BuildTruckIncidentService.Incidents.Domain.Aggregates;
                    using BuildTruckIncidentService.Incidents.Domain.Model.Commands;
                    using BuildTruckIncidentService.Incidents.Domain.ValueObjects;
                    using BuildTruckIncidentService.Shared.Infrastructure.Persistence.EFC.Configuration;
                    
                    namespace BuildTruckIncidentService.Incidents.Application.Internal
                    {
                        public class IncidentCommandHandler : IIncidentCommandHandler
                        {
                            private readonly IncidentServiceDbContext _context;
                            private readonly ICloudinaryService _cloudinaryService;
                    
                            public IncidentCommandHandler(IncidentServiceDbContext context, ICloudinaryService cloudinaryService)
                            {
                                _context = context;
                                _cloudinaryService = cloudinaryService;
                            }
                    
                            public async Task<int> HandleAsync(CreateIncidentCommand command)
                            {
                                var incident = new Incident
                                {
                                    ProjectId = command.ProjectId,
                                    Title = command.Title,
                                    Description = command.Description,
                                    IncidentType = command.IncidentType,
                                    Severity = IncidentSeverityExtensions.FromString(command.Severity),
                                    Status = IncidentStatusExtensions.FromString(command.Status),
                                    Location = command.Location,
                                    ReportedBy = command.ReportedBy,
                                    AssignedTo = command.AssignedTo,
                                    OccurredAt = command.OccurredAt,
                                    Notes = command.Notes,
                                    RegisterDate = DateTime.UtcNow.AddHours(-5),
                                    UpdatedAt = DateTime.UtcNow.AddHours(-5)
                                };
                    
                                // Subir imagen si existe
                                if (!string.IsNullOrEmpty(command.ImagePath))
                                {
                                    var imageBytes = await File.ReadAllBytesAsync(command.ImagePath);
                                    var imageUrl = await _cloudinaryService.UploadIncidentImageAsync(
                                        imageBytes,
                                        Path.GetFileName(command.ImagePath),
                                        incident.Id
                                    );
                                    incident.Image = imageUrl;
                                }
                    
                                await _context.Incidents.AddAsync(incident);
                                await _context.SaveChangesAsync();
                    
                                return incident.Id;
                            }
                    
                            public async Task HandleAsync(UpdateIncidentCommand command)
                            {
                                var incident = await _context.Incidents.FindAsync(command.Id);
                                if (incident == null)
                                    throw new Exception($"No se encontró el incidente con Id {command.Id}");

                                incident.Title = command.Title;
                                incident.ProjectId = command.ProjectId;
                                incident.Description = command.Description;
                                incident.IncidentType = command.IncidentType;
                                incident.Severity = IncidentSeverityExtensions.FromString(command.Severity);
                                incident.Status = IncidentStatusExtensions.FromString(command.Status);
                                incident.Location = command.Location;
                                incident.ReportedBy = command.ReportedBy;
                                incident.AssignedTo = command.AssignedTo;
                                incident.OccurredAt = command.OccurredAt;
                                incident.ResolvedAt = command.ResolvedAt;
                                incident.Notes = command.Notes;
                                incident.UpdatedAt = DateTime.UtcNow.AddHours(-5);

                                // Si hay nueva imagen, elimina la anterior y sube la nueva
                                if (!string.IsNullOrEmpty(command.ImagePath))
                                {
                                    // Elimina la imagen anterior si existe
                                    if (!string.IsNullOrEmpty(incident.Image))
                                    {
                                        await _cloudinaryService.DeleteIncidentImageAsync(incident.Image);
                                    }

                                    var imageBytes = await File.ReadAllBytesAsync(command.ImagePath);
                                    var imageUrl = await _cloudinaryService.UploadIncidentImageAsync(
                                        imageBytes,
                                        Path.GetFileName(command.ImagePath),
                                        incident.Id
                                    );
                                    incident.Image = imageUrl;
                                }

                                await _context.SaveChangesAsync();
                            }
                            
                            public async Task DeleteAsync(int id)
                            {
                                var incident = await _context.Incidents.FindAsync(id);
                                if (incident == null)
                                    throw new Exception($"No se encontró el incidente con Id {id}");
                            
                                // Elimina la imagen de Cloudinary si existe
                                if (!string.IsNullOrEmpty(incident.Image))
                                    await _cloudinaryService.DeleteIncidentImageAsync(incident.Image);
                            
                                _context.Incidents.Remove(incident);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
