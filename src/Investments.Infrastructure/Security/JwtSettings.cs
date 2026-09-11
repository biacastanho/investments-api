namespace Investments.Infrastructure.Security;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "InvestmentsApi";
    public string Audience { get; set; } = "InvestmentsApi";
    public string Secret { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
