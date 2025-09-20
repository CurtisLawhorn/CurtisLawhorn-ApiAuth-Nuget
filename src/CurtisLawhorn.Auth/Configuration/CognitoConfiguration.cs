using System.Diagnostics.CodeAnalysis;

namespace CurtisLawhorn.Auth.Configuration;

[ExcludeFromCodeCoverage]
public class CognitoConfiguration
{
    public string UserPoolId { get; set; } = "";
    public string? AppClientId { get; set; }
}