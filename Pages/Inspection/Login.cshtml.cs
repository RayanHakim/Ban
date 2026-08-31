using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TireTraceabilityDemo.Data;

namespace TireTraceabilityDemo.Pages.Inspection;

public class LoginModel : PageModel
{
    private readonly AppDbContext _context;

    public LoginModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        Username = Username.Trim();
        Password = Password.Trim();

        if (string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username dan password harus diisi.";
            return Page();
        }

        var operatorData = _context.Operators.FirstOrDefault(x =>
            x.Username == Username &&
            x.Password == Password &&
            x.Role == "Inspection" &&
            x.IsActive);

        if (operatorData == null)
        {
            ErrorMessage = "Username atau password salah.";
            return Page();
        }

        return RedirectToPage("/Inspection/Index");
    }
}