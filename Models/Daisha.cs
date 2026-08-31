using System.ComponentModel.DataAnnotations;

namespace TireTraceabilityDemo.Models;

public class Daisha
{
    public int Id { get; set; }

    // =========================================================
    // IDENTITAS DAISHA
    // =========================================================

    [Required]
    public string DaishaCode { get; set; } = string.Empty;


    // =========================================================
    // KOMPUTER / LAPTOP YANG MEMBUAT DAISHA
    // =========================================================

    public string ComputerName { get; set; } = string.Empty;


    // =========================================================
    // OPERATOR YANG MEMBUAT DAISHA
    // =========================================================

    public string OperatorName { get; set; } = string.Empty;


    // =========================================================
    // JUMLAH TIRE DALAM DAISHA
    // =========================================================

    public int TotalTires { get; set; }


    // =========================================================
    // WAKTU PEMBUATAN
    // =========================================================

    public DateTime CreatedAt { get; set; }


    // =========================================================
    // STATUS DAISHA
    // =========================================================
    //
    // Contoh:
    // READY_FOR_CURING
    // IN_CURING
    // COMPLETED
    //
    // =========================================================

    public string Status { get; set; } = "READY_FOR_CURING";


    // =========================================================
    // RELASI KE TIRE
    // =========================================================
    //
    // Satu Daisha memiliki banyak Tire
    //
    // =========================================================

    public ICollection<DaishaTire> DaishaTires { get; set; }
        = new List<DaishaTire>();
}