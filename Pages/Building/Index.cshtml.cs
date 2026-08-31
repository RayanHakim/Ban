using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Data;
using TireTraceabilityDemo.Models;
using TireTraceabilityDemo.Services;

namespace TireTraceabilityDemo.Pages.Building;

public class IndexModel : PageModel
{
    // =========================================================
    // DEPENDENCY
    // =========================================================

    private readonly AppDbContext _context;
    private readonly BarcodeService _barcodeService;

    public IndexModel(
        AppDbContext context,
        BarcodeService barcodeService)
    {
        _context = context;
        _barcodeService = barcodeService;
    }


    // =========================================================
    // DATA HALAMAN
    // =========================================================

    public List<Tire> RecentTires { get; set; } = new();

    public List<Dropdown> TireSizes { get; set; } = new();


    // =========================================================
    // FORM INPUT
    // =========================================================

    [BindProperty]
    public string TireSize { get; set; } = string.Empty;


    // =========================================================
    // MACHINE
    // =========================================================
    //
    // Laptop / komputer yang digunakan dianggap sebagai
    // Machine untuk station Building.
    //
    // Contoh:
    //
    // LAPTOP-OG18VV4B
    //
    // =========================================================

    public string CurrentComputerName { get; set; } = string.Empty;

    public string MachineNumber { get; set; } = string.Empty;


    // =========================================================
    // OPERATOR
    // =========================================================

    public string CurrentOperatorName { get; set; } = "Operator";


    // =========================================================
    // SHIFT
    // =========================================================

    public string CurrentShift { get; set; } = string.Empty;


    // =========================================================
    // CURRENT TIME
    // =========================================================

    public DateTime CurrentTime { get; set; }


    // =========================================================
    // QR / BARCODE RESULT
    // =========================================================

    public string Barcode { get; set; } = string.Empty;

    public string QrCode { get; set; } = string.Empty;

    public string LastTireSize { get; set; } = string.Empty;

    public string LastMachine { get; set; } = string.Empty;

    public string LastOperator { get; set; } = string.Empty;

    public string LastShift { get; set; } = string.Empty;

    public DateTime? LastCreatedAt { get; set; }


    // =========================================================
    // ERROR
    // =========================================================

    public string ErrorMessage { get; set; } = string.Empty;


    // =========================================================
    // ON GET
    // =========================================================

    public async Task OnGetAsync()
    {
        // -----------------------------------------------------
        // Load session
        // -----------------------------------------------------

        LoadSessionData();


        // -----------------------------------------------------
        // Load machine dari nama komputer
        // -----------------------------------------------------

        LoadMachine();


        // -----------------------------------------------------
        // Tentukan shift saat ini
        // -----------------------------------------------------

        SetCurrentShift();


        // -----------------------------------------------------
        // Load Tire Size
        // -----------------------------------------------------

        await LoadTireSizesAsync();


        // -----------------------------------------------------
        // Load production history
        // -----------------------------------------------------

        await LoadRecentTiresAsync();


        // -----------------------------------------------------
        // Jika ada Tire yang baru dibuat,
        // tampilkan QR-nya kembali setelah redirect.
        // -----------------------------------------------------

        var lastBarcode =
            TempData["LastBarcode"]?.ToString();

        if (!string.IsNullOrWhiteSpace(lastBarcode))
        {
            await LoadLastGeneratedTireAsync(lastBarcode);
        }
    }


    // =========================================================
    // LOAD SESSION
    // =========================================================

    private void LoadSessionData()
    {
        CurrentOperatorName =
            HttpContext.Session.GetString(
                "BuildingOperatorName")
            ?? "Operator";
    }


    // =========================================================
    // LOAD MACHINE
    // =========================================================
    //
    // Tidak menggunakan tabel Machine.
    //
    // Nama laptop langsung menjadi MachineNumber.
    //
    // =========================================================

    private void LoadMachine()
    {
        CurrentComputerName =
            Environment.MachineName;

        MachineNumber =
            CurrentComputerName;
    }


    // =========================================================
    // SET CURRENT SHIFT
    // =========================================================
    //
    // Karena pembagian shift asli belum diketahui,
    // sementara digunakan rotasi 8 jam:
    //
    // 08:00 - 15:59 -> Shift 1
    // 16:00 - 23:59 -> Shift 2
    // 00:00 - 07:59 -> Shift 3
    //
    // Nanti angka shift dapat diubah jika pembagian resmi
    // dari perusahaan sudah diketahui.
    //
    // =========================================================

    private void SetCurrentShift()
    {
        CurrentTime =
            DateTime.Now;

        TimeSpan currentTime =
            CurrentTime.TimeOfDay;


        TimeSpan shift1Start =
            new TimeSpan(8, 0, 0);

        TimeSpan shift2Start =
            new TimeSpan(16, 0, 0);


        if (currentTime >= shift1Start &&
            currentTime < shift2Start)
        {
            CurrentShift = "Shift 1";
        }
        else if (currentTime >= shift2Start)
        {
            CurrentShift = "Shift 2";
        }
        else
        {
            CurrentShift = "Shift 3";
        }
    }


    // =========================================================
    // LOAD TIRE SIZE
    // =========================================================

    private async Task LoadTireSizesAsync()
    {
        TireSizes =
            await _context.Dropdowns
                .Where(d =>
                    d.Category == "TireSize" &&
                    d.IsActive)
                .OrderBy(d => d.Value)
                .ToListAsync();
    }


    // =========================================================
    // LOAD RECENT TIRES
    // =========================================================

    private async Task LoadRecentTiresAsync()
    {
        RecentTires =
            await _context.Tires
                .OrderByDescending(
                    t => t.CreatedAt)
                .Take(20)
                .ToListAsync();
    }


    // =========================================================
    // LOAD LAST GENERATED TIRE
    // =========================================================

    private async Task LoadLastGeneratedTireAsync(
        string barcode)
    {
        var tire =
            await _context.Tires
                .FirstOrDefaultAsync(
                    t => t.Barcode == barcode);


        if (tire == null)
        {
            return;
        }


        // -----------------------------------------------------
        // Data barcode
        // -----------------------------------------------------

        Barcode =
            tire.Barcode;


        // -----------------------------------------------------
        // Data tire
        // -----------------------------------------------------

        LastTireSize =
            tire.TireSize;


        // -----------------------------------------------------
        // Machine
        // -----------------------------------------------------

        LastMachine =
            tire.MachineNumber;


        // -----------------------------------------------------
        // Operator
        // -----------------------------------------------------

        LastOperator =
            tire.OperatorId;


        // -----------------------------------------------------
        // Shift
        // -----------------------------------------------------

        LastShift =
            tire.Shift;


        // -----------------------------------------------------
        // Created
        // -----------------------------------------------------

        LastCreatedAt =
            tire.CreatedAt;


        // -----------------------------------------------------
        // Generate QR
        // -----------------------------------------------------

        QrCode =
            GenerateQrDataUri(tire);
    }


    // =========================================================
    // GENERATE UNIQUE BARCODE
    // =========================================================
    //
    // Format:
    //
    // BT-20260829-231905-A433
    //
    // BT      = Building Tire
    // tanggal = yyyyMMdd
    // waktu   = HHmmss
    // random  = 4 karakter
    //
    // =========================================================

    private async Task<string>
        GenerateUniqueBarcodeAsync()
    {
        string barcode;


        do
        {
            string timestamp =
                DateTime.Now.ToString(
                    "yyyyMMdd-HHmmss");


            string randomCode =
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 4)
                    .ToUpperInvariant();


            barcode =
                $"BT-{timestamp}-{randomCode}";
        }
        while (
            await _context.Tires
                .AnyAsync(
                    t => t.Barcode == barcode)
        );


        return barcode;
    }


    // =========================================================
    // CREATE TIRE
    // =========================================================

    public async Task<IActionResult>
        OnPostCreateAsync()
    {
        // =====================================================
        // CEK LOGIN
        // =====================================================

        var operatorUsername =
            HttpContext.Session.GetString(
                "BuildingOperatorUsername");


        var operatorName =
            HttpContext.Session.GetString(
                "BuildingOperatorName");


        if (string.IsNullOrWhiteSpace(
                operatorUsername) ||
            string.IsNullOrWhiteSpace(
                operatorName))
        {
            return RedirectToPage(
                "/Building/Login");
        }


        // =====================================================
        // LOAD INFORMASI TERKINI
        // =====================================================

        CurrentOperatorName =
            operatorName;


        LoadMachine();


        SetCurrentShift();


        // =====================================================
        // VALIDASI MACHINE
        // =====================================================

        if (string.IsNullOrWhiteSpace(
                MachineNumber))
        {
            ErrorMessage =
                "Nama komputer tidak berhasil dideteksi.";


            await ReloadPageDataAsync();


            return Page();
        }


        // =====================================================
        // VALIDASI TIRE SIZE
        // =====================================================

        if (string.IsNullOrWhiteSpace(TireSize))
        {
            ErrorMessage =
                "Tire Size harus dipilih.";


            await ReloadPageDataAsync();


            return Page();
        }


        // =====================================================
        // VALIDASI TIRE SIZE DARI DATABASE
        // =====================================================

        var tireSizeData =
            await _context.Dropdowns
                .FirstOrDefaultAsync(d =>
                    d.Category == "TireSize" &&
                    d.Value == TireSize &&
                    d.IsActive);


        if (tireSizeData == null)
        {
            ErrorMessage =
                "Tire Size tidak ditemukan " +
                "atau sudah tidak aktif.";


            await ReloadPageDataAsync();


            return Page();
        }


        // =====================================================
        // WAKTU PRODUKSI
        // =====================================================

        DateTime productionTime =
            DateTime.Now;


        // =====================================================
        // GENERATE BARCODE
        // =====================================================

        string barcode =
            await GenerateUniqueBarcodeAsync();


        // =====================================================
        // CREATE TIRE
        // =====================================================

        var tire =
            new Tire
            {
                Barcode =
                    barcode,

                TireSize =
                    tireSizeData.Value,

                MachineNumber =
                    MachineNumber,

                Shift =
                    CurrentShift,

                OperatorId =
                    operatorName,

                CreatedAt =
                    productionTime
            };


        // =====================================================
        // SAVE DATABASE
        // =====================================================

        _context.Tires.Add(tire);

        await _context.SaveChangesAsync();


        // =====================================================
        // SIMPAN BARCODE UNTUK QR RESULT
        // =====================================================

        TempData["LastBarcode"] =
            barcode;


        // =====================================================
        // SUCCESS MESSAGE
        // =====================================================

        TempData["Success"] =
            $"Tire berhasil dibuat. " +
            $"Barcode: {barcode}. " +
            $"Machine: {MachineNumber}. " +
            $"Shift: {CurrentShift}.";


        // =====================================================
        // REDIRECT
        // =====================================================

        return RedirectToPage();
    }


    // =========================================================
    // GENERATE QR DATA URI
    // =========================================================

    private string GenerateQrDataUri(
        Tire tire)
    {
        // -----------------------------------------------------
        // Isi QR Code
        // -----------------------------------------------------
        //
        // Saat HP scan QR:
        //
        // Barcode
        // Tire Size
        // Machine
        // Operator
        // Shift
        // Building Time
        // Current Process
        //
        // -----------------------------------------------------

        string qrContent =
            "TIRE TRACEABILITY\n" +
            "\n" +

            $"Barcode: {tire.Barcode}\n" +

            $"Tire Size: {tire.TireSize}\n" +

            $"Machine: {tire.MachineNumber}\n" +

            $"Operator: {tire.OperatorId}\n" +

            $"Shift: {tire.Shift}\n" +

            $"Building Time: " +
            $"{tire.CreatedAt:dd/MM/yyyy HH:mm:ss}\n" +

            "Current Process: BUILDING";


        // -----------------------------------------------------
        // Generate PNG menggunakan BarcodeService
        // -----------------------------------------------------

        string base64 =
            _barcodeService.GenerateQrCode(
                qrContent);


        // -----------------------------------------------------
        // Return data URI
        // -----------------------------------------------------

        return
            $"data:image/png;base64,{base64}";
    }


    // =========================================================
    // DOWNLOAD QR CODE
    // =========================================================
    //
    // URL:
    //
    // /Building?handler=DownloadQr&barcode=xxxxx
    //
    // =========================================================

    public async Task<IActionResult>
        OnGetDownloadQrAsync(
            string barcode)
    {
        // -----------------------------------------------------
        // VALIDASI BARCODE
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(barcode))
        {
            return BadRequest(
                "Barcode tidak boleh kosong.");
        }


        // -----------------------------------------------------
        // BERSIHKAN INPUT
        // -----------------------------------------------------

        string actualBarcode =
            barcode.Trim();


        // -----------------------------------------------------
        // CARI TIRE
        // -----------------------------------------------------

        var tire =
            await _context.Tires
                .FirstOrDefaultAsync(
                    t => t.Barcode == actualBarcode);


        if (tire == null)
        {
            return NotFound(
                "Barcode tidak ditemukan.");
        }


        // -----------------------------------------------------
        // GENERATE QR DATA
        // -----------------------------------------------------

        string qrDataUri =
            GenerateQrDataUri(tire);


        // -----------------------------------------------------
        // AMBIL BASE64 SAJA
        // -----------------------------------------------------

        const string prefix =
            "data:image/png;base64,";


        if (!qrDataUri.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                "Format QR Code tidak valid.");
        }


        string base64 =
            qrDataUri.Substring(
                prefix.Length);


        // -----------------------------------------------------
        // CONVERT BASE64 KE BYTE
        // -----------------------------------------------------

        byte[] qrBytes;

        try
        {
            qrBytes =
                Convert.FromBase64String(
                    base64);
        }
        catch
        {
            return BadRequest(
                "Data QR Code tidak valid.");
        }


        // -----------------------------------------------------
        // RETURN FILE
        // -----------------------------------------------------

        return File(
            qrBytes,
            "image/png",
            $"{tire.Barcode}_QR.png");
    }


    // =========================================================
    // RELOAD PAGE DATA
    // =========================================================

    private async Task ReloadPageDataAsync()
    {
        // -----------------------------------------------------
        // Session
        // -----------------------------------------------------

        LoadSessionData();


        // -----------------------------------------------------
        // Machine
        // -----------------------------------------------------

        LoadMachine();


        // -----------------------------------------------------
        // Shift
        // -----------------------------------------------------

        SetCurrentShift();


        // -----------------------------------------------------
        // Tire Size
        // -----------------------------------------------------

        await LoadTireSizesAsync();


        // -----------------------------------------------------
        // Recent Tire
        // -----------------------------------------------------

        await LoadRecentTiresAsync();
    }
}