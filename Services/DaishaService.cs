using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Data;
using TireTraceabilityDemo.Models;

namespace TireTraceabilityDemo.Services;

public class DaishaService
{
    private readonly AppDbContext _context;

    public DaishaService(AppDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GENERATE DAISHA CODE
    // =========================================================
    //
    // Format:
    //
    // DS-20260830-001
    //
    // DS
    // = Daisha
    //
    // 20260830
    // = tanggal pembuatan
    //
    // 001
    // = nomor urut pada tanggal tersebut
    //
    // =========================================================

    public async Task<string> GenerateDaishaCodeAsync()
    {
        string date =
            DateTime.Now.ToString("yyyyMMdd");

        string prefix =
            $"DS-{date}-";

        var lastDaisha = await _context.Daishas
            .Where(x => x.DaishaCode.StartsWith(prefix))
            .OrderByDescending(x => x.DaishaCode)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (lastDaisha != null)
        {
            string lastNumber =
                lastDaisha.DaishaCode
                    .Substring(prefix.Length);

            if (int.TryParse(lastNumber, out int parsedNumber))
            {
                nextNumber = parsedNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D3}";
    }


    // =========================================================
    // VALIDASI JUMLAH TIRE
    // =========================================================
    //
    // Jumlah yang diperbolehkan:
    //
    // 4, 6, 8, 10, 12, 14, 16
    //
    // =========================================================

    public bool IsValidTireCount(int totalTires)
    {
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

        return validCounts.Contains(totalTires);
    }


    // =========================================================
    // VALIDASI TIRE UNTUK DAISHA
    // =========================================================
    //
    // Memastikan:
    //
    // 1. Tire tersedia
    // 2. Tire belum masuk Daisha lain
    // 3. Tire sesuai jumlah yang dipilih
    //
    // =========================================================

    public async Task<(bool Success, string Message, List<Tire> Tires)>
        ValidateTiresAsync(List<string> barcodes)
    {
        if (barcodes == null || barcodes.Count == 0)
        {
            return (
                false,
                "Belum ada Tire yang dipilih.",
                new List<Tire>()
            );
        }


        // Buang barcode kosong
        barcodes = barcodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();


        // Cari Tire berdasarkan barcode
        var tires = await _context.Tires
            .Where(x => barcodes.Contains(x.Barcode))
            .ToListAsync();


        // =====================================================
        // CEK JUMLAH
        // =====================================================

        if (tires.Count != barcodes.Count)
        {
            return (
                false,
                "Ada Tire yang tidak ditemukan pada database.",
                tires
            );
        }


        // =====================================================
        // CEK APAKAH TIRE SUDAH ADA DI DAISHA
        // =====================================================

        var tireIds = tires
            .Select(x => x.Id)
            .ToList();


        var alreadyGrouped = await _context.DaishaTires
            .Where(x => tireIds.Contains(x.TireId))
            .Include(x => x.Daisha)
            .ToListAsync();


        if (alreadyGrouped.Count > 0)
        {
            var groupedBarcode =
                tires
                    .Where(t =>
                        alreadyGrouped.Any(
                            d => d.TireId == t.Id))
                    .Select(t => t.Barcode)
                    .FirstOrDefault();


            return (
                false,
                $"Tire {groupedBarcode} sudah masuk ke Daisha.",
                tires
            );
        }


        return (
            true,
            "Tire valid dan siap dimasukkan ke Daisha.",
            tires
        );
    }


    // =========================================================
    // CREATE DAISHA
    // =========================================================
    //
    // Membuat:
    //
    // Daisha
    // +
    // DaishaTire
    //
    // =========================================================

    public async Task<Daisha> CreateDaishaAsync(
        List<string> barcodes,
        string computerName,
        string operatorName)
    {
        if (barcodes == null || barcodes.Count == 0)
        {
            throw new ArgumentException(
                "Tire belum dipilih."
            );
        }


        // Bersihkan barcode
        barcodes = barcodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();


        // =====================================================
        // VALIDASI JUMLAH TIRE
        // =====================================================

        if (!IsValidTireCount(barcodes.Count))
        {
            throw new ArgumentException(
                "Jumlah Tire harus 4, 6, 8, 10, 12, 14, atau 16."
            );
        }


        // =====================================================
        // VALIDASI TIRE
        // =====================================================

        var validation =
            await ValidateTiresAsync(barcodes);


        if (!validation.Success)
        {
            throw new InvalidOperationException(
                validation.Message
            );
        }


        List<Tire> tires =
            validation.Tires;


        // =====================================================
        // GENERATE DAISHA CODE
        // =====================================================

        string daishaCode =
            await GenerateDaishaCodeAsync();


        // =====================================================
        // CREATE DAISHA
        // =====================================================

        var daisha = new Daisha
        {
            DaishaCode = daishaCode,

            ComputerName =
                computerName.Trim(),

            OperatorName =
                operatorName.Trim(),

            TotalTires =
                tires.Count,

            CreatedAt =
                DateTime.Now,

            Status =
                "READY_FOR_CURING"
        };


        // =====================================================
        // MASUKKAN TIRE KE DAISHA
        // =====================================================

        int sequence = 1;

        foreach (var tire in tires)
        {
            daisha.DaishaTires.Add(
                new DaishaTire
                {
                    TireId = tire.Id,
                    Sequence = sequence
                }
            );

            sequence++;
        }


        // =====================================================
        // SIMPAN DATABASE
        // =====================================================

        _context.Daishas.Add(daisha);

        await _context.SaveChangesAsync();


        return daisha;
    }


    // =========================================================
    // GET DAISHA BERDASARKAN CODE
    // =========================================================
    //
    // Dipakai nanti saat QR Daisha di-scan.
    //
    // Hasilnya:
    //
    // DS-20260830-001
    //      ↓
    // Tire 1
    // Tire 2
    // Tire 3
    // ...
    //
    // =========================================================

    public async Task<Daisha?> GetDaishaAsync(
        string daishaCode)
    {
        if (string.IsNullOrWhiteSpace(daishaCode))
        {
            return null;
        }


        return await _context.Daishas
            .Include(x => x.DaishaTires)
                .ThenInclude(x => x.Tire)
            .FirstOrDefaultAsync(
                x => x.DaishaCode == daishaCode.Trim()
            );
    }


    // =========================================================
    // GET DAISHA TERBARU
    // =========================================================

    public async Task<List<Daisha>> GetRecentDaishasAsync(
        int count = 20)
    {
        return await _context.Daishas
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .Include(x => x.DaishaTires)
                .ThenInclude(x => x.Tire)
            .ToListAsync();
    }


    // =========================================================
    // GET TIRE YANG BELUM MASUK DAISHA
    // =========================================================
    //
    // Dipakai pada halaman Create Daisha.
    //
    // Hanya Tire yang belum pernah masuk Daisha
    // yang akan ditampilkan.
    //
    // =========================================================

    public async Task<List<Tire>> GetAvailableTiresAsync()
    {
        var groupedTireIds =
            await _context.DaishaTires
                .Select(x => x.TireId)
                .ToListAsync();


        return await _context.Tires
            .Where(x =>
                !groupedTireIds.Contains(x.Id))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }


    // =========================================================
    // PARSE DAISHA CODE
    // =========================================================
    //
    // Dipakai saat QR scanner membaca:
    //
    // DS-20260830-001
    //
    // =========================================================

    public string ExtractDaishaCode(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }


        string text = input.Trim();


        // =====================================================
        // CARI "DS-"
        // =====================================================

        int index =
            text.IndexOf(
                "DS-",
                StringComparison.OrdinalIgnoreCase);


        if (index < 0)
        {
            return string.Empty;
        }


        // Ambil mulai dari DS-
        string result =
            text.Substring(index);


        // =====================================================
        // POTONG SAAT MENEMUKAN BARIS / SPASI
        // =====================================================

        string[] parts =
            result.Split(
                new[]
                {
                    ' ',
                    '\r',
                    '\n',
                    '\t'
                },
                StringSplitOptions.RemoveEmptyEntries);


        if (parts.Length == 0)
        {
            return string.Empty;
        }


        string daishaCode =
            parts[0].Trim();


        // =====================================================
        // VALIDASI FORMAT
        // =====================================================

        if (!daishaCode.StartsWith(
                "DS-",
                StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }


        return daishaCode;
    }
}