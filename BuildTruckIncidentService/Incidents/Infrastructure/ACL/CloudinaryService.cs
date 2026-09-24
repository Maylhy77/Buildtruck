using System;
                            using System.IO;
                            using System.Threading.Tasks;
                            using BuildTruckIncidentService.Incidents.Application.ACL.Services;
                            using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;
                            using Microsoft.Extensions.Logging;
                            
                            namespace BuildTruckIncidentService.Incidents.Infrastructure.ACL;
                            
                            public class CloudinaryService : ICloudinaryService
                            {
                                private readonly ICloudinaryImageService _cloudinaryImageService;
                                private readonly ILogger<CloudinaryService> _logger;
                            
                                // Carpeta correcta para incidentes en Cloudinary
                                private const string INCIDENTS_FOLDER = "buildtruck/incidents";
                            
                                public CloudinaryService(ICloudinaryImageService cloudinaryImageService, ILogger<CloudinaryService> logger)
                                {
                                    _cloudinaryImageService = cloudinaryImageService;
                                    _logger = logger;
                                }
                            
                                public async Task<string> UploadIncidentImageAsync(byte[] imageBytes, string fileName, int incidentId)
                                {
                                    try
                                    {
                                        _logger.LogDebug("Subiendo imagen de incidente: {FileName} para incidente: {IncidentId}", fileName, incidentId);
                            
                                        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                        var extension = Path.GetExtension(fileName);
                                        var uniqueFileName = $"incident_{incidentId}_{timestamp}{extension}";
                            
                                        // Subir a la carpeta buildtruck/incidents
                                        var imageUrl = await _cloudinaryImageService.UploadImageAsync(imageBytes, uniqueFileName, INCIDENTS_FOLDER);
                            
                                        _logger.LogInformation("✅ Imagen de incidente subida correctamente: {ImageUrl}", imageUrl);
                                        return imageUrl;
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "❌ Error al subir imagen de incidente para: {IncidentId}", incidentId);
                                        throw new InvalidOperationException($"Error al subir imagen de incidente: {ex.Message}", ex);
                                    }
                                }
                            
                                public async Task<string> UploadImageAsync(string imagePath)
                                {
                                    throw new NotImplementedException("Usa UploadIncidentImageAsync para subir imágenes de incidentes.");
                                }
                            
                                public async Task<bool> DeleteIncidentImageAsync(string imageUrl)
                                {
                                    try
                                    {
                                        if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
                                        {
                                            _logger.LogWarning("URL de imagen inválida para eliminar: {ImageUrl}", imageUrl);
                                            return false;
                                        }
                            
                                        var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(imageUrl);
                                        if (string.IsNullOrEmpty(publicId))
                                        {
                                            _logger.LogWarning("No se pudo extraer el public ID de la URL: {ImageUrl}", imageUrl);
                                            return false;
                                        }
                            
                                        var success = await _cloudinaryImageService.DeleteImageAsync(publicId);
                            
                                        if (success)
                                            _logger.LogInformation("✅ Imagen de incidente eliminada correctamente: {ImageUrl}", imageUrl);
                                        else
                                            _logger.LogWarning("⚠️ No se pudo eliminar la imagen de incidente: {ImageUrl}", imageUrl);
                            
                                        return success;
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "❌ Error al eliminar imagen de incidente: {ImageUrl}", imageUrl);
                                        return false;
                                    }
                                }
                            
                                public string GetOptimizedIncidentImageUrl(string imageUrl, int width = 400, int height = 400)
                                {
                                    try
                                    {
                                        if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
                                            return imageUrl;
                            
                                        var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(imageUrl);
                                        if (string.IsNullOrEmpty(publicId))
                                            return imageUrl;
                            
                                        var optimizedUrl = _cloudinaryImageService.GetOptimizedImageUrl(publicId, width, height);
                            
                                        _logger.LogDebug("URL optimizada generada para imagen de incidente: {OptimizedUrl}", optimizedUrl);
                                        return optimizedUrl;
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "❌ Error generando URL optimizada para: {ImageUrl}", imageUrl);
                                        return imageUrl;
                                    }
                                }
                            }