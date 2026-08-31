namespace TireTraceabilityDemo.Models;

public class Curing
{
    public int Id { get; set; }

    public string Barcode { get; set; } = string.Empty;

    public string MoldNumber { get; set; } = string.Empty;

    public string OperatorId { get; set; } = string.Empty;

    public string Shift { get; set; } = string.Empty;

    public DateTime CuringAt { get; set; }
}