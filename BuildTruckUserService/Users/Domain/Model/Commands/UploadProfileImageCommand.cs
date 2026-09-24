namespace BuildTruckUserService.Users.Domain.Model.Commands;

/// <summary>
/// Command for uploading user profile image (creates new or replaces existing)
/// </summary>
/// <param name="UserId">The ID of the user</param>
/// <param name="ImageBytes">Image file bytes</param>
/// <param name="FileName">Original file name with extension</param>
public record UploadProfileImageCommand(
    int UserId,
    byte[] ImageBytes,
    string FileName);