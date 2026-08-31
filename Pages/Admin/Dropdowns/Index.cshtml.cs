using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Data;
using TireTraceabilityDemo.Models;

namespace TireTraceabilityDemo.Pages.Admin.Dropdowns;

public class IndexModel : PageModel
{
    // =========================
    // DATABASE
    // =========================

    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    // =========================
    // TIRE SIZE
    // =========================

    public List<Dropdown> TireSizes { get; set; } = new();

    [BindProperty]
    public string NewTireSize { get; set; } = string.Empty;

    [BindProperty]
    public string EditTireSize { get; set; } = string.Empty;

    [BindProperty]
    public int EditId { get; set; }

    // =========================
    // GET
    // =========================

    public async Task OnGetAsync()
    {
        await LoadData();
    }

    // =========================
    // ADD TIRE SIZE
    // =========================

    public async Task<IActionResult> OnPostAddAsync()
    {
        // Validasi kosong
        if (string.IsNullOrWhiteSpace(NewTireSize))
        {
            TempData["Error"] = "Tire Size tidak boleh kosong.";
            return RedirectToPage();
        }

        string size = NewTireSize.Trim();

        // Cek apakah sudah ada
        bool alreadyExists = await _context.Dropdowns
            .AnyAsync(x =>
                x.Category == "TireSize" &&
                x.Value.ToLower() == size.ToLower());

        if (alreadyExists)
        {
            TempData["Error"] = "Tire Size tersebut sudah tersedia.";
            return RedirectToPage();
        }

        // Buat data baru
        var newDropdown = new Dropdown
        {
            Category = "TireSize",
            Value = size,
            IsActive = true
        };

        _context.Dropdowns.Add(newDropdown);

        // Simpan ke MySQL
        await _context.SaveChangesAsync();

        TempData["Success"] = "Tire Size berhasil ditambahkan ke database.";

        return RedirectToPage();
    }

    // =========================
    // EDIT TIRE SIZE
    // =========================

    public async Task<IActionResult> OnPostEditAsync()
    {
        // Cari data berdasarkan ID
        var item = await _context.Dropdowns
            .FirstOrDefaultAsync(x =>
                x.Id == EditId &&
                x.Category == "TireSize");

        if (item == null)
        {
            TempData["Error"] = "Data Tire Size tidak ditemukan.";
            return RedirectToPage();
        }

        // Validasi kosong
        if (string.IsNullOrWhiteSpace(EditTireSize))
        {
            TempData["Error"] = "Tire Size tidak boleh kosong.";
            return RedirectToPage();
        }

        string newSize = EditTireSize.Trim();

        // Cek duplikat
        bool alreadyExists = await _context.Dropdowns
            .AnyAsync(x =>
                x.Id != EditId &&
                x.Category == "TireSize" &&
                x.Value.ToLower() == newSize.ToLower());

        if (alreadyExists)
        {
            TempData["Error"] = "Tire Size tersebut sudah tersedia.";
            return RedirectToPage();
        }

        // Update data
        item.Value = newSize;

        // Simpan perubahan ke MySQL
        await _context.SaveChangesAsync();

        TempData["Success"] = "Tire Size berhasil diperbarui.";

        return RedirectToPage();
    }

    // =========================
    // TOGGLE ACTIVE
    // =========================

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var item = await _context.Dropdowns
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Category == "TireSize");

        if (item == null)
        {
            TempData["Error"] = "Data Tire Size tidak ditemukan.";
            return RedirectToPage();
        }

        // Balik status
        item.IsActive = !item.IsActive;

        // Simpan ke MySQL
        await _context.SaveChangesAsync();

        TempData["Success"] = item.IsActive
            ? "Tire Size berhasil diaktifkan."
            : "Tire Size berhasil dinonaktifkan.";

        return RedirectToPage();
    }

    // =========================
    // DELETE
    // =========================

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var item = await _context.Dropdowns
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Category == "TireSize");

        if (item == null)
        {
            TempData["Error"] = "Data Tire Size tidak ditemukan.";
            return RedirectToPage();
        }

        // Hapus dari database
        _context.Dropdowns.Remove(item);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Tire Size berhasil dihapus.";

        return RedirectToPage();
    }

    // =========================
    // LOAD DATA
    // =========================

    private async Task LoadData()
    {
        TireSizes = await _context.Dropdowns
            .Where(x => x.Category == "TireSize")
            .OrderBy(x => x.Id)
            .ToListAsync();
    }
}