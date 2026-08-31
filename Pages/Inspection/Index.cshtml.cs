using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TireTraceabilityDemo.Data;

namespace TireTraceabilityDemo.Pages.Inspection;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public string Barcode { get; set; } = string.Empty;

    [BindProperty]
    public string TireSize { get; set; } = string.Empty;

    [BindProperty]
    public string MoldNumber { get; set; } = string.Empty;

    [BindProperty]
    public string OperatorId { get; set; } = string.Empty;

    [BindProperty]
    public string DefectName { get; set; } = string.Empty;

    [BindProperty]
    public string Position { get; set; } = string.Empty;

    [BindProperty]
    public bool Rework { get; set; }

    [BindProperty]
    public bool Strap { get; set; }

    [BindProperty]
    public bool Hold { get; set; }

    public TireTraceabilityDemo.Models.Tire? Tire { get; set; }

    public TireTraceabilityDemo.Models.Curing? CuringData { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost(string action)
    {
        if (string.IsNullOrWhiteSpace(Barcode))
        {
            Message = "Barcode harus diisi.";
            IsSuccess = false;

            return Page();
        }

        Tire = _context.Tires
            .FirstOrDefault(x => x.Barcode == Barcode);

        if (Tire == null)
        {
            Message = "Barcode tidak ditemukan pada proses Building.";
            IsSuccess = false;

            return Page();
        }

        CuringData = _context.Curings
            .FirstOrDefault(x => x.Barcode == Barcode);

        if (CuringData == null)
        {
            Message = "Barcode belum tercatat pada proses Curing.";
            IsSuccess = false;

            return Page();
        }

        if (action == "search")
        {
            TireSize = Tire.TireSize;
            MoldNumber = CuringData.MoldNumber;
            OperatorId = CuringData.OperatorId;

            Message = "Data tire berhasil ditemukan.";
            IsSuccess = true;

            return Page();
        }

        if (action == "save")
        {
            if (string.IsNullOrWhiteSpace(TireSize) ||
                string.IsNullOrWhiteSpace(MoldNumber) ||
                string.IsNullOrWhiteSpace(OperatorId) ||
                string.IsNullOrWhiteSpace(DefectName) ||
                string.IsNullOrWhiteSpace(Position))
            {
                Message = "Data inspection harus diisi.";
                IsSuccess = false;

                return Page();
            }

            var existingInspection = _context.Inspections
                .FirstOrDefault(x => x.Barcode == Barcode);

            if (existingInspection != null)
            {
                Message = "Barcode ini sudah memiliki data Final Inspection.";
                IsSuccess = false;

                return Page();
            }

            var inspection = new TireTraceabilityDemo.Models.Inspection
            {
                Barcode = Barcode,
                TireSize = TireSize,
                MoldNumber = MoldNumber,
                OperatorId = OperatorId,
                DefectName = DefectName,
                Position = Position,
                Rework = Rework,
                Strap = Strap,
                Hold = Hold,
                InspectionAt = DateTime.Now
            };

            _context.Inspections.Add(inspection);
            _context.SaveChanges();

            Message = "Data Final Inspection berhasil disimpan.";
            IsSuccess = true;

            Barcode = string.Empty;
            TireSize = string.Empty;
            MoldNumber = string.Empty;
            OperatorId = string.Empty;
            DefectName = string.Empty;
            Position = string.Empty;
            Rework = false;
            Strap = false;
            Hold = false;

            Tire = null;
            CuringData = null;

            return Page();
        }

        Message = "Perintah tidak dikenali.";
        IsSuccess = false;

        return Page();
    }
}