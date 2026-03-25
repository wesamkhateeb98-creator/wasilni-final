using System.ComponentModel.DataAnnotations;

namespace SoftPro.Wasilni.Domain.Options;

public class JwtOption
{
    public static string SectionNanme = "JWT";
    [Required]
    public required string keyJwt { get; set; }
    [Required]
    public required string AudienceJwt { get; set; }
    [Required]
    public required string IssuerJwt { get; set; }
    [Required]
    public required int DurationExpiredInDayJWT { get; set; }
}
