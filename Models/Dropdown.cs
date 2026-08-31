namespace TireTraceabilityDemo.Models;

public class Dropdown
{
    public int Id { get; set; }

    // Kategori dropdown
    // Contoh: TireSize, DaishaCapacity
    public string Category { get; set; } = string.Empty;

    // Nilai yang ditampilkan pada dropdown
    // Contoh: 185/65R15, 195/65R15, 4, 12, 16
    public string Value { get; set; } = string.Empty;

    // Menentukan apakah pilihan masih aktif
    public bool IsActive { get; set; } = true;
}