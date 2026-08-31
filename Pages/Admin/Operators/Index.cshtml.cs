using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Data;
using TireTraceabilityDemo.Models;

namespace TireTraceabilityDemo.Pages.Admin.Operators;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    // =========================
    // OPERATOR LIST
    // =========================

    public List<Operator> Operators { get; set; } = new();

    // =========================
    // CREATE OPERATOR
    // =========================

    [BindProperty]
    public Operator NewOperator { get; set; } = new();

    // =========================
    // DELETE
    // =========================

    [BindProperty]
    public int DeleteId { get; set; }

    // =========================
    // GET
    // =========================

    public async Task OnGetAsync()
    {
        await LoadOperators();
    }

    // =========================
    // LOAD OPERATORS
    // =========================

    private async Task LoadOperators()
    {
        Operators = await _context.Operators
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    // =========================
    // CREATE OPERATOR
    // =========================

    public async Task<IActionResult> OnPostCreateAsync()
    {
        // =========================
        // VALIDATION
        // =========================

        if (string.IsNullOrWhiteSpace(NewOperator.Username))
        {
            ModelState.AddModelError(
                "NewOperator.Username",
                "Username wajib diisi."
            );
        }

        if (string.IsNullOrWhiteSpace(NewOperator.Password))
        {
            ModelState.AddModelError(
                "NewOperator.Password",
                "Password wajib diisi."
            );
        }

        if (string.IsNullOrWhiteSpace(NewOperator.Name))
        {
            ModelState.AddModelError(
                "NewOperator.Name",
                "Nama operator wajib diisi."
            );
        }

        if (string.IsNullOrWhiteSpace(NewOperator.Role))
        {
            ModelState.AddModelError(
                "NewOperator.Role",
                "Station wajib dipilih."
            );
        }

        if (!ModelState.IsValid)
        {
            await LoadOperators();
            return Page();
        }

        // =========================
        // CLEAN DATA
        // =========================

        NewOperator.Username = NewOperator.Username.Trim();
        NewOperator.Password = NewOperator.Password.Trim();
        NewOperator.Name = NewOperator.Name.Trim();
        NewOperator.Role = NewOperator.Role.Trim();

        // =========================
        // VALID ROLE
        // =========================

        string[] validRoles =
        {
            "Building",
            "Curing",
            "Inspection"
        };

        if (!validRoles.Contains(NewOperator.Role))
        {
            ModelState.AddModelError(
                "NewOperator.Role",
                "Station tidak valid."
            );

            await LoadOperators();
            return Page();
        }

        // =========================
        // CHECK USERNAME
        // =========================

        bool usernameExists = await _context.Operators
            .AnyAsync(x => x.Username == NewOperator.Username);

        if (usernameExists)
        {
            ModelState.AddModelError(
                "NewOperator.Username",
                "Username sudah digunakan."
            );

            await LoadOperators();
            return Page();
        }

        // =========================
        // DEFAULT STATUS
        // =========================

        NewOperator.IsActive = true;

        // =========================
        // SAVE DATABASE
        // =========================

        _context.Operators.Add(NewOperator);

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    // =========================
    // DELETE OPERATOR
    // =========================

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var operatorData = await _context.Operators
            .FirstOrDefaultAsync(x => x.Id == DeleteId);

        if (operatorData != null)
        {
            _context.Operators.Remove(operatorData);

            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    // =========================
    // TOGGLE STATUS
    // =========================

    public async Task<IActionResult> OnPostToggleStatusAsync(int id)
    {
        var operatorData = await _context.Operators
            .FirstOrDefaultAsync(x => x.Id == id);

        if (operatorData != null)
        {
            operatorData.IsActive = !operatorData.IsActive;

            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}