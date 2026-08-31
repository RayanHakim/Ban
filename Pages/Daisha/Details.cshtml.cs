using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SkiaSharp;
using System.Text;
using TireTraceabilityDemo.Data;
using TireTraceabilityDemo.Models;

// Alias agar tidak bentrok dengan namespace Pages.Daisha
using DaishaModel = TireTraceabilityDemo.Models.Daisha;

namespace TireTraceabilityDemo.Pages.Daisha;

public class DetailsModel : PageModel
{
    // =========================================================
    // DATABASE
    // =========================================================

    private readonly AppDbContext _context;

    public DetailsModel(AppDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // DAISHA
    // =========================================================

    public DaishaModel? Daisha { get; set; }


    // =========================================================
    // INFORMATION
    // =========================================================

    public string ComputerName { get; set; }
        = "Unknown Computer";

    public string OperatorName { get; set; }
        = "Operator";


    // =========================================================
    // QR CODE
    // =========================================================

    public string QrCode { get; set; }
        = string.Empty;


    // =========================================================
    // GET DETAILS
    // =========================================================

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // -----------------------------------------------------
        // COMPUTER
        // -----------------------------------------------------

        ComputerName =
            Environment.MachineName;


        // -----------------------------------------------------
        // OPERATOR
        // -----------------------------------------------------

        OperatorName =
            HttpContext.Session.GetString(
                "BuildingOperatorName")
            ?? "Operator";


        // -----------------------------------------------------
        // LOAD DAISHA
        // -----------------------------------------------------

        Daisha =
            await LoadDaishaAsync(id);


        // -----------------------------------------------------
        // NOT FOUND
        // -----------------------------------------------------

        if (Daisha == null)
        {
            TempData["Error"] =
                "Data Daisha tidak ditemukan.";

            return Page();
        }


        // -----------------------------------------------------
        // GUNAKAN DATA DARI DATABASE
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            Daisha.ComputerName))
        {
            ComputerName =
                Daisha.ComputerName;
        }


        if (!string.IsNullOrWhiteSpace(
            Daisha.OperatorName))
        {
            OperatorName =
                Daisha.OperatorName;
        }


        // -----------------------------------------------------
        // QR IMAGE ENDPOINT
        // -----------------------------------------------------

        QrCode =
            $"?handler=Qr&id={Daisha.Id}";


        return Page();
    }


    // =========================================================
    // QR IMAGE ENDPOINT
    // =========================================================

    public async Task<IActionResult> OnGetQrAsync(
        int id)
    {
        var daisha =
            await LoadDaishaAsync(id);


        if (daisha == null)
        {
            return NotFound(
                "Daisha tidak ditemukan.");
        }


        byte[] qrBytes =
            GenerateDaishaQrBytes(daisha);


        return File(
            qrBytes,
            "image/png");
    }


    // =========================================================
    // DOWNLOAD QR
    // =========================================================

    public async Task<IActionResult> OnGetDownloadQrAsync(
        int id)
    {
        var daisha =
            await LoadDaishaAsync(id);


        if (daisha == null)
        {
            return NotFound(
                "Daisha tidak ditemukan.");
        }


        byte[] qrBytes =
            GenerateDaishaQrBytes(daisha);


        return File(
            qrBytes,
            "image/png",
            $"{daisha.DaishaCode}_QR.png");
    }


    // =========================================================
    // DOWNLOAD TAG PNG
    // =========================================================

    public async Task<IActionResult> OnGetDownloadTagAsync(
        int id)
    {
        // -----------------------------------------------------
        // LOAD DAISHA
        // -----------------------------------------------------

        var daisha =
            await LoadDaishaAsync(id);


        if (daisha == null)
        {
            return NotFound(
                "Daisha tidak ditemukan.");
        }


        // -----------------------------------------------------
        // INFORMATION
        // -----------------------------------------------------

        string computerName =
            !string.IsNullOrWhiteSpace(
                daisha.ComputerName)
                ? daisha.ComputerName
                : Environment.MachineName;


        string operatorName =
            !string.IsNullOrWhiteSpace(
                daisha.OperatorName)
                ? daisha.OperatorName
                : "Operator";


        // -----------------------------------------------------
        // GENERATE FULL TAG PNG
        // -----------------------------------------------------

        byte[] tagBytes =
            GenerateTagPng(
                daisha,
                computerName,
                operatorName);


        // -----------------------------------------------------
        // DOWNLOAD
        // -----------------------------------------------------

        return File(
            tagBytes,
            "image/png",
            $"{daisha.DaishaCode}_TAG.png");
    }


    // =========================================================
    // LOAD DAISHA
    // =========================================================

    private async Task<DaishaModel?> LoadDaishaAsync(
        int id)
    {
        return await _context.Daishas
            .Include(d => d.DaishaTires)
            .ThenInclude(dt => dt.Tire)
            .FirstOrDefaultAsync(
                d => d.Id == id);
    }


    // =========================================================
    // GENERATE DAISHA QR
    // =========================================================

    private byte[] GenerateDaishaQrBytes(
        DaishaModel daisha)
    {
        string qrContent =
            BuildQrContent(daisha);


        using var qrGenerator =
            new QRCodeGenerator();


        using QRCodeData qrCodeData =
            qrGenerator.CreateQrCode(
                qrContent,
                QRCodeGenerator.ECCLevel.M);


        var pngQrCode =
            new PngByteQRCode(
                qrCodeData);


        return pngQrCode.GetGraphic(12);
    }


    // =========================================================
    // BUILD QR CONTENT
    // =========================================================

    private string BuildQrContent(
        DaishaModel daisha)
    {
        var builder =
            new StringBuilder();


        // -----------------------------------------------------
        // HEADER
        // -----------------------------------------------------

        builder.AppendLine(
            "TIRE TRACEABILITY");


        builder.AppendLine(
            "DAISHA CONTAINER");


        builder.AppendLine();


        // -----------------------------------------------------
        // DAISHA INFORMATION
        // -----------------------------------------------------

        builder.AppendLine(
            $"Daisha ID: {CleanQrValue(
                daisha.DaishaCode)}");


        builder.AppendLine(
            $"Computer: {CleanQrValue(
                daisha.ComputerName)}");


        builder.AppendLine(
            $"Operator: {CleanQrValue(
                daisha.OperatorName)}");


        builder.AppendLine(
            $"Created: {daisha.CreatedAt:dd/MM/yyyy HH:mm}");


        builder.AppendLine(
            $"Total Tire: {daisha.DaishaTires.Count}");


        builder.AppendLine();


        // -----------------------------------------------------
        // TIRE LIST
        // -----------------------------------------------------

        builder.AppendLine(
            "TIRES");


        builder.AppendLine(
            "==============================");


        int number = 1;


        foreach (var relation in
            daisha.DaishaTires
                .OrderBy(x => x.Sequence)
                .ThenBy(x => x.Id))
        {
            var tire =
                relation.Tire;


            if (tire == null)
            {
                continue;
            }


            builder.AppendLine(
                $"Tire {number}");


            builder.AppendLine(
                $"Barcode: {CleanQrValue(
                    tire.Barcode)}");


            builder.AppendLine(
                $"Tire Size: {CleanQrValue(
                    tire.TireSize)}");


            builder.AppendLine(
                $"Machine: {CleanQrValue(
                    tire.MachineNumber)}");


            builder.AppendLine(
                $"Shift: {CleanQrValue(
                    tire.Shift)}");


            builder.AppendLine(
                $"Operator: {CleanQrValue(
                    tire.OperatorId)}");


            builder.AppendLine(
                $"Building: {tire.CreatedAt:dd/MM/yyyy HH:mm}");


            builder.AppendLine();


            number++;
        }


        // -----------------------------------------------------
        // PROCESS
        // -----------------------------------------------------

        builder.AppendLine(
            "Current Process: DAISHA");


        builder.AppendLine(
            "Next Process: CURING");


        return builder.ToString();
    }


    // =========================================================
    // CLEAN QR VALUE
    // =========================================================

    private static string CleanQrValue(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }


        return value
            .Replace("|", "/")
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();
    }


    // =========================================================
    // GENERATE TAG PNG
    // =========================================================

    private byte[] GenerateTagPng(
        DaishaModel daisha,
        string computerName,
        string operatorName)
    {
        // -----------------------------------------------------
        // WIDTH
        // -----------------------------------------------------

        const int width = 900;


        // -----------------------------------------------------
        // JUMLAH TIRE
        // -----------------------------------------------------

        int tireCount =
            daisha.DaishaTires
                .Count(x => x.Tire != null);


        // -----------------------------------------------------
        // UKURAN BARIS TIRE
        // -----------------------------------------------------

        const int tireRowHeight = 135;


        // -----------------------------------------------------
        // HEIGHT DASAR
        // -----------------------------------------------------

        const int baseHeight = 980;


        // -----------------------------------------------------
        // HEIGHT DINAMIS
        // -----------------------------------------------------

        int height =
            baseHeight +
            (tireCount * tireRowHeight);


        // -----------------------------------------------------
        // BITMAP
        // -----------------------------------------------------

        using var bitmap =
            new SKBitmap(
                width,
                height,
                SKColorType.Rgba8888,
                SKAlphaType.Premul);


        using var canvas =
            new SKCanvas(bitmap);


        // =====================================================
        // BACKGROUND
        // =====================================================

        canvas.Clear(
            SKColors.White);


        // =====================================================
        // BORDER PAINT
        // =====================================================

        using var borderPaint =
            new SKPaint
            {
                Color =
                    SKColors.Black,

                Style =
                    SKPaintStyle.Stroke,

                StrokeWidth = 3,

                IsAntialias = true
            };


        // =====================================================
        // THIN LINE PAINT
        // =====================================================

        using var thinLinePaint =
            new SKPaint
            {
                Color =
                    new SKColor(
                        210,
                        210,
                        210),

                Style =
                    SKPaintStyle.Stroke,

                StrokeWidth = 1,

                IsAntialias = true
            };


        // =====================================================
        // NORMAL TEXT PAINT
        // =====================================================

        using var normalTextPaint =
            new SKPaint
            {
                Color =
                    SKColors.Black,

                Style =
                    SKPaintStyle.Fill,

                IsAntialias = true
            };


        // =====================================================
        // GRAY TEXT PAINT
        // =====================================================

        using var grayTextPaint =
            new SKPaint
            {
                Color =
                    new SKColor(
                        95,
                        95,
                        95),

                Style =
                    SKPaintStyle.Fill,

                IsAntialias = true
            };


        // =====================================================
        // FONT
        // =====================================================

        using var fontBold =
            SKTypeface.FromFamilyName(
                "Arial",
                SKFontStyle.Bold);


        using var fontNormal =
            SKTypeface.FromFamilyName(
                "Arial",
                SKFontStyle.Normal);


        using var fontMono =
            SKTypeface.FromFamilyName(
                "Consolas",
                SKFontStyle.Bold);


        // =====================================================
        // OUTER BORDER
        // =====================================================

        canvas.DrawRect(
            new SKRect(
                20,
                20,
                width - 20,
                height - 20),
            borderPaint);


        // =====================================================
        // BRAND
        // =====================================================

        DrawCenteredText(
            canvas,
            "TIRE TRACEABILITY",
            width / 2f,
            70,
            30,
            fontBold,
            normalTextPaint);


        // =====================================================
        // SUBTITLE
        // =====================================================

        DrawCenteredText(
            canvas,
            "DAISHA MATERIAL CONTAINER",
            width / 2f,
            105,
            17,
            fontBold,
            grayTextPaint);


        // =====================================================
        // HEADER LINE
        // =====================================================

        canvas.DrawLine(
            55,
            135,
            width - 55,
            135,
            borderPaint);


        // =====================================================
        // DAISHA ID
        // =====================================================

        DrawCenteredText(
            canvas,
            daisha.DaishaCode,
            width / 2f,
            185,
            38,
            fontMono,
            normalTextPaint);


        // =====================================================
        // QR CODE
        // =====================================================

        byte[] qrBytes =
            GenerateDaishaQrBytes(
                daisha);


        using var qrBitmap =
            SKBitmap.Decode(
                qrBytes);


        if (qrBitmap != null)
        {
            const int qrSize = 300;


            float qrX =
                (width - qrSize) / 2f;


            float qrY =
                210;


            // =================================================
            // SKIASHARP 4.x
            // DrawBitmap overload lama sudah obsolete.
            // Gunakan SKSamplingOptions.
            // =================================================

            var samplingOptions =
                new SKSamplingOptions(
                    SKFilterMode.Nearest,
                    SKMipmapMode.None);


            canvas.DrawBitmap(
                qrBitmap,
                new SKRect(
                    qrX,
                    qrY,
                    qrX + qrSize,
                    qrY + qrSize),
                samplingOptions);
        }


        // =====================================================
        // SCAN TEXT
        // =====================================================

        DrawCenteredText(
            canvas,
            "SCAN UNTUK MELIHAT SELURUH DATA DAISHA",
            width / 2f,
            550,
            15,
            fontBold,
            grayTextPaint);


        // =====================================================
        // DIVIDER
        // =====================================================

        canvas.DrawLine(
            55,
            580,
            width - 55,
            580,
            borderPaint);


        // =====================================================
        // INFORMATION
        // =====================================================

        int y = 620;


        DrawInfoRow(
            canvas,
            "DAISHA ID",
            daisha.DaishaCode,
            ref y,
            width,
            fontBold,
            fontNormal,
            grayTextPaint,
            normalTextPaint,
            thinLinePaint);


        DrawInfoRow(
            canvas,
            "TOTAL TIRE",
            tireCount.ToString(),
            ref y,
            width,
            fontBold,
            fontNormal,
            grayTextPaint,
            normalTextPaint,
            thinLinePaint);


        DrawInfoRow(
            canvas,
            "COMPUTER",
            computerName,
            ref y,
            width,
            fontBold,
            fontNormal,
            grayTextPaint,
            normalTextPaint,
            thinLinePaint);


        DrawInfoRow(
            canvas,
            "OPERATOR",
            operatorName,
            ref y,
            width,
            fontBold,
            fontNormal,
            grayTextPaint,
            normalTextPaint,
            thinLinePaint);


        DrawInfoRow(
            canvas,
            "CREATED",
            daisha.CreatedAt
                .ToString(
                    "dd/MM/yyyy HH:mm"),
            ref y,
            width,
            fontBold,
            fontNormal,
            grayTextPaint,
            normalTextPaint,
            thinLinePaint);


        // =====================================================
        // TIRE SECTION
        // =====================================================

        y += 20;


        canvas.DrawLine(
            55,
            y,
            width - 55,
            y,
            borderPaint);


        y += 35;


        DrawText(
            canvas,
            "TIRES",
            60,
            y,
            19,
            fontBold,
            normalTextPaint);


        y += 25;


        canvas.DrawLine(
            55,
            y,
            width - 55,
            y,
            borderPaint);


        y += 30;


        // =====================================================
        // TIRE LIST
        // =====================================================

        int tireNumber = 1;


        foreach (var relation in
            daisha.DaishaTires
                .OrderBy(x => x.Sequence)
                .ThenBy(x => x.Id))
        {
            var tire =
                relation.Tire;


            if (tire == null)
            {
                continue;
            }


            // -------------------------------------------------
            // NUMBER
            // -------------------------------------------------

            DrawText(
                canvas,
                $"TIRE {tireNumber:D2}",
                60,
                y,
                17,
                fontBold,
                normalTextPaint);


            // -------------------------------------------------
            // BARCODE
            // -------------------------------------------------

            DrawText(
                canvas,
                tire.Barcode,
                60,
                y + 30,
                14,
                fontMono,
                normalTextPaint);


            // -------------------------------------------------
            // SIZE
            // -------------------------------------------------

            DrawText(
                canvas,
                $"SIZE: {tire.TireSize}",
                60,
                y + 58,
                13,
                fontNormal,
                grayTextPaint);


            // -------------------------------------------------
            // MACHINE
            // -------------------------------------------------

            DrawText(
                canvas,
                $"MACHINE: {tire.MachineNumber}",
                300,
                y + 58,
                13,
                fontNormal,
                grayTextPaint);


            // -------------------------------------------------
            // SHIFT
            // -------------------------------------------------

            DrawText(
                canvas,
                $"SHIFT: {tire.Shift}",
                650,
                y + 58,
                13,
                fontNormal,
                grayTextPaint);


            // -------------------------------------------------
            // OPERATOR
            // -------------------------------------------------

            DrawText(
                canvas,
                $"OPERATOR: {tire.OperatorId}",
                300,
                y + 86,
                12,
                fontNormal,
                grayTextPaint);


            // -------------------------------------------------
            // BUILDING
            // -------------------------------------------------

            DrawText(
                canvas,
                $"BUILDING: {tire.CreatedAt:dd/MM/yyyy HH:mm}",
                650,
                y + 86,
                12,
                fontNormal,
                grayTextPaint);


            // -------------------------------------------------
            // DIVIDER
            // -------------------------------------------------

            canvas.DrawLine(
                55,
                y + 105,
                width - 55,
                y + 105,
                thinLinePaint);


            y +=
                tireRowHeight;


            tireNumber++;
        }


        // =====================================================
        // FOOTER
        // =====================================================

        y += 15;


        DrawCenteredText(
            canvas,
            "MATERIAL GROUPING FOR CURING PROCESS",
            width / 2f,
            y,
            13,
            fontBold,
            normalTextPaint);


        y += 25;


        DrawCenteredText(
            canvas,
            "TIRE TRACEABILITY SYSTEM",
            width / 2f,
            y,
            11,
            fontNormal,
            grayTextPaint);


        // =====================================================
        // ENCODE PNG
        // =====================================================

        using var image =
            SKImage.FromBitmap(
                bitmap);


        using var data =
            image.Encode(
                SKEncodedImageFormat.Png,
                100);


        return data.ToArray();
    }


    // =========================================================
    // DRAW TEXT
    // =========================================================

    private static void DrawText(
        SKCanvas canvas,
        string text,
        float x,
        float y,
        float fontSize,
        SKTypeface typeface,
        SKPaint paint)
    {
        using var font =
            new SKFont(
                typeface,
                fontSize);


        canvas.DrawText(
            text ?? string.Empty,
            x,
            y,
            SKTextAlign.Left,
            font,
            paint);
    }


    // =========================================================
    // DRAW CENTERED TEXT
    // =========================================================

    private static void DrawCenteredText(
        SKCanvas canvas,
        string text,
        float centerX,
        float y,
        float fontSize,
        SKTypeface typeface,
        SKPaint paint)
    {
        using var font =
            new SKFont(
                typeface,
                fontSize);


        canvas.DrawText(
            text ?? string.Empty,
            centerX,
            y,
            SKTextAlign.Center,
            font,
            paint);
    }


    // =========================================================
    // DRAW INFORMATION ROW
    // =========================================================

    private static void DrawInfoRow(
        SKCanvas canvas,
        string label,
        string value,
        ref int y,
        int width,
        SKTypeface fontBold,
        SKTypeface fontNormal,
        SKPaint grayPaint,
        SKPaint blackPaint,
        SKPaint linePaint)
    {
        // -----------------------------------------------------
        // LABEL
        // -----------------------------------------------------

        DrawText(
            canvas,
            label,
            60,
            y,
            11,
            fontBold,
            grayPaint);


        // -----------------------------------------------------
        // VALUE
        // -----------------------------------------------------

        string safeValue =
            string.IsNullOrWhiteSpace(value)
                ? "-"
                : value;


        using var font =
            new SKFont(
                fontNormal,
                14);


        float valueWidth =
            font.MeasureText(
                safeValue,
                blackPaint);


        // -----------------------------------------------------
        // MAX WIDTH
        // -----------------------------------------------------

        const float maxWidth =
            500;


        if (valueWidth <= maxWidth)
        {
            DrawText(
                canvas,
                safeValue,
                width - 60 - valueWidth,
                y,
                14,
                fontNormal,
                blackPaint);
        }
        else
        {
            string shortened =
                ShortenText(
                    safeValue,
                    58);


            using var shortenedFont =
                new SKFont(
                    fontNormal,
                    14);


            float shortenedWidth =
                shortenedFont.MeasureText(
                    shortened,
                    blackPaint);


            DrawText(
                canvas,
                shortened,
                width - 60 - shortenedWidth,
                y,
                14,
                fontNormal,
                blackPaint);
        }


        // -----------------------------------------------------
        // LINE
        // -----------------------------------------------------

        canvas.DrawLine(
            55,
            y + 12,
            width - 55,
            y + 12,
            linePaint);


        y += 38;
    }


    // =========================================================
    // SHORTEN TEXT
    // =========================================================

    private static string ShortenText(
        string value,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }


        if (value.Length <= maxLength)
        {
            return value;
        }


        if (maxLength <= 3)
        {
            return value.Substring(
                0,
                maxLength);
        }


        return value.Substring(
            0,
            maxLength - 3)
            + "...";
    }
}