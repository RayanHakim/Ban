namespace TireTraceabilityDemo.Models;

public class Inspection
{
    public int Id { get; set; }

    // Barcode dari QR Code ban
    public string Barcode { get; set; } = string.Empty;

    public string TireSize { get; set; } = string.Empty;

    public string MoldNumber { get; set; } = string.Empty;

    public string OperatorId { get; set; } = string.Empty;

    public string DefectName { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public bool Rework { get; set; }

    public bool Strap { get; set; }

    public bool Hold { get; set; }

    public DateTime InspectionAt { get; set; }
}