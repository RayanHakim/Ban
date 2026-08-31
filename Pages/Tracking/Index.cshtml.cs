using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Data;

using TireEntity = TireTraceabilityDemo.Models.Tire;
using CuringEntity = TireTraceabilityDemo.Models.Curing;
using InspectionEntity = TireTraceabilityDemo.Models.Inspection;
using DaishaEntity = TireTraceabilityDemo.Models.Daisha;

namespace TireTraceabilityDemo.Pages.Tracking;

public class IndexModel : PageModel
{
    // =========================================================
    // DATABASE
    // =========================================================

    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // FILTER TIRE
    // =========================================================

    [BindProperty(SupportsGet = true)]
    public string Barcode { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public DateTime? BuildingDate { get; set; }


    // =========================================================
    // SEARCH DAISHA
    // =========================================================

    [BindProperty(SupportsGet = true)]
    public string DaishaSearch { get; set; } = string.Empty;


    // =========================================================
    // TIRE DATA
    // =========================================================

    public List<TireEntity> Tires { get; set; } = new();

    public TireEntity? Tire { get; set; }


    // =========================================================
    // CURING DATA
    // =========================================================

    public CuringEntity? CuringData { get; set; }


    // =========================================================
    // INSPECTION DATA
    // =========================================================

    public InspectionEntity? InspectionData { get; set; }


    // =========================================================
    // DAISHA DATA
    // =========================================================

    public DaishaEntity? Daisha { get; set; }

    public List<DaishaEntity> Daishas { get; set; } = new();


    // =========================================================
    // TIRE DALAM DAISHA
    // =========================================================

    public List<TireEntity> DaishaTires { get; set; } = new();


    // =========================================================
    // MESSAGE
    // =========================================================

    public string Message { get; set; } = string.Empty;


    // =========================================================
    // STATUS
    // =========================================================

    public bool IsFound { get; set; }

    public bool IsDaishaFound { get; set; }


    // =========================================================
    // GET
    // =========================================================

    public async Task OnGetAsync()
    {
        await LoadTireHistoryAsync();

        await LoadSelectedTireAsync();

        await LoadDaishaSearchAsync();
    }


    // =========================================================
    // LOAD TIRE HISTORY
    // =========================================================

    private async Task LoadTireHistoryAsync()
    {
        IQueryable<TireEntity> query =
            _context.Tires.AsNoTracking();


        // -----------------------------------------------------
        // FILTER TANGGAL BUILDING
        // -----------------------------------------------------

        if (BuildingDate.HasValue)
        {
            DateTime startDate =
                BuildingDate.Value.Date;

            DateTime endDate =
                startDate.AddDays(1);

            query =
                query.Where(x =>
                    x.CreatedAt >= startDate &&
                    x.CreatedAt < endDate);
        }


        // -----------------------------------------------------
        // FILTER BARCODE
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(Barcode))
        {
            string keyword =
                Barcode.Trim();

            query =
                query.Where(x =>
                    x.Barcode.Contains(keyword));
        }


        // -----------------------------------------------------
        // LOAD DATA
        // -----------------------------------------------------

        Tires =
            await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
    }


    // =========================================================
    // LOAD SELECTED TIRE
    // =========================================================

    private async Task LoadSelectedTireAsync()
    {
        // -----------------------------------------------------
        // Tidak ada barcode
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(Barcode))
        {
            return;
        }


        string actualBarcode =
            Barcode.Trim();


        // -----------------------------------------------------
        // CARI TIRE
        // -----------------------------------------------------

        Tire =
            await _context.Tires
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Barcode == actualBarcode);


        // -----------------------------------------------------
        // TIRE TIDAK DITEMUKAN
        // -----------------------------------------------------

        if (Tire == null)
        {
            Message =
                $"Barcode tire {actualBarcode} tidak ditemukan.";

            IsFound = false;

            return;
        }


        // -----------------------------------------------------
        // LOAD CURING
        // -----------------------------------------------------

        CuringData =
            await _context.Curings
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Barcode == actualBarcode);


        // -----------------------------------------------------
        // LOAD INSPECTION
        // -----------------------------------------------------

        InspectionData =
            await _context.Inspections
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Barcode == actualBarcode);


        // -----------------------------------------------------
        // CARI RELASI DAISHA
        // -----------------------------------------------------

        var daishaRelation =
            await _context.DaishaTires
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.TireId == Tire.Id);


        // -----------------------------------------------------
        // JIKA TIRE SUDAH MASUK DAISHA
        // -----------------------------------------------------

        if (daishaRelation != null)
        {
            Daisha =
                await _context.Daishas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == daishaRelation.DaishaId);


            if (Daisha != null)
            {
                await LoadDaishaTiresAsync(
                    Daisha.Id);
            }
        }


        // -----------------------------------------------------
        // SUCCESS
        // -----------------------------------------------------

        IsFound = true;

        Message =
            "Data traceability berhasil ditemukan.";

        Barcode =
            actualBarcode;
    }


    // =========================================================
    // LOAD SEMUA TIRE DALAM DAISHA
    // =========================================================

    private async Task LoadDaishaTiresAsync(
        int daishaId)
    {
        if (daishaId <= 0)
        {
            DaishaTires =
                new List<TireEntity>();

            return;
        }


        // -----------------------------------------------------
        // AMBIL ID TIRE
        // -----------------------------------------------------

        var tireIds =
            await _context.DaishaTires
                .AsNoTracking()
                .Where(x =>
                    x.DaishaId == daishaId)
                .Select(x =>
                    x.TireId)
                .ToListAsync();


        if (tireIds.Count == 0)
        {
            DaishaTires =
                new List<TireEntity>();

            return;
        }


        // -----------------------------------------------------
        // AMBIL DATA TIRE
        // -----------------------------------------------------

        DaishaTires =
            await _context.Tires
                .AsNoTracking()
                .Where(x =>
                    tireIds.Contains(x.Id))
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
    }


    // =========================================================
    // LOAD DAISHA SEARCH
    // =========================================================

    private async Task LoadDaishaSearchAsync()
    {
        // -----------------------------------------------------
        // Tidak ada search
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(DaishaSearch))
        {
            return;
        }


        string keyword =
            DaishaSearch.Trim();


        // -----------------------------------------------------
        // CARI DAISHA
        // -----------------------------------------------------

        Daishas =
            await _context.Daishas
                .AsNoTracking()
                .Where(x =>
                    x.DaishaCode.Contains(keyword))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();


        // -----------------------------------------------------
        // TIDAK DITEMUKAN
        // -----------------------------------------------------

        if (Daishas.Count == 0)
        {
            Message =
                $"Daisha dengan ID \"{keyword}\" tidak ditemukan.";

            IsDaishaFound = false;

            return;
        }


        // -----------------------------------------------------
        // EXACT MATCH
        // -----------------------------------------------------

        Daisha =
            Daishas.FirstOrDefault(x =>
                string.Equals(
                    x.DaishaCode,
                    keyword,
                    StringComparison.OrdinalIgnoreCase));


        // -----------------------------------------------------
        // KALAU TIDAK ADA EXACT MATCH
        // -----------------------------------------------------

        if (Daisha == null)
        {
            Daisha =
                Daishas.First();
        }


        // -----------------------------------------------------
        // LOAD TIRES DALAM DAISHA
        // -----------------------------------------------------

        await LoadDaishaTiresAsync(
            Daisha.Id);


        // -----------------------------------------------------
        // SUCCESS
        // -----------------------------------------------------

        IsDaishaFound = true;

        Message =
            $"Daisha {Daisha.DaishaCode} berhasil ditemukan.";
    }


    // =========================================================
    // GET CURING
    // =========================================================

    public CuringEntity? GetCuring(
        string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return null;
        }


        string actualBarcode =
            barcode.Trim();


        return _context.Curings
            .AsNoTracking()
            .FirstOrDefault(x =>
                x.Barcode == actualBarcode);
    }


    // =========================================================
    // GET INSPECTION
    // =========================================================

    public InspectionEntity? GetInspection(
        string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return null;
        }


        string actualBarcode =
            barcode.Trim();


        return _context.Inspections
            .AsNoTracking()
            .FirstOrDefault(x =>
                x.Barcode == actualBarcode);
    }


    // =========================================================
    // GET DAISHA BERDASARKAN TIRE
    // =========================================================

    public DaishaEntity? GetDaishaByTire(
        int tireId)
    {
        if (tireId <= 0)
        {
            return null;
        }


        // -----------------------------------------------------
        // CARI RELASI
        // -----------------------------------------------------

        var relation =
            _context.DaishaTires
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.TireId == tireId);


        if (relation == null)
        {
            return null;
        }


        // -----------------------------------------------------
        // CARI DAISHA
        // -----------------------------------------------------

        return _context.Daishas
            .AsNoTracking()
            .FirstOrDefault(x =>
                x.Id == relation.DaishaId);
    }


    // =========================================================
    // JUMLAH TIRE DALAM DAISHA
    // =========================================================

    public int GetDaishaTireCount(
        int daishaId)
    {
        if (daishaId <= 0)
        {
            return 0;
        }


        return _context.DaishaTires
            .AsNoTracking()
            .Count(x =>
                x.DaishaId == daishaId);
    }


    // =========================================================
    // STATUS TIRE
    // =========================================================

    public string GetTireStatus(
        string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return "UNKNOWN";
        }


        string actualBarcode =
            barcode.Trim();


        // -----------------------------------------------------
        // INSPECTION
        // -----------------------------------------------------

        bool hasInspection =
            _context.Inspections
                .AsNoTracking()
                .Any(x =>
                    x.Barcode == actualBarcode);


        if (hasInspection)
        {
            return "INSPECTED";
        }


        // -----------------------------------------------------
        // CURING
        // -----------------------------------------------------

        bool hasCuring =
            _context.Curings
                .AsNoTracking()
                .Any(x =>
                    x.Barcode == actualBarcode);


        if (hasCuring)
        {
            return "CURING";
        }


        // -----------------------------------------------------
        // BUILDING
        // -----------------------------------------------------

        bool hasTire =
            _context.Tires
                .AsNoTracking()
                .Any(x =>
                    x.Barcode == actualBarcode);


        if (hasTire)
        {
            return "BUILDING";
        }


        return "UNKNOWN";
    }


    // =========================================================
    // STATUS TIRE DALAM DAISHA
    // =========================================================

    public string GetDaishaTireStatus(
        TireEntity tire)
    {
        if (tire == null)
        {
            return "UNKNOWN";
        }


        return GetTireStatus(
            tire.Barcode);
    }


    // =========================================================
    // JUMLAH TIRE DAISHA YANG SEDANG DITAMPILKAN
    // =========================================================

    public int GetCurrentDaishaTireCount()
    {
        return DaishaTires.Count;
    }
}