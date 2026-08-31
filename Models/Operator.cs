namespace TireTraceabilityDemo.Models;

public class Operator
{
    public int Id { get; set; }

    // Username untuk login
    public string Username { get; set; } = string.Empty;

    // Password dummy untuk sementara
    public string Password { get; set; } = string.Empty;

    // Nama asli operator
    public string Name { get; set; } = string.Empty;

    // Status akun
    public bool IsActive { get; set; } = true;

    // Station / role operator
    // Building, Curing, Inspection
    public string Role { get; set; } = "Building";
}