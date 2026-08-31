using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Data;
using TireTraceabilityDemo.Models;
using DaishaModel = TireTraceabilityDemo.Models.Daisha;

namespace TireTraceabilityDemo.Pages.Daisha;

public class CreateModel : PageModel
{
    // =========================================================
    // DATABASE
    // =========================================================

    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // FORM
    // =========================================================

    [BindProperty]
    public int TireCount { get; set; }


    // =========================================================
    // INFORMATION
    // =========================================================

    public string ComputerName { get; set; } = string.Empty;

    public string OperatorName { get; set; } = "Operator";


    // =========================================================
    // AVAILABLE TIRE
    // =========================================================

    public int AvailableTireCount { get; set; }


    // =========================================================
    // GET
    // =========================================================

    public async Task<IActionResult> OnGetAsync()
    {
        LoadInformation();

        AvailableTireCount =
            await GetAvailableTireCountAsync();

        return Page();
    }


    // =========================================================
    // LOAD INFORMATION
    // =========================================================

    private void LoadInformation()
    {
        // -----------------------------------------------------
        // COMPUTER / LAPTOP
        // -----------------------------------------------------

        ComputerName =
            Environment.MachineName;


        // -----------------------------------------------------
        // OPERATOR
        // -----------------------------------------------------

        OperatorName =
            HttpContext.Session.GetString(
                "BuildingOperatorName")
            ?? "Operator";
    }


    // =========================================================
    // GET AVAILABLE TIRES
    // =========================================================
    //
    // Tire yang sudah masuk Daisha tidak boleh dipilih lagi.
    //
    // IMPORTANT:
    // Menggunakan FIFO:
    //
    // Tire paling lama dibuat
    // akan dipilih terlebih dahulu.
    //
    // Contoh:
    //
    // Tire A - 10:00
    // Tire B - 10:05
    // Tire C - 10:10
    //
    // Jika TireCount = 2:
    //
    // A + B
    //
    // BUKAN:
    //
    // C + B
    //
    // =========================================================

    private async Task<List<Tire>> GetAvailableTiresAsync()
    {
        // -----------------------------------------------------
        // Ambil Tire ID yang sudah masuk Daisha
        // -----------------------------------------------------

        var assignedTireIds =
            await _context.DaishaTires
                .AsNoTracking()
                .Select(x => x.TireId)
                .ToListAsync();


        // -----------------------------------------------------
        // Ambil Tire yang belum pernah masuk Daisha
        //
        // ORDER BY CreatedAt ASC
        // = Tire paling lama terlebih dahulu
        // -----------------------------------------------------

        var tires =
            await _context.Tires
                .AsNoTracking()
                .Where(t =>
                    !assignedTireIds.Contains(t.Id))
                .OrderBy(t => t.CreatedAt)
                .ThenBy(t => t.Id)
                .ToListAsync();


        return tires;
    }


    // =========================================================
    // COUNT AVAILABLE TIRES
    // =========================================================

    private async Task<int> GetAvailableTireCountAsync()
    {
        // -----------------------------------------------------
        // Query Tire yang belum masuk Daisha
        // -----------------------------------------------------

        var assignedTireIds =
            _context.DaishaTires
                .AsNoTracking()
                .Select(x => x.TireId);


        return await _context.Tires
            .AsNoTracking()
            .Where(t =>
                !assignedTireIds.Contains(t.Id))
            .CountAsync();
    }


    // =========================================================
    // GENERATE DAISHA CODE
    // =========================================================
    //
    // Format:
    //
    // DS-20260830-001
    // DS-20260830-002
    // DS-20260830-003
    //
    // =========================================================

    private async Task<string> GenerateDaishaCodeAsync()
    {
        string date =
            DateTime.Now.ToString("yyyyMMdd");


        string prefix =
            $"DS-{date}-";


        // -----------------------------------------------------
        // Cari seluruh kode Daisha hari ini
        // -----------------------------------------------------

        var existingCodes =
            await _context.Daishas
                .AsNoTracking()
                .Where(x =>
                    x.DaishaCode.StartsWith(prefix))
                .Select(x =>
                    x.DaishaCode)
                .ToListAsync();


        int nextNumber = 1;


        // -----------------------------------------------------
        // Cari nomor terbesar
        // -----------------------------------------------------

        foreach (string code in existingCodes)
        {
            string[] parts =
                code.Split('-');


            if (parts.Length != 3)
            {
                continue;
            }


            if (int.TryParse(
                parts[2],
                out int number))
            {
                if (number >= nextNumber)
                {
                    nextNumber =
                        number + 1;
                }
            }
        }


        // -----------------------------------------------------
        // Buat kode pertama
        // -----------------------------------------------------

        string daishaCode =
            $"{prefix}{nextNumber:D3}";


        // -----------------------------------------------------
        // Safety check
        // -----------------------------------------------------

        while (await _context.Daishas.AnyAsync(
            x => x.DaishaCode == daishaCode))
        {
            nextNumber++;


            daishaCode =
                $"{prefix}{nextNumber:D3}";
        }


        return daishaCode;
    }


    // =========================================================
    // POST CREATE
    // =========================================================

    public async Task<IActionResult> OnPostAsync()
    {
        // =====================================================
        // LOAD INFORMATION
        // =====================================================

        LoadInformation();


        // =====================================================
        // VALIDASI LOGIN
        // =====================================================

        if (string.IsNullOrWhiteSpace(OperatorName) ||
            OperatorName == "Operator")
        {
            TempData["Error"] =
                "Operator Building belum login.";

            return RedirectToPage(
                "/Building/Login");
        }


        // =====================================================
        // VALIDASI JUMLAH TIRE
        // =====================================================

        int[] validCounts =
        {
            4,
            6,
            8,
            10,
            12,
            14,
            16
        };


        if (!validCounts.Contains(TireCount))
        {
            TempData["Error"] =
                "Jumlah tire harus 4, 6, 8, 10, 12, 14, atau 16.";

            return RedirectToPage();
        }


        // =====================================================
        // AMBIL TIRE YANG BELUM MASUK DAISHA
        // =====================================================

        var availableTires =
            await GetAvailableTiresAsync();


        // =====================================================
        // VALIDASI JUMLAH TIRE
        // =====================================================

        if (availableTires.Count < TireCount)
        {
            TempData["Error"] =
                $"Tire yang tersedia hanya " +
                $"{availableTires.Count} buah. " +
                $"Tidak cukup untuk membuat Daisha " +
                $"dengan {TireCount} tire.";

            return RedirectToPage();
        }


        // =====================================================
        // PILIH TIRE DENGAN FIFO
        // =====================================================
        //
        // GetAvailableTiresAsync() sudah mengurutkan:
        //
        // CreatedAt ASC
        //
        // Jadi:
        //
        // Take(TireCount)
        //
        // mengambil Tire PALING LAMA.
        //
        // =====================================================

        var selectedTires =
            availableTires
                .Take(TireCount)
                .ToList();


        // =====================================================
        // SAFETY CHECK
        // =====================================================

        if (selectedTires.Count != TireCount)
        {
            TempData["Error"] =
                "Jumlah Tire yang dipilih tidak sesuai.";

            return RedirectToPage();
        }


        // =====================================================
        // GENERATE DAISHA ID
        // =====================================================

        string daishaCode =
            await GenerateDaishaCodeAsync();


        // =====================================================
        // CREATED TIME
        // =====================================================

        DateTime createdAt =
            DateTime.Now;


        // =====================================================
        // CREATE DAISHA
        // =====================================================

        var daisha =
            new DaishaModel
            {
                DaishaCode =
                    daishaCode,

                CreatedAt =
                    createdAt
            };


        // =====================================================
        // SET PROPERTY TAMBAHAN
        // =====================================================
        //
        // Model Daisha kamu memiliki:
        //
        // ComputerName
        // OperatorName
        // TotalTires
        // Status
        //
        // Kita isi menggunakan helper supaya tetap aman.
        //
        // =====================================================

        SetPropertyIfExists(
            daisha,
            "ComputerName",
            ComputerName);


        SetPropertyIfExists(
            daisha,
            "OperatorName",
            OperatorName);


        SetPropertyIfExists(
            daisha,
            "OperatorId",
            OperatorName);


        SetPropertyIfExists(
            daisha,
            "TotalTires",
            selectedTires.Count);


        SetPropertyIfExists(
            daisha,
            "Status",
            "READY");


        // =====================================================
        // SIMPAN DAISHA
        // =====================================================

        _context.Daishas.Add(daisha);

        await _context.SaveChangesAsync();


        // =====================================================
        // BUAT RELASI DAISHA - TIRE
        // =====================================================
        //
        // Sequence:
        //
        // Tire pertama = 1
        // Tire kedua   = 2
        // Tire ketiga  = 3
        // dst.
        //
        // =====================================================

        int sequence = 1;


        foreach (var tire in selectedTires)
        {
            var daishaTire =
                new DaishaTire
                {
                    DaishaId =
                        daisha.Id,

                    TireId =
                        tire.Id,

                    Sequence =
                        sequence
                };


            _context.DaishaTires.Add(
                daishaTire);


            sequence++;
        }


        // =====================================================
        // SIMPAN RELASI
        // =====================================================

        await _context.SaveChangesAsync();


        // =====================================================
        // SUCCESS
        // =====================================================

        TempData["Success"] =
            $"Daisha {daisha.DaishaCode} berhasil dibuat " +
            $"dengan {selectedTires.Count} tire.";


        // =====================================================
        // REDIRECT KE DETAILS
        // =====================================================

        return RedirectToPage(
            "/Daisha/Details",
            new
            {
                id = daisha.Id
            });
    }


    // =========================================================
    // REFLECTION HELPER
    // =========================================================
    //
    // Digunakan untuk property optional pada model Daisha.
    //
    // =========================================================

    private static void SetPropertyIfExists(
        object target,
        string propertyName,
        object value)
    {
        var property =
            target.GetType().GetProperty(
                propertyName);


        // -----------------------------------------------------
        // Property tidak ada
        // -----------------------------------------------------

        if (property == null)
        {
            return;
        }


        // -----------------------------------------------------
        // Property readonly
        // -----------------------------------------------------

        if (!property.CanWrite)
        {
            return;
        }


        try
        {
            // -------------------------------------------------
            // STRING
            // -------------------------------------------------

            if (property.PropertyType ==
                typeof(string))
            {
                property.SetValue(
                    target,
                    value?.ToString() ?? string.Empty);

                return;
            }


            // -------------------------------------------------
            // TIPE SAMA
            // -------------------------------------------------

            if (value != null &&
                property.PropertyType ==
                value.GetType())
            {
                property.SetValue(
                    target,
                    value);

                return;
            }


            // -------------------------------------------------
            // CONVERT
            // -------------------------------------------------

            if (value != null)
            {
                var convertedValue =
                    Convert.ChangeType(
                        value,
                        property.PropertyType);


                property.SetValue(
                    target,
                    convertedValue);
            }
        }
        catch
        {
            // -------------------------------------------------
            // Property tambahan tidak boleh menggagalkan
            // proses Create Daisha.
            // -------------------------------------------------
        }
    }
}