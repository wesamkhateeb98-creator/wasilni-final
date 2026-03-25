using System.Security.Cryptography;

namespace SoftPro.Wasilni.Application.Extensions;
public class GenerateCode
{
    public static string Generate8DigitCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        int number = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF; // تأكد أنه موجب
        int code = number % 100_000_000; // حتى يكون من 8 أرقام
        return code.ToString("D8"); // لضبطه إلى 8 خانات دائمًا
    }
}
