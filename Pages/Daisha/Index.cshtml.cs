using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Data;
using TireTraceabilityDemo.Models;

// Alias supaya tidak bentrok dengan namespace Pages.Daisha
using DaishaModel = TireTraceabilityDemo.Models.Daisha;

namespace TireTraceabilityDemo.Pages.Daisha;

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
    // DATA DAISHA
    // =========================================================

    public List<DaishaModel> Daishas { get; set; } = new();


    // =========================================================
    // SEARCH
    // =========================================================

    [BindProperty(SupportsGet = true)]
    public string Search { get; set; } = string.Empty;


    // =========================================================
    // GET
    // =========================================================

    public async Task OnGetAsync()
    {
        await LoadDaishasAsync();
    }


    // =========================================================
    // LOAD DATA DAISHA
    // =========================================================

    private async Task LoadDaishasAsync()
    {
        IQueryable<DaishaModel> query =
            _context.Daishas
                .Include(d => d.DaishaTires)
                .ThenInclude(dt => dt.Tire);


        // =====================================================
        // SEARCH
        // =====================================================

        if (!string.IsNullOrWhiteSpace(Search))
        {
            string keyword = Search.Trim();

            query = query.Where(d =>
                d.DaishaCode.Contains(keyword));
        }


        // =====================================================
        // ORDER
        // =====================================================

        Daishas = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }


    // =========================================================
    // DELETE DAISHA
    // =========================================================
    //
    // Yang dihapus hanya:
    // - Daisha
    // - Relasi DaishaTire
    //
    // Tire asli TIDAK dihapus.
    //
    // =========================================================

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var daisha = await _context.Daishas
            .FirstOrDefaultAsync(d => d.Id == id);

        if (daisha == null)
        {
            TempData["Error"] =
                "Data Daisha tidak ditemukan.";

            return RedirectToPage();
        }


        // =====================================================
        // HAPUS RELASI DAISHA - TIRE
        // =====================================================

        var daishaTires = await _context.DaishaTires
            .Where(dt => dt.DaishaId == id)
            .ToListAsync();


        if (daishaTires.Count > 0)
        {
            _context.DaishaTires.RemoveRange(daishaTires);
        }


        // =====================================================
        // HAPUS DAISHA
        // =====================================================

        _context.Daishas.Remove(daisha);

        await _context.SaveChangesAsync();


        TempData["Success"] =
            $"Daisha {daisha.DaishaCode} berhasil dihapus.";


        return RedirectToPage();
    }


    // =========================================================
    // DETAILS
    // =========================================================

    public IActionResult OnGetDetails(int id)
    {
        return RedirectToPage(
            "/Daisha/Details",
            new
            {
                id = id
            });
    }
}