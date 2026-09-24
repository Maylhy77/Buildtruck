namespace BuildTruckDocumentationService.Documentation.Interfaces.REST.Resources;

/// <summary>
/// Resource for delete documentation operation response
/// </summary>
public record DeleteDocumentationResource(
    bool Success,
    string Message
);