using Amazon;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;

namespace CurtisLawhorn.Auth.Configuration;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCognitoAuth(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind Cognito settings from appSettings.json
        var cognitoConfiguration =
            configuration.GetSection($"AWS:{nameof(CognitoConfiguration)}").Get<CognitoConfiguration>()
            ?? throw new Exception($"{nameof(CognitoConfiguration)} not provided.");

        // Get AWS Options (reads AWS_PROFILE, AWS_REGION, environment variables)
        var awsOptions = configuration.GetAWSOptions();
        var region = awsOptions?.Region ?? ResolveRegionFromEnvironmentOrFallback();

        // Build authority URL dynamically
        string authority = $"https://cognito-idp.{region.SystemName}.amazonaws.com/{cognitoConfiguration.UserPoolId}";

        // Apply authorization globally
        services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });

        // Configure JWT Bearer authentication
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authority,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var tokenUse = context.Principal?.FindFirst("token_use")?.Value;
                        if (!string.Equals(tokenUse, "access", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Fail("Not an access token");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Resolves AWS region from environment variables or defaults to US East 2.
    /// </summary>
    /// <returns>RegionEndpoint</returns>
    private static RegionEndpoint ResolveRegionFromEnvironmentOrFallback()
    {
        var envRegion = Environment.GetEnvironmentVariable("AWS_REGION")
                        ?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION");

        if (!string.IsNullOrEmpty(envRegion))
            return RegionEndpoint.GetBySystemName(envRegion);

        // Default fallback if nothing is set
        return RegionEndpoint.USEast2;
    }
}