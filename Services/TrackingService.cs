using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Data;

namespace TireTraceabilityDemo.Services;

public class TrackingService
{
    private readonly AppDbContext _context;

    public TrackingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TireExists(string barcode)
    {
        return await _context.Tires
            .AnyAsync(x => x.Barcode == barcode);
    }

    public async Task<object?> GetTracking(string barcode)
    {
        var tire = await _context.Tires
            .FirstOrDefaultAsync(x => x.Barcode == barcode);

        if (tire == null)
            return null;

        var curing = await _context.Curings
            .FirstOrDefaultAsync(x => x.Barcode == barcode);

        var inspection = await _context.Inspections
            .FirstOrDefaultAsync(x => x.Barcode == barcode);

        return new
        {
            Tire = tire,
            Curing = curing,
            Inspection = inspection
        };
    }
}