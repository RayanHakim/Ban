namespace TireTraceabilityDemo.Models;

public class Admin
{
    public int Id { get; set; }

    // Username admin
    public string Username { get; set; } = string.Empty;

    // Password dummy untuk sementara
    public string Password { get; set; } = string.Empty;

    // Nama admin
    public string Name { get; set; } = string.Empty;

    // Status akun
    public bool IsActive { get; set; } = true;
}