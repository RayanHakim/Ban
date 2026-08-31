using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Data;
using TireTraceabilityDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// RAZOR PAGES
// =====================================================

builder.Services.AddRazorPages();

// =====================================================
// SESSION
// =====================================================

// WAJIB untuk menggunakan:
// HttpContext.Session.GetString()
// HttpContext.Session.SetString()

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    // Session aktif selama 8 jam
    options.IdleTimeout = TimeSpan.FromHours(8);

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// =====================================================
// DATABASE
// =====================================================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

// =====================================================
// SERVICES
// =====================================================

builder.Services.AddScoped<BarcodeService>();

builder.Services.AddScoped<TrackingService>();

// Membaca nama komputer/laptop
builder.Services.AddScoped<ComputerService>();

// =====================================================
// BUILD APPLICATION
// =====================================================

var app = builder.Build();

// =====================================================
// HTTP PIPELINE
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    app.UseHsts();
}

// Redirect HTTP -> HTTPS
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// =====================================================
// SESSION
// =====================================================

// Session HARUS diletakkan sebelum Razor Pages
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();