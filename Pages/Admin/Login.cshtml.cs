using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TireTraceabilityDemo.Pages.Admin;

public class LoginModel : PageModel
{
    // ==========================================
    // ADMIN LOGIN DUMMY
    // Ganti username/password di sini
    // ==========================================

    private const string AdminUsername = "admin";
    private const string AdminPassword = "123";


    [BindProperty]
    public string Username { get; set; } = string.Empty;


    [BindProperty]
    public string Password { get; set; } = string.Empty;


    public string ErrorMessage { get; set; } = string.Empty;


    public IActionResult OnPost()
    {
        // ==========================================
        // CEK USERNAME DAN PASSWORD
        // ==========================================

        if (Username == AdminUsername && Password == AdminPassword)
        {
            // Login berhasil
            return RedirectToPage("/Admin/Index");
        }


        // Login gagal
        ErrorMessage = "Username atau password salah.";

        return Page();
    }
}