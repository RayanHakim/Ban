using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Data;
using TireTraceabilityDemo.Models;

namespace TireTraceabilityDemo.Pages.Curing;

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
    // INPUT CURING
    // =========================================================

    [BindProperty]
    public string Barcode { get; set; } = string.Empty;

    [BindProperty]
    public string MoldNumber { get; set; } = string.Empty;

    [BindProperty]
    public string OperatorId { get; set; } = string.Empty;

    [BindProperty]
    public string Shift { get; set; } = string.Empty;


    // =========================================================
    // BUILDING DATA
    // =========================================================

    public Tire? BuildingTire { get; set; }


    // =========================================================
    // MESSAGE
    // =========================================================

    public string Message { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }


    // =========================================================
    // GET
    // =========================================================

    public void OnGet()
    {
    }


    // =========================================================
    // GET TIRE BY BARCODE / QR
    // =========================================================
    //
    // URL:
    //
    // /Curing?handler=GetTire&barcode=xxxxx
    //
    // Bisa menerima:
    //
    // 1. TIRE-20260829123456789
    //
    // 2. BT-20260829-231905-A433
    //
    // 3. Isi QR Code lengkap
    //
    // =========================================================

    public IActionResult OnGetGetTire(string barcode)
    {
        try
        {
            // -------------------------------------------------
            // VALIDASI INPUT
            // -------------------------------------------------

            if (string.IsNullOrWhiteSpace(barcode))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Barcode kosong."
                });
            }


            // -------------------------------------------------
            // EXTRACT BARCODE
            // -------------------------------------------------

            string actualBarcode =
                ExtractBarcode(barcode);


            if (string.IsNullOrWhiteSpace(actualBarcode))
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        "Barcode tidak valid atau format barcode tidak dikenali."
                });
            }


            // -------------------------------------------------
            // CARI DATA BUILDING
            // -------------------------------------------------

            var tire = _context.Tires
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.Barcode == actualBarcode);


            // -------------------------------------------------
            // TIDAK DITEMUKAN
            // -------------------------------------------------

            if (tire == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        $"Barcode {actualBarcode} tidak ditemukan pada data Building."
                });
            }


            // -------------------------------------------------
            // CEK SUDAH CURING
            // -------------------------------------------------

            var existingCuring =
                _context.Curings
                    .AsNoTracking()
                    .FirstOrDefault(x =>
                        x.Barcode == actualBarcode);


            if (existingCuring != null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        "Barcode ini sudah tercatat pada proses Curing."
                });
            }


            // -------------------------------------------------
            // KIRIM DATA BUILDING
            // -------------------------------------------------

            return new JsonResult(new
            {
                success = true,

                data = new
                {
                    barcode = tire.Barcode,

                    tireSize = tire.TireSize,

                    operatorId = tire.OperatorId,

                    machineNumber = tire.MachineNumber,

                    shift = tire.Shift,

                    buildingTime =
                        tire.CreatedAt.ToString(
                            "dd MMM yyyy HH:mm"
                        ),

                    currentProcess = "BUILDING"
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "ERROR GET TIRE CURING: " + ex
            );

            return new JsonResult(new
            {
                success = false,
                message =
                    "Terjadi kesalahan server saat mengambil data Building."
            });
        }
    }


    // =========================================================
    // POST CURING
    // =========================================================

    public IActionResult OnPost()
    {
        try
        {
            // -------------------------------------------------
            // VALIDASI BARCODE
            // -------------------------------------------------

            if (string.IsNullOrWhiteSpace(Barcode))
            {
                Message =
                    "QR Code / Barcode harus diisi.";

                IsSuccess = false;

                return Page();
            }


            // -------------------------------------------------
            // EXTRACT BARCODE
            // -------------------------------------------------

            string actualBarcode =
                ExtractBarcode(Barcode);


            if (string.IsNullOrWhiteSpace(actualBarcode))
            {
                Message =
                    "Barcode tidak valid.";

                IsSuccess = false;

                return Page();
            }


            // -------------------------------------------------
            // CARI DATA BUILDING
            // -------------------------------------------------

            var tire =
                _context.Tires
                    .FirstOrDefault(x =>
                        x.Barcode == actualBarcode);


            if (tire == null)
            {
                Message =
                    "Barcode tidak ditemukan pada data Building.";

                IsSuccess = false;

                return Page();
            }


            BuildingTire = tire;


            // -------------------------------------------------
            // CEK SUDAH CURING
            // -------------------------------------------------

            var existingCuring =
                _context.Curings
                    .FirstOrDefault(x =>
                        x.Barcode == actualBarcode);


            if (existingCuring != null)
            {
                Message =
                    "Barcode ini sudah tercatat pada proses Curing.";

                IsSuccess = false;

                Barcode = actualBarcode;

                return Page();
            }


            // -------------------------------------------------
            // VALIDASI DATA CURING
            // -------------------------------------------------

            if (string.IsNullOrWhiteSpace(MoldNumber) ||
                string.IsNullOrWhiteSpace(OperatorId) ||
                string.IsNullOrWhiteSpace(Shift))
            {
                Message =
                    "Data Building ditemukan. Silakan lengkapi data Curing.";

                IsSuccess = true;

                Barcode = actualBarcode;

                return Page();
            }


            // -------------------------------------------------
            // BUAT DATA CURING
            // -------------------------------------------------

            var curing =
                new TireTraceabilityDemo.Models.Curing
                {
                    Barcode = actualBarcode,

                    MoldNumber =
                        MoldNumber.Trim(),

                    OperatorId =
                        OperatorId.Trim(),

                    Shift =
                        Shift.Trim(),

                    CuringAt =
                        DateTime.Now
                };


            // -------------------------------------------------
            // SIMPAN DATABASE
            // -------------------------------------------------

            _context.Curings.Add(curing);

            _context.SaveChanges();


            // -------------------------------------------------
            // SUCCESS
            // -------------------------------------------------

            Message =
                "Data Curing berhasil disimpan.";

            IsSuccess = true;

            BuildingTire = tire;

            Barcode = actualBarcode;


            // -------------------------------------------------
            // RESET FORM
            // -------------------------------------------------

            MoldNumber = string.Empty;

            OperatorId = string.Empty;

            Shift = string.Empty;


            return Page();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "ERROR POST CURING: " + ex
            );

            Message =
                "Terjadi kesalahan saat menyimpan data Curing.";

            IsSuccess = false;

            return Page();
        }
    }


    // =========================================================
    // EXTRACT BARCODE
    // =========================================================
    //
    // Mendukung:
    //
    // TIRE-20260829134811901
    //
    // BT-20260829-231905-A433
    //
    // maupun QR Code lengkap.
    //
    // =========================================================

    private string ExtractBarcode(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }


        input = input.Trim();


        // =====================================================
        // PRIORITAS 1
        // CARI FORMAT TIRE-
        // =====================================================

        int tireIndex =
            input.IndexOf(
                "TIRE-",
                StringComparison.OrdinalIgnoreCase);


        if (tireIndex >= 0)
        {
            string result =
                input.Substring(tireIndex);


            string[] nextFields =
            {
                "Tire Size:",
                "Operator:",
                "Machine Number:",
                "Machine:",
                "Shift:",
                "Building Time:",
                "Current Process:"
            };


            foreach (string field in nextFields)
            {
                int index =
                    result.IndexOf(
                        field,
                        StringComparison.OrdinalIgnoreCase);


                if (index > 0)
                {
                    result =
                        result.Substring(0, index);

                    break;
                }
            }


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


            if (parts.Length > 0)
            {
                string barcode =
                    parts[0].Trim();


                if (barcode.StartsWith(
                        "TIRE-",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return barcode;
                }
            }
        }


        // =====================================================
        // PRIORITAS 2
        // CARI FORMAT BT-
        // =====================================================

        int btIndex =
            input.IndexOf(
                "BT-",
                StringComparison.OrdinalIgnoreCase);


        if (btIndex >= 0)
        {
            string result =
                input.Substring(btIndex);


            string[] nextFields =
            {
                "Tire Size:",
                "Operator:",
                "Machine Number:",
                "Machine:",
                "Shift:",
                "Building Time:",
                "Current Process:"
            };


            foreach (string field in nextFields)
            {
                int index =
                    result.IndexOf(
                        field,
                        StringComparison.OrdinalIgnoreCase);


                if (index > 0)
                {
                    result =
                        result.Substring(0, index);

                    break;
                }
            }


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


            if (parts.Length > 0)
            {
                string barcode =
                    parts[0].Trim();


                if (barcode.StartsWith(
                        "BT-",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return barcode;
                }
            }
        }


        // =====================================================
        // FALLBACK
        // =====================================================

        return string.Empty;
    }
}