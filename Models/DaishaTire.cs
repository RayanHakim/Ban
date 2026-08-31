using System.ComponentModel.DataAnnotations;

namespace TireTraceabilityDemo.Models;

public class DaishaTire
{
    public int Id { get; set; }


    // =========================================================
    // DAISHA
    // =========================================================

    [Required]
    public int DaishaId { get; set; }

    public Daisha? Daisha { get; set; }


    // =========================================================
    // TIRE
    // =========================================================

    [Required]
    public int TireId { get; set; }

    public Tire? Tire { get; set; }


    // =========================================================
    // URUTAN TIRE DI DALAM DAISHA
    // =========================================================
    //
    // Contoh:
    //
    // 1 = Tire pertama
    // 2 = Tire kedua
    // 3 = Tire ketiga
    //
    // sampai 16.
    //
    // =========================================================

    public int Sequence { get; set; }
}