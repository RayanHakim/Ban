namespace TireTraceabilityDemo.Models;

public class Tire
{
    public int Id { get; set; }

    public string Barcode { get; set; } = string.Empty;

    public string OperatorId { get; set; } = string.Empty;

    public string MachineNumber { get; set; } = string.Empty;

    public string TireSize { get; set; } = string.Empty;

    public string Shift { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}