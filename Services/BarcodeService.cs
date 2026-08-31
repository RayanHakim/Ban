using QRCoder;

namespace TireTraceabilityDemo.Services;

public class BarcodeService
{
    // =========================================================
    // GENERATE UNIQUE BARCODE / TIRE ID
    // =========================================================

    public string GenerateBarcode()
    {
        string date = DateTime.Now.ToString("yyyyMMdd");

        string uniqueCode =
            Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpper();

        return $"TIRE-{date}-{uniqueCode}";
    }


    // =========================================================
    // GENERATE QR CODE IMAGE
    // =========================================================

    public string GenerateQrCode(string text)
    {
        using var qrGenerator = new QRCodeGenerator();

        using var qrCodeData = qrGenerator.CreateQrCode(
            text,
            QRCodeGenerator.ECCLevel.Q
        );

        var pngQrCode = new PngByteQRCode(qrCodeData);

        byte[] qrBytes = pngQrCode.GetGraphic(10);

        return Convert.ToBase64String(qrBytes);
    }
}