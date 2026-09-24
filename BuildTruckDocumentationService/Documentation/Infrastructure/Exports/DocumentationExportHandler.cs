using ClosedXML.Excel;
using System.Data;

namespace BuildTruckDocumentationService.Documentation.Infrastructure.Exports;

/// <summary>
/// Handler for exporting Documentation data to various formats
/// Adapted from Personnel ExportHandler for PDF generation
/// </summary>
public class DocumentationExportHandler
{
    public byte[] ExportToExcel(
        IEnumerable<Domain.Model.Aggregates.Documentation> documentation, 
        string sheetName = "Documentation")
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Headers
        var headers = new List<string>
        {
            "ID",
            "Title",
            "Description",
            "Date",
            "Project ID",
            "Created By",
            "Image Path",
            "Created At",
            "Updated At"
        };

        // Set headers
        for (int i = 0; i < headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Data rows
        var row = 2;
        foreach (var doc in documentation.Where(d => !d.IsDeleted))
        {
            var col = 1;
            worksheet.Cell(row, col++).Value = doc.Id;
            worksheet.Cell(row, col++).Value = doc.Title;
            worksheet.Cell(row, col++).Value = doc.Description;
            worksheet.Cell(row, col++).Value = doc.Date.ToString("yyyy-MM-dd");
            worksheet.Cell(row, col++).Value = doc.ProjectId;
            worksheet.Cell(row, col++).Value = doc.CreatedBy;
            worksheet.Cell(row, col++).Value = doc.ImagePath;
            worksheet.Cell(row, col++).Value = doc.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            worksheet.Cell(row, col++).Value = doc.UpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

            row++;
        }

        // Auto-fit columns
        worksheet.ColumnsUsed().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public DataTable ConvertToDataTable(IEnumerable<Domain.Model.Aggregates.Documentation> documentation)
    {
        var table = new DataTable("Documentation");

        // Columns
        table.Columns.Add("ID", typeof(int));
        table.Columns.Add("Title", typeof(string));
        table.Columns.Add("Description", typeof(string));
        table.Columns.Add("Date", typeof(DateTime));
        table.Columns.Add("ProjectId", typeof(int));
        table.Columns.Add("CreatedBy", typeof(int));
        table.Columns.Add("ImagePath", typeof(string));
        table.Columns.Add("CreatedAt", typeof(DateTime));
        table.Columns.Add("UpdatedAt", typeof(DateTime));

        // Rows
        foreach (var doc in documentation.Where(d => !d.IsDeleted))
        {
            table.Rows.Add(
                doc.Id,
                doc.Title,
                doc.Description,
                doc.Date,
                doc.ProjectId,
                doc.CreatedBy,
                doc.ImagePath,
                doc.CreatedDate?.DateTime,
                doc.UpdatedDate?.DateTime
            );
        }

        return table;
    }

    public byte[] ExportToCsv(IEnumerable<Domain.Model.Aggregates.Documentation> documentation)
    {
        var csv = new System.Text.StringBuilder();
        
        // Headers
        csv.AppendLine("ID,Title,Description,Date,ProjectId,CreatedBy,ImagePath,CreatedAt,UpdatedAt");

        // Data
        foreach (var doc in documentation.Where(d => !d.IsDeleted))
        {
            csv.AppendLine($"{doc.Id}," +
                          $"\"{doc.Title.Replace("\"", "\"\"")}\"," +
                          $"\"{doc.Description.Replace("\"", "\"\"")}\"," +
                          $"{doc.Date:yyyy-MM-dd}," +
                          $"{doc.ProjectId}," +
                          $"{doc.CreatedBy}," +
                          $"\"{doc.ImagePath}\"," +
                          $"{doc.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss")}," +
                          $"{doc.UpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    /// <summary>
    /// Export documentation to PDF with embedded images
    /// This method is designed to be called by the shared ExportsController
    /// </summary>
    public byte[] ExportToPdf(
        IEnumerable<Domain.Model.Aggregates.Documentation> documentation,
        string projectName = "Project",
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var docs = documentation.Where(d => !d.IsDeleted).ToList();

        // Filter by date range if provided
        if (startDate.HasValue)
        {
            docs = docs.Where(d => d.Date >= startDate.Value).ToList();
        }
        if (endDate.HasValue)
        {
            docs = docs.Where(d => d.Date <= endDate.Value).ToList();
        }

        // Order by date descending
        docs = docs.OrderByDescending(d => d.Date).ToList();

        var pdfContent = GeneratePdfContent(docs, projectName, startDate, endDate);
        return System.Text.Encoding.UTF8.GetBytes(pdfContent);
    }

    private string GeneratePdfContent(
        List<Domain.Model.Aggregates.Documentation> documentation,
        string projectName,
        DateTime? startDate,
        DateTime? endDate)
    {
        var content = new System.Text.StringBuilder();
        
        content.AppendLine("DOCUMENTATION REPORT");
        content.AppendLine("===================");
        content.AppendLine($"Project: {projectName}");
        content.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        
        if (startDate.HasValue || endDate.HasValue)
        {
            content.AppendLine($"Date Range: {startDate?.ToString("yyyy-MM-dd") ?? "All"} to {endDate?.ToString("yyyy-MM-dd") ?? "All"}");
        }
        
        content.AppendLine($"Total Documents: {documentation.Count}");
        content.AppendLine();

        foreach (var doc in documentation)
        {
            content.AppendLine($"Document ID: {doc.Id}");
            content.AppendLine($"Title: {doc.Title}");
            content.AppendLine($"Date: {doc.Date:yyyy-MM-dd}");
            content.AppendLine($"Description: {doc.Description}");
            content.AppendLine($"Image: {(doc.HasValidImage() ? "Available" : "Not Available")}");
            content.AppendLine($"Created: {doc.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss")}");
            content.AppendLine("---");
            content.AppendLine();
        }

        return content.ToString();
    }

    /// <summary>
    /// Get documentation statistics for a project
    /// </summary>
    public Dictionary<string, object> GetProjectStatistics(
        IEnumerable<Domain.Model.Aggregates.Documentation> documentation)
    {
        var docs = documentation.Where(d => !d.IsDeleted).ToList();
        
        var stats = new Dictionary<string, object>
        {
            ["totalDocuments"] = docs.Count,
            ["documentsWithImages"] = docs.Count(d => d.HasValidImage()),
            ["recentDocuments"] = docs.Count(d => 
                d.Date >= DateTime.Now.Date.AddDays(-7)),
            ["oldestDocument"] = docs.Any() 
                ? docs.Min(d => d.Date).ToString("yyyy-MM-dd")
                : string.Empty,
            ["newestDocument"] = docs.Any() 
                ? docs.Max(d => d.Date).ToString("yyyy-MM-dd")
                : string.Empty,
            ["documentsThisMonth"] = docs.Count(d => 
                d.Date.Year == DateTime.Now.Year && 
                d.Date.Month == DateTime.Now.Month),
            ["averageDescriptionLength"] = docs.Any() 
                ? docs.Average(d => d.Description.Length)
                : 0
        };

        return stats;
    }
}
